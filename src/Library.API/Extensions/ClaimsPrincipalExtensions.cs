using System.Security.Claims;

namespace Library.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>המזהה של המשתמשת המחוברת, מתוך ה-claim שנכנס ל-JWT בהתחברות.</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("The user id claim is missing."));
}
