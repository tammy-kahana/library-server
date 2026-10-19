namespace Library.Domain.Common;

/// <summary>
/// ישות שמוגנת מפני עדכונים מקבילים.
/// ה-DbContext מחליף את Version ל-Guid חדש בכל שמירה של ישות שהשתנתה,
/// ו-EF בודק את הערך הישן בתנאי ה-WHERE.
/// </summary>
public interface IVersioned
{
    Guid Version { get; set; }
}
