using System.Linq.Expressions;
using Library.Application.Common;
using Library.Application.DTOs.Books;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Repositories;

public class BookRepository : RepositoryBase<Book>, IBookRepository
{
    public BookRepository(AppDbContext db, ILogger<BookRepository> logger) : base(db, logger)
    {
    }

    public async Task<PagedResult<Book>> GetPagedAsync(BookQuery query, CancellationToken ct)
    {
        var books = Db.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            books = books.Where(b => b.Title.ToLower().Contains(search) || b.Author.ToLower().Contains(search));
        }

        if (query.CategoryId is int categoryId)
        {
            books = books.Where(b => b.Categories.Any(c => c.Id == categoryId));
        }

        return await ToPagedResultAsync(WithDetails(Sort(books, query.SortBy, query.Desc)), query, ct);
    }

    public Task<Book?> GetByIdAsync(int id, CancellationToken ct) =>
        WithDetails(Db.Books.AsNoTracking()).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<Book?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.Books.Include(b => b.Categories).FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        Db.Books.AnyAsync(b => b.Id == id, ct);

    public Task<bool> IsbnExistsAsync(string isbn, int? excludeBookId, CancellationToken ct) =>
        Db.Books.AnyAsync(b => b.Isbn == isbn && b.Id != excludeBookId, ct);

    public Task<bool> HasLoanHistoryAsync(int bookId, CancellationToken ct) =>
        Db.Loans.AnyAsync(l => l.BookCopy.BookId == bookId, ct);

    /// <summary>
    /// טעינת הקטגוריות והעותקים יחד עם הספרים (בלי N+1).
    /// AsSplitQuery: שאילתה נפרדת לכל אוסף, כדי שספר לא יחזור פעם לכל צירוף של קטגוריה ועותק.
    /// </summary>
    private static IQueryable<Book> WithDetails(IQueryable<Book> books) =>
        books.Include(b => b.Categories)
             .Include(b => b.Copies)
             .AsSplitQuery();

    /// <summary>מיון לפי בחירת המשתמשת, ואחריו תמיד לפי Id, כדי שהעימוד יהיה יציב.</summary>
    private static IQueryable<Book> Sort(IQueryable<Book> books, BookSortBy sortBy, bool desc)
    {
        var ordered = sortBy switch
        {
            BookSortBy.Author => OrderBy(books, b => b.Author, desc),
            BookSortBy.PublishedYear => OrderBy(books, b => b.PublishedYear, desc),
            _ => OrderBy(books, b => b.Title, desc)
        };

        return ordered.ThenBy(b => b.Id);
    }

    private static IOrderedQueryable<Book> OrderBy<TKey>(IQueryable<Book> books, Expression<Func<Book, TKey>> key, bool desc) =>
        desc ? books.OrderByDescending(key) : books.OrderBy(key);
}
