namespace Library.Application.Common;

/// <summary>
/// מישהו אחר שינה את הנתון בין הקריאה לשמירה.
/// ה-Repository מתרגם אליה את DbUpdateConcurrencyException של EF,
/// כך ש-Application לא מכיר את EF, וב-Moq קל לדמות התנגשות.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
