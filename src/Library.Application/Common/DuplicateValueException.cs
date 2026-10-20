namespace Library.Application.Common;

/// <summary>
/// בסיס הנתונים דחה ערך שחייב להיות ייחודי (ISBN, ברקוד, שם קטגוריה, מייל).
/// קורה רק במרוץ: שתי בקשות עברו יחד את בדיקת "האם כבר קיים?" ב-Service.
/// ה-Repository מתרגם אליה את שגיאת בסיס הנתונים, וה-Middleware מחזיר 409.
/// </summary>
public class DuplicateValueException : Exception
{
    public DuplicateValueException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
