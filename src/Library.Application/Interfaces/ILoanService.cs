using Library.Application.Common;
using Library.Application.DTOs.Loans;

namespace Library.Application.Interfaces;

/// <summary>
/// isStaff: האם המבקשת ספרנית או מנהלת. נקבע ב-Controller לפי ה-token.
/// קוראת רואה ומחזירה רק השאלות שלה; צוות רואה ומחזיר כל השאלה.
/// </summary>
public interface ILoanService
{
    Task<Result<LoanResponse>> BorrowAsync(int copyId, int userId, CancellationToken ct);
    Task<Result> ReturnAsync(int loanId, int userId, bool isStaff, CancellationToken ct);
    Task<Result<LoanResponse>> GetByIdAsync(int loanId, int userId, bool isStaff, CancellationToken ct);
    Task<PagedResult<LoanResponse>> GetMyLoansAsync(int userId, MyLoansQuery query, CancellationToken ct);
    Task<PagedResult<LoanResponse>> GetAllAsync(LoansQuery query, CancellationToken ct);
}
