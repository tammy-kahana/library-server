using Library.Application.Common;
using Library.Application.DTOs.Books;

namespace Library.Application.Interfaces;

public interface IBookService
{
    Task<PagedResult<BookResponse>> GetPagedAsync(BookQuery query, CancellationToken ct);
    Task<Result<BookResponse>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<BookResponse>> CreateAsync(BookRequest request, CancellationToken ct);
    Task<Result> UpdateAsync(int id, BookRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}
