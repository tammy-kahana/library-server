using Library.Application.DTOs.Categories;

namespace Library.Application.DTOs.Books;

public class BookResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Isbn { get; init; } = string.Empty;
    public int PublishedYear { get; init; }
    public List<CategoryResponse> Categories { get; init; } = new();
    public int TotalCopies { get; init; }
    public int AvailableCopies { get; init; }
}
