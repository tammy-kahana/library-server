using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Repositories;

public class BookCopyRepository : RepositoryBase<BookCopy>, IBookCopyRepository
{
    public BookCopyRepository(AppDbContext db, ILogger<BookCopyRepository> logger) : base(db, logger)
    {
    }

    public async Task<IReadOnlyList<BookCopy>> GetByBookIdAsync(int bookId, CancellationToken ct) =>
        await Db.BookCopies.AsNoTracking()
                           .Where(c => c.BookId == bookId)
                           .OrderBy(c => c.Barcode)
                           .ToListAsync(ct);

    public Task<BookCopy?> GetByIdAsync(int id, CancellationToken ct) =>
        Db.BookCopies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<BookCopy?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.BookCopies.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> BarcodeExistsAsync(string barcode, CancellationToken ct) =>
        Db.BookCopies.AnyAsync(c => c.Barcode == barcode, ct);

    public Task<bool> HasLoanHistoryAsync(int copyId, CancellationToken ct) =>
        Db.Loans.AnyAsync(l => l.BookCopyId == copyId, ct);
}
