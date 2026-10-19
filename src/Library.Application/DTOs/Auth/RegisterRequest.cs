using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Auth;

/// <summary>הרשמה. אין כאן שדה Role: כל נרשמת חדשה היא קוראת.</summary>
public class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
