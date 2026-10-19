using Library.Application.Common;
using Library.Application.DTOs.Loans;
using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface ILoanRepository : IRepository<Loan>
{
    /// <summary>השאלות פעילות (שלא הוחזרו) של הקוראת. לבדיקת המכסה.</summary>
    Task<int> CountActiveByUserAsync(int userId, CancellationToken ct);

    /// <summary>קריאה בלבד (AsNoTracking), כולל עותק → ספר, וקוראת.</summary>
    Task<Loan?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>עם tracking, כולל העותק, לצורך החזרה.</summary>
    Task<Loan?> GetForUpdateAsync(int id, CancellationToken ct);

    /// <summary>קריאה בלבד, ההשאלות של קוראת אחת, מהחדשה לישנה.</summary>
    Task<PagedResult<Loan>> GetPagedByUserAsync(int userId, MyLoansQuery query, CancellationToken ct);

    /// <summary>קריאה בלבד, כל ההשאלות, מהחדשה לישנה.</summary>
    Task<PagedResult<Loan>> GetPagedAsync(LoansQuery query, CancellationToken ct);
}
