namespace Library.Application.Common;

public enum ErrorType
{
    NotFound,
    Conflict,
    Validation,
    Forbidden,
    Unauthorized
}

/// <summary>
/// כישלון עסקי צפוי (לא באג). ה-Controller ממפה את Type לסטטוס HTTP.
/// </summary>
public sealed record Error(ErrorType Type, string Message)
{
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);
    public static Error Conflict(string message) => new(ErrorType.Conflict, message);
    public static Error Validation(string message) => new(ErrorType.Validation, message);
    public static Error Forbidden(string message) => new(ErrorType.Forbidden, message);
    public static Error Unauthorized(string message) => new(ErrorType.Unauthorized, message);
}
