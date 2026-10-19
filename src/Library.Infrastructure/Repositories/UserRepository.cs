using Library.Application.Common;
using Library.Application.DTOs.Common;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(AppDbContext db, ILogger<UserRepository> logger) : base(db, logger)
    {
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        Db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByIdAsync(int id, CancellationToken ct) =>
        Db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<PagedResult<User>> GetPagedAsync(PagingQuery query, CancellationToken ct) =>
        ToPagedResultAsync(Db.Users.AsNoTracking().OrderBy(u => u.Id), query, ct);

    public Task<User?> GetForUpdateAsync(int id, CancellationToken ct) =>
        Db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        Db.Users.AnyAsync(u => u.Email == email, ct);

    public void MarkAsChanged(User user) => Db.Entry(user).State = EntityState.Modified;
}
