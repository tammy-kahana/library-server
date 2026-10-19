namespace Library.Application.Interfaces;

/// <summary>
/// ממשק בסיס לכל ה-Repositories. המימוש נכתב פעם אחת במחלקת בסיס ב-Infrastructure.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);

    void Remove(TEntity entity);

    /// <exception cref="Common.ConcurrencyConflictException">
    /// כשמישהו אחר שינה את אחת הישויות בין הקריאה לשמירה.
    /// </exception>
    Task SaveChangesAsync(CancellationToken ct);
}
