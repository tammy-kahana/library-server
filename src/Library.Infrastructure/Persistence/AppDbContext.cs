using Library.Domain.Common;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // טוען את כל קבצי ה-Configuration (Fluent API) מהתיקייה Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// לפני כל שמירה: כל ישות מוגנת (IVersioned) שהשתנתה מקבלת Version חדש.
    /// הערך הישן נשאר ב-OriginalValues, ו-EF משתמש בו בתנאי ה-WHERE.
    /// אם מישהו אחר הקדים ושמר, אף שורה לא מתעדכנת ונזרקת DbUpdateConcurrencyException.
    /// דורסים את הגרסה עם acceptAllChangesOnSuccess, כי כל קריאה ל-SaveChangesAsync עוברת דרכה.
    /// </summary>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IVersioned>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version = Guid.NewGuid();
            }
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
