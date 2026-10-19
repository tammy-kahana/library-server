using Library.Application.DTOs.Common;

namespace Library.Application.DTOs.Books;

public enum BookSortBy
{
    Title,
    Author,
    PublishedYear
}

/// <summary>חיפוש, סינון ומיון ברשימת הספרים: GET /api/books?page=&pageSize=&search=&categoryId=&sortBy=&desc=</summary>
public class BookQuery : PagingQuery
{
    /// <summary>חיפוש בכותרת או בשם המחבר.</summary>
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public BookSortBy SortBy { get; set; } = BookSortBy.Title;

    public bool Desc { get; set; }
}
