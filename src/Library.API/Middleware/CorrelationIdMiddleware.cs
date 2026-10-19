namespace Library.API.Middleware;

/// <summary>
/// מזהה ייחודי לכל בקשה: נלקח מהכותרת X-Correlation-Id אם הלקוח שלח, אחרת נוצר חדש.
/// המזהה מוחזר בכותרת התשובה, נשמר ב-TraceIdentifier, ונכנס ל-scope של הלוג,
/// כך שכל שורת לוג של הבקשה הזו נושאת אותו.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetValidHeaderValue(context) ?? Guid.NewGuid().ToString();

        context.TraceIdentifier = correlationId;

        // OnStarting: הכותרת נוספת ממש לפני שהתשובה נשלחת, כך שהיא שורדת גם Response.Clear() בשגיאה
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// מקבלים מזהה מהלקוח רק אם הוא קצר ומכיל אותיות, ספרות ומקפים,
    /// כדי שאי אפשר יהיה להזריק לקובץ הלוג טקסט מזויף.
    /// </summary>
    private static string? GetValidHeaderValue(HttpContext context)
    {
        var value = context.Request.Headers[HeaderName].ToString();

        return value.Length is > 0 and <= 64 && value.All(c => char.IsLetterOrDigit(c) || c == '-')
            ? value
            : null;
    }
}
