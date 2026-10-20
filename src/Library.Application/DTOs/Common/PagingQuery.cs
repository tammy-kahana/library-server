using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Common;

/// <summary>פרמטרי עימוד משותפים לכל רשימה. מגיעים מה-query string.</summary>
public class PagingQuery
{
    /// <summary>גבול עליון סביר: מונע חישוב Skip שחורג מגבולות int במספר עמוד עצום.</summary>
    [Range(1, 100_000)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
