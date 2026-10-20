using Library.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Middleware;

/// <summary>
/// ראשון ב-pipeline: רשת ביטחון לחריגות שאף אחד לא תפס (באגים, בסיס נתונים שלא זמין).
/// כותב ללוג ברמת Error ומחזיר 500 בפורמט ה-JSON האחיד (ProblemDetails).
/// זה המקום היחיד שתופס Exception כללי: שגיאות עסקיות לא מגיעות לכאן, הן חוזרות כ-Result.
/// יוצא מן הכלל: DuplicateValueException (מרוץ נדיר על ערך ייחודי) מתורגם ל-409.
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
        catch (DuplicateValueException ex)
        {
            // ה-Warning כבר נכתב ב-RepositoryBase. כאן רק מחזירים 409 במקום 500
            await WriteProblemAsync(context, StatusCodeFor(ex), "Conflict",
                "הערך כבר קיים במערכת (למשל ISBN, ברקוד, שם קטגוריה או מייל). יש לרענן ולנסות שוב.");
        }
        catch (Exception ex)
        {
            // TraceIdentifier הוא ה-CorrelationId (נקבע ב-CorrelationIdMiddleware), ולכן גם לוג השגיאה מכיל אותו
            _logger.LogError(ex, "Unhandled exception (CorrelationId {CorrelationId})", context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteProblemAsync(context, StatusCodeFor(ex), "Internal Server Error",
                "אירעה שגיאה בלתי צפויה. אם הבעיה חוזרת, יש לפנות לתמיכה עם מזהה הבקשה.");
        }
    }

    /// <summary>
    /// איזה סטטוס יקבל הלקוח על חריגה. גם RequestLoggingMiddleware משתמש בזה,
    /// כדי ששורת הלוג של הבקשה תציג את אותו סטטוס שהלקוח קיבל בפועל.
    /// </summary>
    public static int StatusCodeFor(Exception exception) =>
        exception is DuplicateValueException ? StatusCodes.Status409Conflict : StatusCodes.Status500InternalServerError;

    /// <summary>תשובת שגיאה בפורמט ה-JSON האחיד (ProblemDetails), עם מזהה הבקשה.</summary>
    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails { Status = statusCode, Title = title, Detail = detail };
        problem.Extensions["correlationId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
