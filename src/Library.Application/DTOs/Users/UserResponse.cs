using Library.Domain.Enums;

namespace Library.Application.DTOs.Users;

/// <summary>פרטי משתמשת. בלי PasswordHash ובלי Version: אלה לא יוצאים מהשרת.</summary>
public class UserResponse
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
