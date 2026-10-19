using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests.Concurrency;

/// <summary>
/// בסיס נתונים SQLite בזיכרון, עם חיבור אחד פתוח שמשותף לכמה DbContext נפרדים.
/// כל DbContext מדמה בקשה נפרדת: יש לו זיכרון (Change Tracker) משלו, אבל כולם רואים את אותם נתונים.
/// בסיס הנתונים קיים כל עוד החיבור פתוח, ונמחק ב-Dispose.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // EnsureCreated ולא Migrate: ה-migrations נכתבו ל-PostgreSQL
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    /// <summary>
    /// ספר עם 4 עותקים, קוראת A בלי השאלות, וקוראת B עם 2 השאלות פעילות (על עותקים 3 ו-4).
    /// עותקים 1 ו-2 פנויים.
    /// </summary>
    public async Task<TestData> SeedAsync()
    {
        await using var db = CreateContext();
        var now = DateTime.UtcNow;

        var book = new Book { Title = "ספר לבדיקה", Author = "מחברת", Isbn = "TEST-1", PublishedYear = 2020 };
        var copies = Enumerable.Range(1, 4).Select(i => new BookCopy { Barcode = $"T-{i}", Book = book }).ToList();
        var readerA = new User { FullName = "קוראת A", Email = "a@test.local", PasswordHash = "x", Role = UserRole.Reader, CreatedAtUtc = now };
        var readerB = new User { FullName = "קוראת B", Email = "b@test.local", PasswordHash = "x", Role = UserRole.Reader, CreatedAtUtc = now };

        db.AddRange(copies);
        db.AddRange(readerA, readerB);
        foreach (var copy in copies.Skip(2))
        {
            copy.Status = CopyStatus.Loaned;
            db.Loans.Add(new Loan { BookCopy = copy, User = readerB, LoanedAtUtc = now, DueAtUtc = now.AddDays(14) });
        }

        await db.SaveChangesAsync();

        return new TestData(copies[0].Id, copies[1].Id, readerA.Id, readerB.Id);
    }

    public void Dispose() => _connection.Dispose();
}

public sealed record TestData(int FreeCopy1Id, int FreeCopy2Id, int ReaderAId, int ReaderBWithTwoLoansId);
