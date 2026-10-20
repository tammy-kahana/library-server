using Library.Application.Interfaces;

namespace Library.Application.Common;

public static class RepositoryExtensions
{
    /// <summary>
    /// שמירה שמחזירה Result: הצלחה, או Conflict (409) עם ההודעה שנמסרה אם הייתה התנגשות.
    /// כל ה-Services שמשנים ישויות מוגנות (עותק, קוראת) משתמשים בה, כדי שהטיפול לא יחזור בכל Service.
    /// ה-Warning ללוג כבר נכתב ב-RepositoryBase.
    /// </summary>
    public static async Task<Result> TrySaveChangesAsync<TEntity>(
        this IRepository<TEntity> repository, string conflictMessage, CancellationToken ct)
        where TEntity : class
    {
        try
        {
            await repository.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict(conflictMessage);
        }
    }
}
