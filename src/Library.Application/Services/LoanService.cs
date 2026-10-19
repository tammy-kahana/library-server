using AutoMapper;
using Library.Application.Common;
using Library.Application.DTOs.Loans;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Library.Application.Services;

/// <summary>
/// השאלה והחזרה של עותקים: המשאב המוגבל של המערכת.
/// ההגנה מפני התנגשות: הבדיקות (עותק פנוי, מכסה) והשמירה רצות מול אותן ישויות,
/// ובשמירה EF בודק שה-Version של העותק ושל הקוראת לא השתנה מאז הקריאה.
/// אם השתנה, מישהי הקדימה אותנו: ה-Repository זורק ConcurrencyConflictException ומחזירים 409.
/// </summary>
public class LoanService : ILoanService
{
    private const string LoanNotFound = "ההשאלה לא נמצאה";

    private readonly ILoanRepository _loans;
    private readonly IBookCopyRepository _copies;
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;
    private readonly LoanRulesOptions _rules;
    private readonly ILogger<LoanService> _logger;

    public LoanService(
        ILoanRepository loans,
        IBookCopyRepository copies,
        IUserRepository users,
        IMapper mapper,
        IOptions<LoanRulesOptions> rules,
        ILogger<LoanService> logger)
    {
        _loans = loans;
        _copies = copies;
        _users = users;
        _mapper = mapper;
        _rules = rules.Value;
        _logger = logger;
    }

    public async Task<Result<LoanResponse>> BorrowAsync(int copyId, int userId, CancellationToken ct)
    {
        // 1. העותק, עם tracking: ה-Version שנקרא כאן ייבדק בשמירה
        var copy = await _copies.GetForUpdateAsync(copyId, ct);
        if (copy is null)
        {
            return Error.NotFound(ErrorMessages.CopyNotFound);
        }

        if (copy.Status != CopyStatus.Available)
        {
            _logger.LogWarning("Borrow rejected: copy {CopyId} is {Status}", copyId, copy.Status);
            return Error.Conflict(copy.Status == CopyStatus.Loaned ? "העותק כבר מושאל" : "העותק אינו זמין להשאלה");
        }

        // 2. הקוראת, עם tracking: גם ה-Version שלה ייבדק בשמירה (הגנה על המכסה)
        var user = await _users.GetForUpdateAsync(userId, ct);
        if (user is null)
        {
            return Error.NotFound(ErrorMessages.UserNotFound);
        }

        var activeLoans = await _loans.CountActiveByUserAsync(userId, ct);
        if (activeLoans >= _rules.MaxActiveLoans)
        {
            _logger.LogWarning("Borrow rejected: user {UserId} reached the limit of {MaxActiveLoans} active loans",
                userId, _rules.MaxActiveLoans);
            return Error.Conflict(
                $"לא ניתן להשאיל: הגעת למכסת ההשאלות ({_rules.MaxActiveLoans} ספרים). יש להחזיר ספר כדי להשאיל חדש.");
        }

        // 3. השינויים: העותק מושאל, נוצרת השאלה, והקוראת מסומנת כדי שה-Version שלה יתחלף
        var now = DateTime.UtcNow;
        copy.Status = CopyStatus.Loaned;
        var loan = new Loan
        {
            BookCopyId = copy.Id,
            UserId = user.Id,
            LoanedAtUtc = now,
            DueAtUtc = now.AddDays(_rules.LoanPeriodDays)
        };
        _loans.Add(loan);
        _users.MarkAsChanged(user);

        // 4. שמירה אחת = טרנזקציה אחת: העותק, הקוראת וההשאלה נשמרים יחד, או ששום דבר לא נשמר
        var saveResult = await SaveAsync("העותק נתפס על ידי משתמשת אחרת. נסי שוב.", ct);
        if (!saveResult.IsSuccess)
        {
            return saveResult.Error!;
        }

        _logger.LogInformation("Copy {CopyId} loaned to user {UserId} (loan {LoanId})", copyId, userId, loan.Id);

        // טוענים מחדש עם העותק, הספר והקוראת, כדי להחזיר בדיוק את מה ש-GET מחזיר
        var created = await _loans.GetByIdAsync(loan.Id, ct);
        return _mapper.Map<LoanResponse>(created!);
    }

    public async Task<Result> ReturnAsync(int loanId, int userId, bool isStaff, CancellationToken ct)
    {
        var loan = await _loans.GetForUpdateAsync(loanId, ct);
        if (loan is null)
        {
            return Error.NotFound(LoanNotFound);
        }

        if (!isStaff && loan.UserId != userId)
        {
            return Error.Forbidden("אפשר להחזיר רק השאלה שלך");
        }

        if (loan.ReturnedAtUtc is not null)
        {
            return Error.Conflict("ההשאלה כבר הוחזרה");
        }

        loan.ReturnedAtUtc = DateTime.UtcNow;
        loan.BookCopy.Status = CopyStatus.Available;

        // שתי החזרות באותו רגע: שתיהן משנות את העותק, ה-Version שלו תופס את השנייה
        var saveResult = await SaveAsync("ההשאלה עודכנה במקביל. יש לרענן ולנסות שוב.", ct);
        if (saveResult.IsSuccess)
        {
            _logger.LogInformation("Loan {LoanId} returned (copy {CopyId})", loanId, loan.BookCopyId);
        }

        return saveResult;
    }

    public async Task<Result<LoanResponse>> GetByIdAsync(int loanId, int userId, bool isStaff, CancellationToken ct)
    {
        var loan = await _loans.GetByIdAsync(loanId, ct);
        if (loan is null)
        {
            return Error.NotFound(LoanNotFound);
        }

        if (!isStaff && loan.UserId != userId)
        {
            return Error.Forbidden("אפשר לצפות רק בהשאלות שלך");
        }

        return _mapper.Map<LoanResponse>(loan);
    }

    public async Task<PagedResult<LoanResponse>> GetMyLoansAsync(int userId, MyLoansQuery query, CancellationToken ct)
    {
        var page = await _loans.GetPagedByUserAsync(userId, query, ct);
        return page.Map(items => _mapper.Map<List<LoanResponse>>(items));
    }

    public async Task<PagedResult<LoanResponse>> GetAllAsync(LoansQuery query, CancellationToken ct)
    {
        var page = await _loans.GetPagedAsync(query, ct);
        return page.Map(items => _mapper.Map<List<LoanResponse>>(items));
    }

    /// <summary>
    /// שמירה של כל השינויים. התנגשות → 409 עם ההודעה שנמסרה.
    /// ה-Warning ללוג כבר נכתב ב-RepositoryBase, ולכן לא כותבים אותו שוב כאן.
    /// </summary>
    private async Task<Result> SaveAsync(string conflictMessage, CancellationToken ct)
    {
        try
        {
            await _loans.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict(conflictMessage);
        }
    }
}
