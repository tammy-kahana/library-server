using Microsoft.AspNetCore.Mvc;

namespace Library.API.Middleware;

/// <summary>
/// ראשון ב-pipeline: רשת ביטחון לחריגות שאף אחד לא תפס (באגים, בסיס נתונים שלא זמין).
/// כותב ללוג ברמת Error ומחזיר 500 בפורמט ה-JSON האחיד (ProblemDetails).
/// זה המקום היחיד שתופס Exception כללי: שגיאות עסקיות לא מגיעות לכאן, הן חוזרות כ-Result.
/// פרטי החריגה לא נשלחים ללקוח (גם לא בפיתוח), רק ללוג.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // הלקוח סגר את החיבור באמצע. זו לא שגיאה של השרת, ואין למי להחזיר תשובה
            _logger.LogInformation("Request was cancelled by the client (CorrelationId {CorrelationId})", context.TraceIdentifier);
        }
        catch (Exception ex)
        {
            // TraceIdentifier הוא ה-CorrelationId (נקבע ב-CorrelationIdMiddleware), ולכן גם לוג השגיאה מכיל אותו
            _logger.LogError(ex, "Unhandled exception (CorrelationId {CorrelationId})", context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "אירעה שגיאה בלתי צפויה. אם הבעיה חוזרת, יש לפנות לתמיכה עם מזהה הבקשה."
            };
            problem.Extensions["correlationId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }
}
