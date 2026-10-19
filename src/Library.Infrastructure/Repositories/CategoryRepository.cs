using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Repositories;

public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext db, ILogger<CategoryRepository> logger) : base(db, logger)
    {
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct) =>
        await Db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        Db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Category>> GetForUpdateByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        await Db.Categories.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

    public Task<bool> NameExistsAsync(string name, int? excludeCategoryId, CancellationToken ct) =>
        Db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeCategoryId, ct);
}
