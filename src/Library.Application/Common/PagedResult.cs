namespace Library.Application.Common;

/// <summary>מבנה אחיד לכל רשימה עם עימוד. ה-Skip/Take עצמו רץ בתוך השאילתה ב-Repository.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>ממיר את הפריטים (למשל ישויות ל-DTOs) ומשאיר את נתוני העימוד כמו שהם.</summary>
    public PagedResult<TOut> Map<TOut>(Func<IReadOnlyList<T>, IReadOnlyList<TOut>> map) =>
        new(map(Items), Page, PageSize, TotalCount);
}
