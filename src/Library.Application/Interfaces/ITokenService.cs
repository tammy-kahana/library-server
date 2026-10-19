using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);

/// <summary>יצירת JWT. המימוש ב-Infrastructure, כך ש-Application לא תלוי בספריית JWT.</summary>
public interface ITokenService
{
    AccessToken CreateToken(User user);
}
