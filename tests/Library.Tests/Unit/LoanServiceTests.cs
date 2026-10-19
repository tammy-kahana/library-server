using Library.Application.Common;
using Library.Application.Interfaces;
using Library.Application.Services;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Library.Tests.Unit;

public class LoanServiceTests
{
    private const int ReaderId = 7;
    private const int CopyId = 5;

    private readonly Mock<ILoanRepository> _loans = new();
    private readonly Mock<IBookCopyRepository> _copies = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly BookCopy _copy = new() { Id = CopyId, BookId = 1, Barcode = "LIB-0005", Status = CopyStatus.Available };
    private readonly User _reader = new() { Id = ReaderId, FullName = "קוראת לבדיקה", Role = UserRole.Reader };
    private readonly LoanService _service;

    public LoanServiceTests()
    {
        _copies.Setup(r => r.GetForUpdateAsync(CopyId, It.IsAny<CancellationToken>())).ReturnsAsync(_copy);
        _users.Setup(r => r.GetForUpdateAsync(ReaderId, It.IsAny<CancellationToken>())).ReturnsAsync(_reader);

        var rules = Options.Create(new LoanRulesOptions { MaxActiveLoans = 3, LoanPeriodDays = 14 });

        _service = new LoanService(_loans.Object, _copies.Object, _users.Object, TestMapper.Create(), rules,
            NullLogger<LoanService>.Instance);
    }

    [Fact]
    public async Task Borrow_AvailableCopy_CreatesLoanDueIn14Days()
    {
        Loan? added = null;
        _loans.Setup(r => r.CountActiveByUserAsync(ReaderId, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _loans.Setup(r => r.Add(It.IsAny<Loan>())).Callback<Loan>(loan => added = loan);
        _loans.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(() => WithDetails(added!));

        var result = await _service.BorrowAsync(CopyId, ReaderId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(TimeSpan.FromDays(14), added!.DueAtUtc - added.LoanedAtUtc);
        Assert.Equal(CopyStatus.Loaned, _copy.Status);
        _users.Verify(r => r.MarkAsChanged(_reader), Times.Once);
        _loans.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Borrow_CopyAlreadyLoaned_ReturnsConflictWithoutSaving()
    {
        _copy.Status = CopyStatus.Loaned;

        var result = await _service.BorrowAsync(CopyId, ReaderId, CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error?.Type);
        _loans.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Borrow_ReaderAtLoanLimit_ReturnsConflictWithLimitMessage()
    {
        _loans.Setup(r => r.CountActiveByUserAsync(ReaderId, It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var result = await _service.BorrowAsync(CopyId, ReaderId, CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error?.Type);
        Assert.Contains("מכסת ההשאלות", result.Error!.Message);
        _loans.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Borrow_RepositoryReportsConcurrencyConflict_ReturnsConflict()
    {
        _loans.Setup(r => r.CountActiveByUserAsync(ReaderId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _loans.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new ConcurrencyConflictException("conflict", new Exception()));

        var result = await _service.BorrowAsync(CopyId, ReaderId, CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error?.Type);
        Assert.Contains("נתפס על ידי משתמשת אחרת", result.Error!.Message);
    }

    [Fact]
    public async Task Return_LoanOfAnotherReader_ReturnsForbidden()
    {
        var loan = new Loan { Id = 1, UserId = 99, BookCopy = _copy };
        _loans.Setup(r => r.GetForUpdateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(loan);

        var result = await _service.ReturnAsync(1, ReaderId, isStaff: false, CancellationToken.None);

        Assert.Equal(ErrorType.Forbidden, result.Error?.Type);
        Assert.Null(loan.ReturnedAtUtc);
    }

    [Fact]
    public async Task Return_ByLibrarianForAnyReader_MarksReturnedAndFreesCopy()
    {
        _copy.Status = CopyStatus.Loaned;
        var loan = new Loan { Id = 1, UserId = 99, BookCopy = _copy };
        _loans.Setup(r => r.GetForUpdateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(loan);

        var result = await _service.ReturnAsync(1, userId: 2, isStaff: true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(loan.ReturnedAtUtc);
        Assert.Equal(CopyStatus.Available, _copy.Status);
    }

    /// <summary>מה ש-GetByIdAsync האמיתי מחזיר: ההשאלה עם העותק, הספר והקוראת (לצורך המיפוי).</summary>
    private Loan WithDetails(Loan loan)
    {
        _copy.Book = new Book { Id = 1, Title = "ספר לבדיקה" };
        loan.BookCopy = _copy;
        loan.User = _reader;
        return loan;
    }
}
