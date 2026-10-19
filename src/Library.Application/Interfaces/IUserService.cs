using Library.Application.Common;
using Library.Application.DTOs.Common;
using Library.Application.DTOs.Users;

namespace Library.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<UserResponse>> GetPagedAsync(PagingQuery query, CancellationToken ct);
    Task<Result> ChangeRoleAsync(int id, ChangeRoleRequest request, int currentUserId, CancellationToken ct);
}
