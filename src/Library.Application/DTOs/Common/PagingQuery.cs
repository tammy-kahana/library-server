using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Common;

/// <summary>פרמטרי עימוד משותפים לכל רשימה. מגיעים מה-query string.</summary>
public class PagingQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
