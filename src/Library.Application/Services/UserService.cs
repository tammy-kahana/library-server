using AutoMapper;
using Library.Application.Common;
using Library.Application.DTOs.Common;
using Library.Application.DTOs.Users;
using Library.Application.Interfaces;

namespace Library.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;

    public UserService(IUserRepository users, IMapper mapper)
    {
        _users = users;
        _mapper = mapper;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null)
        {
            return Error.NotFound(ErrorMessages.UserNotFound);
        }

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<PagedResult<UserResponse>> GetPagedAsync(PagingQuery query, CancellationToken ct)
    {
        var page = await _users.GetPagedAsync(query, ct);
        return page.Map(items => _mapper.Map<List<UserResponse>>(items));
    }

    /// <summary>
    /// שינוי תפקיד (מנהלת בלבד). מנהלת לא משנה את התפקיד של עצמה, כדי שהמערכת לא תישאר בלי מנהלת.
    /// התפקיד החדש נכנס לתוקף בהתחברות הבאה של המשתמשת, כי ה-token הקיים שלה עדיין מחזיק את התפקיד הקודם.
    /// </summary>
    public async Task<Result> ChangeRoleAsync(int id, ChangeRoleRequest request, int currentUserId, CancellationToken ct)
    {
        if (id == currentUserId)
        {
            return Error.Conflict("לא ניתן לשנות את התפקיד של עצמך");
        }

        var user = await _users.GetForUpdateAsync(id, ct);
        if (user is null)
        {
            return Error.NotFound(ErrorMessages.UserNotFound);
        }

        user.Role = request.Role;

        // User מוגנת ב-Version: אם היא השאילה ספר באותו רגע, אחת הפעולות תקבל התנגשות
        try
        {
            await _users.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict("המשתמשת עודכנה במקביל. יש לרענן ולנסות שוב.");
        }
    }
}
