using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Library.Infrastructure.Persistence;

/// <summary>
/// רץ בכל עליית שרת: מריץ migrations שעוד לא הורצו, ואם בסיס הנתונים ריק מכניס נתוני דמו.
/// כך המערכת עולה ורצה בלי התערבות ידנית בבסיס הנתונים.
/// הנתונים בדויים (ספרים, מחברים ו-ISBN).
/// </summary>
public class DatabaseInitializer
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(AppDbContext db, IPasswordHasher passwordHasher, ILogger<DatabaseInitializer> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        await _db.Database.MigrateAsync(ct);

        if (await _db.Users.AnyAsync(ct))
        {
            return;
        }

        var now = DateTime.UtcNow;

        // --- משתמשות דמו (הסיסמאות מתועדות ב-README) ---
        User NewUser(string fullName, string email, string password, UserRole role) => new()
        {
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            Role = role,
            CreatedAtUtc = now
        };

        var reader1 = NewUser("חנה גרין", "reader1@library.local", "Reader123!", UserRole.Reader);
        var reader2 = NewUser("אסתר בלום", "reader2@library.local", "Reader123!", UserRole.Reader);
        _db.Users.AddRange(
            NewUser("מרים לוי", "admin@library.local", "Admin123!", UserRole.Admin),
            NewUser("שושנה כץ", "librarian@library.local", "Librarian123!", UserRole.Librarian),
            reader1,
            reader2);

        // --- קטגוריות ---
        var categories = new[] { "סיפורת", "ילדים", "נוער", "קומיקס", "היסטוריה" }
            .ToDictionary(name => name, name => new Category { Name = name });

        // --- ספרים ועותקים ---
        var barcode = 0;
        Book NewBook(string title, string author, string isbn, int year, int copies, params string[] categoryNames)
        {
            var book = new Book { Title = title, Author = author, Isbn = isbn, PublishedYear = year };
            for (var i = 0; i < copies; i++)
            {
                book.Copies.Add(new BookCopy { Barcode = $"LIB-{++barcode:D4}" });
            }
            foreach (var name in categoryNames)
            {
                book.Categories.Add(categories[name]);
            }
            return book;
        }

        var house = NewBook("הבית ברחוב רבי עקיבא", "מ. גולדברג", "978-0-00-000001-0", 2015, 3, "סיפורת");
        var yanky = NewBook("יענקי והחתול האבוד", "ח. לוין", "978-0-00-000005-0", 2019, 3, "ילדים");
        var journey = NewBook("המסע אל הכפר הנידח", "ב. אורבך", "978-0-00-000009-0", 2018, 2, "נוער");

        _db.Books.AddRange(
            house,
            NewBook("המכתב שלא נשלח", "ר. פרידמן", "978-0-00-000002-0", 2017, 2, "סיפורת"),
            NewBook("שבילים בירושלים של פעם", "א. ברגמן", "978-0-00-000003-0", 2012, 2, "סיפורת", "היסטוריה"),
            NewBook("האור שבחלון", "ש. וייס", "978-0-00-000004-0", 2020, 3, "סיפורת"),
            yanky,
            NewBook("שבת בגן של סבתא", "ד. כהן", "978-0-00-000006-0", 2021, 2, "ילדים"),
            NewBook("מה מסתתר בעליית הגג?", "י. רוזן", "978-0-00-000007-0", 2016, 2, "ילדים"),
            // ספר עם עותק יחיד: להדגמת ההתנגשות (שתי קוראות על אותו עותק)
            NewBook("הסוד של מחנה הקיץ", "נ. שפירא", "978-0-00-000008-0", 2022, 1, "נוער"),
            journey,
            NewBook("הבלשים מבני ברק", "צ. הלפרין", "978-0-00-000010-0", 2023, 3, "קומיקס", "נוער"),
            NewBook("קהילות ישראל בתימן", "מ. שטרן", "978-0-00-000011-0", 2010, 2, "היסטוריה"),
            NewBook("מסע בזמן: ירושלים העתיקה", "ל. קליין", "978-0-00-000012-0", 2021, 2, "קומיקס", "היסטוריה"));

        // --- השאלות קיימות: אחת באיחור, אחת פעילה בזמן, אחת שהוחזרה ---
        Loan NewLoan(User reader, BookCopy copy, int loanedDaysAgo, int dueInDays, int? returnedDaysAgo = null)
        {
            if (returnedDaysAgo is null)
            {
                copy.Status = CopyStatus.Loaned;
            }

            return new Loan
            {
                User = reader,
                BookCopy = copy,
                LoanedAtUtc = now.AddDays(-loanedDaysAgo),
                DueAtUtc = now.AddDays(dueInDays),
                ReturnedAtUtc = returnedDaysAgo is null ? null : now.AddDays(-returnedDaysAgo.Value)
            };
        }

        _db.Loans.AddRange(
            NewLoan(reader1, house.Copies.First(), loanedDaysAgo: 20, dueInDays: -6),                      // באיחור
            NewLoan(reader1, yanky.Copies.First(), loanedDaysAgo: 3, dueInDays: 11),                       // פעילה
            NewLoan(reader2, journey.Copies.First(), loanedDaysAgo: 30, dueInDays: -16, returnedDaysAgo: 18)); // הוחזרה

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Database seeded with demo data");
    }
}
