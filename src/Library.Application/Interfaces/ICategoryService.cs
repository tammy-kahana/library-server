using Library.Application.Common;
using Library.Application.DTOs.Categories;

namespace Library.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct);
    Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<CategoryResponse>> CreateAsync(CategoryRequest request, CancellationToken ct);
    Task<Result> UpdateAsync(int id, CategoryRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int id, CancellationToken ct);
}
