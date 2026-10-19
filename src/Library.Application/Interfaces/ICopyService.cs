using Library.Application.Common;
using Library.Application.DTOs.Copies;

namespace Library.Application.Interfaces;

public interface ICopyService
{
    Task<Result<List<CopyResponse>>> GetByBookIdAsync(int bookId, CancellationToken ct);
    Task<Result<CopyResponse>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<CopyResponse>> AddAsync(int bookId, CopyRequest request, CancellationToken ct);
    Task<Result> ChangeStatusAsync(int id, CopyStatusRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}
