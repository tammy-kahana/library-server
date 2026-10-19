using AutoMapper;
using Library.Application.Common;
using Library.Application.DTOs.Copies;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services;

public class CopyService : ICopyService
{

    private readonly IBookCopyRepository _copies;
    private readonly IBookRepository _books;
    private readonly IMapper _mapper;

    public CopyService(IBookCopyRepository copies, IBookRepository books, IMapper mapper)
    {
        _copies = copies;
        _books = books;
        _mapper = mapper;
    }

    public async Task<Result<List<CopyResponse>>> GetByBookIdAsync(int bookId, CancellationToken ct)
    {
        if (!await _books.ExistsAsync(bookId, ct))
        {
            return Error.NotFound(ErrorMessages.BookNotFound);
        }

        var copies = await _copies.GetByBookIdAsync(bookId, ct);
        return _mapper.Map<List<CopyResponse>>(copies);
    }

    public async Task<Result<CopyResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var copy = await _copies.GetByIdAsync(id, ct);
        if (copy is null)
        {
            return Error.NotFound(ErrorMessages.CopyNotFound);
        }

        return _mapper.Map<CopyResponse>(copy);
    }

    public async Task<Result<CopyResponse>> AddAsync(int bookId, CopyRequest request, CancellationToken ct)
    {
        if (!await _books.ExistsAsync(bookId, ct))
        {
            return Error.NotFound(ErrorMessages.BookNotFound);
        }

        if (await _copies.BarcodeExistsAsync(request.Barcode, ct))
        {
            return Error.Conflict("כבר קיים עותק עם הברקוד הזה");
        }

        var copy = _mapper.Map<BookCopy>(request);
        copy.BookId = bookId;

        _copies.Add(copy);
        await _copies.SaveChangesAsync(ct);

        return _mapper.Map<CopyResponse>(copy);
    }

    public async Task<Result> ChangeStatusAsync(int id, CopyStatusRequest request, CancellationToken ct)
    {
        if (request.Status == CopyStatus.Loaned)
        {
            return Error.Validation("השאלה נעשית דרך /api/loans ולא בשינוי סטטוס");
        }

        var copy = await _copies.GetForUpdateAsync(id, ct);
        if (copy is null)
        {
            return Error.NotFound(ErrorMessages.CopyNotFound);
        }

        if (copy.Status == CopyStatus.Loaned)
        {
            return Error.Conflict("העותק מושאל כרגע. יש להחזיר אותו לפני שינוי הסטטוס.");
        }

        copy.Status = request.Status;
        return await SaveCopyChangesAsync(ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var copy = await _copies.GetForUpdateAsync(id, ct);
        if (copy is null)
        {
            return Error.NotFound(ErrorMessages.CopyNotFound);
        }

        if (await _copies.HasLoanHistoryAsync(id, ct))
        {
            return Error.Conflict("לא ניתן למחוק עותק שיש לו היסטוריית השאלות. ניתן לסמן אותו כ-Lost.");
        }

        _copies.Remove(copy);
        return await SaveCopyChangesAsync(ct);
    }

    /// <summary>
    /// שמירה של שינוי בעותק קיים. אם מישהי השאילה או שינתה את העותק בין הקריאה לשמירה,
    /// ה-Version כבר לא תואם, ה-Repository זורק ConcurrencyConflictException, ומחזירים 409.
    /// </summary>
    private async Task<Result> SaveCopyChangesAsync(CancellationToken ct)
    {
        try
        {
            await _copies.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict("העותק שונה על ידי משתמשת אחרת. יש לרענן ולנסות שוב.");
        }
    }
}
