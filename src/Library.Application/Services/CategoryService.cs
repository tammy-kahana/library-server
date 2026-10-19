using AutoMapper;
using Library.Application.Common;
using Library.Application.DTOs.Categories;
using Library.Application.Interfaces;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class CategoryService : ICategoryService
{
    private const string CategoryNotFound = "הקטגוריה לא נמצאה";
    private const string NameTaken = "כבר קיימת קטגוריה בשם הזה";

    private readonly ICategoryRepository _categories;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categories, IMapper mapper)
    {
        _categories = categories;
        _mapper = mapper;
    }

    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct)
    {
        var categories = await _categories.GetAllAsync(ct);
        return _mapper.Map<List<CategoryResponse>>(categories);
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var category = await _categories.GetByIdAsync(id, ct);
        if (category is null)
        {
            return Error.NotFound(CategoryNotFound);
        }

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CategoryRequest request, CancellationToken ct)
    {
        if (await _categories.NameExistsAsync(request.Name, excludeCategoryId: null, ct))
        {
            return Error.Conflict(NameTaken);
        }

        var category = _mapper.Map<Category>(request);
        _categories.Add(category);
        await _categories.SaveChangesAsync(ct);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<Result> UpdateAsync(int id, CategoryRequest request, CancellationToken ct)
    {
        var category = await _categories.GetForUpdateAsync(id, ct);
        if (category is null)
        {
            return Error.NotFound(CategoryNotFound);
        }

        if (await _categories.NameExistsAsync(request.Name, excludeCategoryId: id, ct))
        {
            return Error.Conflict(NameTaken);
        }

        _mapper.Map(request, category);
        await _categories.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>מחיקת קטגוריה מסירה רק את הקישור שלה לספרים. הספרים עצמם נשארים.</summary>
    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var category = await _categories.GetForUpdateAsync(id, ct);
        if (category is null)
        {
            return Error.NotFound(CategoryNotFound);
        }

        _categories.Remove(category);
        await _categories.SaveChangesAsync(ct);
        return Result.Success();
    }
}
