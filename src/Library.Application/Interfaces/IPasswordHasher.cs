namespace Library.Application.Interfaces;

/// <summary>
/// הצפנת סיסמאות. המימוש (BCrypt) יושב ב-Infrastructure,
/// כך ש-Application לא תלוי בספרייה חיצונית.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
