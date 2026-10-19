using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Books;

/// <summary>משמש גם ליצירה וגם לעדכון (PUT מחליף את כל השדות).</summary>
public class BookRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Isbn { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int PublishedYear { get; set; }

    /// <summary>הקטגוריות של הספר. קיומן נבדק ב-Service (ולידציה עסקית).</summary>
    public List<int> CategoryIds { get; set; } = new();
}
