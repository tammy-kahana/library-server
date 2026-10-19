using Library.Application.Common;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Library.Tests.Concurrency;

/// <summary>
/// ההוכחה שה-concurrency token עובד: שני DbContext נפרדים (שתי "בקשות") קוראים את אותה ישות,
/// שניהם משנים אותה, הראשון שומר בהצלחה והשני מקבל חריגה.
/// </summary>
public class ConcurrencyTests
{
    [Fact]
    public async Task TwoReadersBorrowSameCopy_FirstSucceeds_SecondThrows_OnlyOneActiveLoan()
    {
        using var database = new SqliteTestDatabase();
        var data = await database.SeedAsync();

        await using var contextA = database.CreateContext();
        await using var contextB = database.CreateContext();

        // שתי הבקשות קוראות את העותק לפני שמישהי שמרה: שתיהן רואות Available עם אותו Version
        var copyA = await contextA.BookCopies.SingleAsync(c => c.Id == data.FreeCopy1Id);
        var copyB = await contextB.BookCopies.SingleAsync(c => c.Id == data.FreeCopy1Id);
        Assert.Equal(CopyStatus.Available, copyA.Status);
        Assert.Equal(CopyStatus.Available, copyB.Status);

        Borrow(contextA, copyA, data.ReaderAId);
        Borrow(contextB, copyB, data.ReaderBWithTwoLoansId);

        await contextA.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());

        // השמירה השנייה בוטלה כולה (טרנזקציה אחת): גם ההשאלה שלה לא נכנסה
        await using var verify = database.CreateContext();
        var activeLoans = await verify.Loans.CountAsync(l => l.BookCopyId == data.FreeCopy1Id && l.ReturnedAtUtc == null);
        Assert.Equal(1, activeLoans);
    }

    [Fact]
    public async Task SameReaderBorrowsTwoCopiesAtOnce_AtLimit_SecondThrows_ReaderEndsWithThreeLoans()
    {
        using var database = new SqliteTestDatabase();
        var data = await database.SeedAsync();
        var readerId = data.ReaderBWithTwoLoansId;

        await using var contextA = database.CreateContext();
        await using var contextB = database.CreateContext();

        // שתי הבקשות סופרות 2 השאלות פעילות (פחות מ-3), ולכן שתיהן "מאשרות"
        Assert.Equal(2, await contextA.Loans.CountAsync(l => l.UserId == readerId && l.ReturnedAtUtc == null));
        Assert.Equal(2, await contextB.Loans.CountAsync(l => l.UserId == readerId && l.ReturnedAtUtc == null));

        var readerInA = await contextA.Users.SingleAsync(u => u.Id == readerId);
        var readerInB = await contextB.Users.SingleAsync(u => u.Id == readerId);
        Borrow(contextA, await contextA.BookCopies.SingleAsync(c => c.Id == data.FreeCopy1Id), readerId);
        Borrow(contextB, await contextB.BookCopies.SingleAsync(c => c.Id == data.FreeCopy2Id), readerId);

        // בדיוק מה ש-LoanService עושה: מסמנת את הקוראת, כדי שה-Version שלה ייבדק
        new UserRepository(contextA, NullLogger<UserRepository>.Instance).MarkAsChanged(readerInA);
        new UserRepository(contextB, NullLogger<UserRepository>.Instance).MarkAsChanged(readerInB);

        await contextA.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());

        await using var verify = database.CreateContext();
        Assert.Equal(3, await verify.Loans.CountAsync(l => l.UserId == readerId && l.ReturnedAtUtc == null));
    }

    [Fact]
    public async Task Repository_TranslatesConcurrencyExceptionToApplicationException()
    {
        using var database = new SqliteTestDatabase();
        var data = await database.SeedAsync();

        await using var contextA = database.CreateContext();
        await using var contextB = database.CreateContext();
        var repositoryA = new BookCopyRepository(contextA, NullLogger<BookCopyRepository>.Instance);
        var repositoryB = new BookCopyRepository(contextB, NullLogger<BookCopyRepository>.Instance);

        var copyA = await repositoryA.GetForUpdateAsync(data.FreeCopy1Id, CancellationToken.None);
        var copyB = await repositoryB.GetForUpdateAsync(data.FreeCopy1Id, CancellationToken.None);
        copyA!.Status = CopyStatus.Lost;
        copyB!.Status = CopyStatus.Lost;

        await repositoryA.SaveChangesAsync(CancellationToken.None);

        // Application לא מכיר את EF: מה שיוצא מה-Repository הוא החריגה שלנו
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repositoryB.SaveChangesAsync(CancellationToken.None));
    }

    private static void Borrow(AppDbContext context, BookCopy copy, int readerId)
    {
        var now = DateTime.UtcNow;
        copy.Status = CopyStatus.Loaned;
        context.Loans.Add(new Loan { BookCopyId = copy.Id, UserId = readerId, LoanedAtUtc = now, DueAtUtc = now.AddDays(14) });
    }
}
