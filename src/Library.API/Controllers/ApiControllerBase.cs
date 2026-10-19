using Library.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

/// <summary>
/// בסיס לכל ה-Controllers. כאן, ורק כאן, Result מתורגם לתשובת HTTP.
/// [ApiController] בודק את ה-ModelState אוטומטית ומחזיר 400 אם ה-DTO לא תקין.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>הצלחה בלי תוכן → 204. כישלון → סטטוס לפי סוג השגיאה.</summary>
    protected IActionResult ToActionResult(Result result) =>
        result.IsSuccess ? NoContent() : ToErrorResponse(result.Error!);

    /// <summary>הצלחה עם ערך → 200. כישלון → סטטוס לפי סוג השגיאה.</summary>
    protected IActionResult ToActionResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : ToErrorResponse(result.Error!);

    /// <summary>
    /// יצירה מוצלחת → 201 עם כותרת Location שמצביעה על המשאב החדש.
    /// controllerName נדרש רק כשה-Location נמצא ב-Controller אחר.
    /// </summary>
    protected IActionResult ToCreatedResult<T>(Result<T> result, string actionName, Func<T, object> routeValues, string? controllerName = null) =>
        result.IsSuccess
            ? CreatedAtAction(actionName, controllerName, routeValues(result.Value!), result.Value)
            : ToErrorResponse(result.Error!);

    /// <summary>שגיאה עסקית → תשובת ProblemDetails (הפורמט התקני של ASP.NET Core לשגיאות).</summary>
    private IActionResult ToErrorResponse(Error error) =>
        Problem(detail: error.Message, statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        });
}
