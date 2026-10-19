using AutoMapper;
using Library.Application.Common;
using Library.Application.DTOs.Books;
using Library.Application.Interfaces;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class BookService : IBookService
{
    private const string IsbnTaken = "כבר קיים ספר עם ה-ISBN הזה";

    private readonly IBookRepository _books;
    private readonly ICategoryRepository _categories;
    private readonly IMapper _mapper;

    public BookService(IBookRepository books, ICategoryRepository categories, IMapper mapper)
    {
        _books = books;
        _categories = categories;
        _mapper = mapper;
    }

    public async Task<PagedResult<BookResponse>> GetPagedAsync(BookQuery query, CancellationToken ct)
    {
        var page = await _books.GetPagedAsync(query, ct);
        return page.Map(items => _mapper.Map<List<BookResponse>>(items));
    }

    public async Task<Result<BookResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var book = await _books.GetByIdAsync(id, ct);
        if (book is null)
        {
            return Error.NotFound(ErrorMessages.BookNotFound);
        }

        return _mapper.Map<BookResponse>(book);
    }

    public async Task<Result<BookResponse>> CreateAsync(BookRequest request, CancellationToken ct)
    {
        if (await _books.IsbnExistsAsync(request.Isbn, excludeBookId: null, ct))
        {
            return Error.Conflict(IsbnTaken);
        }

        var book = _mapper.Map<Book>(request);

        var categoriesResult = await SetCategoriesAsync(book, request.CategoryIds, ct);
        if (!categoriesResult.IsSuccess)
        {
            return categoriesResult.Error!;
        }

        _books.Add(book);
        await _books.SaveChangesAsync(ct);

        return _mapper.Map<BookResponse>(book);
    }

    public async Task<Result> UpdateAsync(int id, BookRequest request, CancellationToken ct)
    {
        var book = await _books.GetForUpdateAsync(id, ct);
        if (book is null)
        {
            return Error.NotFound(ErrorMessages.BookNotFound);
        }

        if (await _books.IsbnExistsAsync(request.Isbn, excludeBookId: id, ct))
        {
            return Error.Conflict(IsbnTaken);
        }

        _mapper.Map(request, book);

        var categoriesResult = await SetCategoriesAsync(book, request.CategoryIds, ct);
        if (!categoriesResult.IsSuccess)
        {
            return categoriesResult;
        }

        await _books.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var book = await _books.GetForUpdateAsync(id, ct);
        if (book is null)
        {
            return Error.NotFound(ErrorMessages.BookNotFound);
        }

        // היסטוריית ההשאלות מוגנת ב-Restrict, ולכן ספר שהושאל אי פעם לא נמחק.
        // עותק שנהרס או אבד מסמנים כ-Lost (PATCH /api/copies/{id}).
        if (await _books.HasLoanHistoryAsync(id, ct))
        {
            return Error.Conflict("לא ניתן למחוק ספר שיש לו היסטוריית השאלות. ניתן לסמן את העותקים שלו כ-Lost.");
        }

        _books.Remove(book);
        await _books.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>
    /// טוען את הקטגוריות לפי המזהים ומחליף בהן את הקטגוריות של הספר.
    /// ולידציה עסקית: כל המזהים חייבים להתקיים בבסיס הנתונים.
    /// </summary>
    private async Task<Result> SetCategoriesAsync(Book book, List<int> categoryIds, CancellationToken ct)
    {
        var distinctIds = categoryIds.Distinct().ToList();
        var categories = await _categories.GetForUpdateByIdsAsync(distinctIds, ct);

        if (categories.Count != distinctIds.Count)
        {
            return Error.Validation("אחת או יותר מהקטגוריות שנבחרו לא קיימת");
        }

        book.Categories.Clear();
        foreach (var category in categories)
        {
            book.Categories.Add(category);
        }

        return Result.Success();
    }
}
