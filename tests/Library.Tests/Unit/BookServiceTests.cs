using Library.Application.Common;
using Library.Application.DTOs.Books;
using Library.Application.Interfaces;
using Library.Application.Services;
using Library.Domain.Entities;
using Moq;

namespace Library.Tests.Unit;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _books = new();
    private readonly BookService _service;

    public BookServiceTests()
    {
        _service = new BookService(_books.Object, Mock.Of<ICategoryRepository>(), TestMapper.Create());
    }

    [Fact]
    public async Task GetById_BookDoesNotExist_ReturnsNotFound()
    {
        _books.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var result = await _service.GetByIdAsync(42, CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error?.Type);
    }

    [Fact]
    public async Task GetPaged_MapsItemsAndKeepsPagingData()
    {
        var query = new BookQuery { Page = 2, PageSize = 2 };
        var books = new List<Book> { new() { Id = 3, Title = "ג" }, new() { Id = 4, Title = "ד" } };
        _books.Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new PagedResult<Book>(books, Page: 2, PageSize: 2, TotalCount: 5));

        var result = await _service.GetPagedAsync(query, CancellationToken.None);

        Assert.Equal(new[] { 3, 4 }, result.Items.Select(b => b.Id));
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }
}
