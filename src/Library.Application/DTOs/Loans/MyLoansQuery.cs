using Library.Application.DTOs.Common;

namespace Library.Application.DTOs.Loans;

/// <summary>GET /api/loans/me?page=&pageSize=&activeOnly=</summary>
public class MyLoansQuery : PagingQuery
{
    public bool ActiveOnly { get; set; }
}
