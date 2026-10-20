using Library.Application.Common;
using Library.Application.DTOs.Common;
using Library.Application.Interfaces;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// מימוש משותף לכל ה-Repositories: הוספה, מחיקה ושמירה.
/// כאן, ורק כאן, מתורגמות שגיאות השמירה של EF לחריגות של Application (כך ש-Application לא מכיר את EF):
/// DbUpdateConcurrencyException → ConcurrencyConflictException, וערך ייחודי כפול → DuplicateValueException.
/// שתיהן נרשמות ללוג ברמת Warning.
/// </summary>
public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly ILogger _logger;

    protected RepositoryBase(AppDbContext db, ILogger logger)
    {
        Db = db;
        _logger = logger;
    }

    protected AppDbContext Db { get; }

    public void Add(TEntity entity) => Db.Set<TEntity>().Add(entity);

    public void Remove(TEntity entity) => Db.Set<TEntity>().Remove(entity);

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entities = string.Join(", ", ex.Entries.Select(e => e.Metadata.ClrType.Name));
            _logger.LogWarning(ex, "Concurrency conflict while saving {Entities}", entities);

            throw new ConcurrencyConflictException("The data was changed by another request.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // מרוץ על ערך ייחודי: שתי בקשות עברו יחד את בדיקת "האם כבר קיים?", והאינדקס הייחודי עצר את השנייה
            _logger.LogWarning(ex, "Unique value rejected by the database");

            throw new DuplicateValueException("A unique value already exists.", ex);
        }
    }

    /// <summary>
    /// עימוד אמיתי: ספירה, ואז Skip/Take בתוך השאילתה, כך שבסיס הנתונים מחזיר רק את העמוד המבוקש.
    /// השאילתה שמגיעה לכאן חייבת להיות ממוינת, אחרת הסדר בין העמודים לא מובטח.
    /// </summary>
    protected static async Task<PagedResult<T>> ToPagedResultAsync<T>(IQueryable<T> query, PagingQuery paging, CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((paging.Page - 1) * paging.PageSize)
                               .Take(paging.PageSize)
                               .ToListAsync(ct);

        return new PagedResult<T>(items, paging.Page, paging.PageSize, totalCount);
    }
}
