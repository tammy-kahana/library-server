using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Categories;

/// <summary>משמש גם ליצירה וגם לעדכון (אותם שדות בדיוק).</summary>
public class CategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
