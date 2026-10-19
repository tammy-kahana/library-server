using System.ComponentModel.DataAnnotations;
using Library.Domain.Enums;

namespace Library.Application.DTOs.Users;

public class ChangeRoleRequest
{
    /// <summary>EnumDataType דוחה ערך שלא מוגדר ב-Enum, כולל 0.</summary>
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }
}
