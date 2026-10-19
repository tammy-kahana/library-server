using Library.Application.DTOs.Common;

namespace Library.Application.DTOs.Loans;

/// <summary>GET /api/loans?page=&pageSize=&overdue= (ספרנית ומנהלת)</summary>
public class LoansQuery : PagingQuery
{
    /// <summary>רק השאלות פעילות שתאריך ההחזרה שלהן עבר.</summary>
    public bool Overdue { get; set; }
}
