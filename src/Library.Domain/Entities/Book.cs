namespace Library.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublishedYear { get; set; }

    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
