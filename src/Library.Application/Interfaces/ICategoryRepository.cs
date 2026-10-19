using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>קריאה בלבד (AsNoTracking), ממוין לפי שם.</summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct);

    /// <summary>קריאה בלבד (AsNoTracking).</summary>
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>עם tracking, לצורך עדכון או מחיקה.</summary>
    Task<Category?> GetForUpdateAsync(int id, CancellationToken ct);

    /// <summary>עם tracking, כדי לקשר את הקטגוריות לספר.</summary>
    Task<IReadOnlyList<Category>> GetForUpdateByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);

    Task<bool> NameExistsAsync(string name, int? excludeCategoryId, CancellationToken ct);
}
