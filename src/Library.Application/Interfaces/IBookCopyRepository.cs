using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IBookCopyRepository : IRepository<BookCopy>
{
    /// <summary>קריאה בלבד (AsNoTracking).</summary>
    Task<IReadOnlyList<BookCopy>> GetByBookIdAsync(int bookId, CancellationToken ct);

    /// <summary>קריאה בלבד (AsNoTracking).</summary>
    Task<BookCopy?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>עם tracking, לצורך עדכון או מחיקה (Version נבדק בשמירה).</summary>
    Task<BookCopy?> GetForUpdateAsync(int id, CancellationToken ct);

    Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct);

    /// <summary>האם לעותק היו אי פעם השאלות (גם כאלה שהוחזרו).</summary>
    Task<bool> HasLoanHistoryAsync(int copyId, CancellationToken ct);
}
