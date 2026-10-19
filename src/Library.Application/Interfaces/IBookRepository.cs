using Library.Application.Common;
using Library.Application.DTOs.Books;
using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    /// <summary>קריאה בלבד (AsNoTracking), כולל קטגוריות ועותקים. סינון, מיון ו-Skip/Take בתוך השאילתה.</summary>
    Task<PagedResult<Book>> GetPagedAsync(BookQuery query, CancellationToken ct);

    /// <summary>קריאה בלבד (AsNoTracking), כולל קטגוריות ועותקים.</summary>
    Task<Book?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>עם tracking, כולל קטגוריות, לצורך עדכון או מחיקה.</summary>
    Task<Book?> GetForUpdateAsync(int id, CancellationToken ct);

    Task<bool> ExistsAsync(int id, CancellationToken ct);

    Task<bool> IsbnExistsAsync(string isbn, int? excludeBookId, CancellationToken ct);

    /// <summary>האם לאחד העותקים של הספר היו אי פעם השאלות (גם כאלה שהוחזרו).</summary>
    Task<bool> HasLoanHistoryAsync(int bookId, CancellationToken ct);
}
