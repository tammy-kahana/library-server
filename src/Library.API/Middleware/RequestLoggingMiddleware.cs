using System.Diagnostics;

namespace Library.API.Middleware;

/// <summary>
/// שורת לוג אחת לכל בקשה (Information): method, נתיב, סטטוס ומשך.
/// בכוונה בלי גוף הבקשה ובלי כותרות: אסור שסיסמאות או tokens ייכנסו ללוג.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
            Log(context, context.Response.StatusCode, stopwatch);
        }
        catch (Exception ex)
        {
            // החריגה תהפוך לתשובה ב-ExceptionHandlingMiddleware, שעוטף אותנו מבחוץ. כאן רושמים את אותו סטטוס
            Log(context, ExceptionHandlingMiddleware.StatusCodeFor(ex), stopwatch);
            throw;
        }
    }

    private void Log(HttpContext context, int statusCode, Stopwatch stopwatch) =>
        _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
            context.Request.Method, context.Request.Path, statusCode, stopwatch.ElapsedMilliseconds);
}
