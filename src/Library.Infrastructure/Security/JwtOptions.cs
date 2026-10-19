using System.ComponentModel.DataAnnotations;

namespace Library.Infrastructure.Security;

/// <summary>
/// הגדרות JWT מהקונפיגורציה (סעיף "Jwt").
/// Issuer, Audience ו-ExpiryMinutes ב-appsettings.json. Key הוא סוד: User Secrets בפיתוח, משתנה סביבה בענן.
/// הולידציה רצה בעליית השרת (ValidateOnStart), כך שמפתח חסר או קצר מונע מהשרת לעלות.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>לפחות 32 תווים: HMAC-SHA256 דורש מפתח של 256 ביט לפחות.</summary>
    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
