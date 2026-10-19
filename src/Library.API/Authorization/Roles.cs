using Library.Domain.Enums;

namespace Library.API.Authorization;

/// <summary>
/// שמות התפקידים ל-[Authorize(Roles = ...)], נגזרים מה-Enum ולא כתובים כמחרוזות חופשיות,
/// כך ששינוי שם ב-Enum לא ישאיר הרשאה שבורה בשקט.
/// </summary>
public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Librarian = nameof(UserRole.Librarian);
    public const string Reader = nameof(UserRole.Reader);

    public const string LibrarianOrAdmin = Librarian + "," + Admin;
}
