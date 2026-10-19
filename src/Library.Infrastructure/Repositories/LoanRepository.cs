using Library.Application.Common;
using Library.Application.DTOs.Loans;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Repositories;

public class LoanRepository : RepositoryBase<Loan>, ILoanRepository
{
    public LoanRepository(AppDbContext db, ILogger<LoanRepository> logger) : base(db, logger)
    {
    }

    public Task<int> CountActiveByUserAsync(int userId, CancellationToken ct) =>
        Db.Loans.CountAsync(l => l.UserId == userId && l.ReturnedAtUtc == null, ct);

    public Task<Loan?> GetByIdAsync(int id, CancellationToken ct) =>
        WithDetails(Db.Loans.AsNoTracking()).FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<Loan?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.Loans.Include(l => l.BookCopy).FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<PagedResult<Loan>> GetPagedByUserAsync(int userId, MyLoansQuery query, CancellationToken ct)
    {
        var loans = Db.Loans.AsNoTracking().Where(l => l.UserId == userId);

        if (query.ActiveOnly)
        {
            loans = loans.Where(l => l.ReturnedAtUtc == null);
        }

        return ToPagedResultAsync(NewestFirst(WithDetails(loans)), query, ct);
    }

    public Task<PagedResult<Loan>> GetPagedAsync(LoansQuery query, CancellationToken ct)
    {
        var loans = Db.Loans.AsNoTracking();

        if (query.Overdue)
        {
            // אותה הגדרה של "באיחור" כמו IsOverdue ב-LoanProfile
            var now = DateTime.UtcNow;
            loans = loans.Where(l => l.ReturnedAtUtc == null && l.DueAtUtc < now);
        }

        return ToPagedResultAsync(NewestFirst(WithDetails(loans)), query, ct);
    }

    /// <summary>השאלה → עותק → ספר (Include/ThenInclude), וגם הקוראת. שאילתה אחת, בלי N+1.</summary>
    private static IQueryable<Loan> WithDetails(IQueryable<Loan> loans) =>
        loans.Include(l => l.BookCopy).ThenInclude(c => c.Book)
             .Include(l => l.User);

    private static IQueryable<Loan> NewestFirst(IQueryable<Loan> loans) =>
        loans.OrderByDescending(l => l.LoanedAtUtc).ThenByDescending(l => l.Id);
}
