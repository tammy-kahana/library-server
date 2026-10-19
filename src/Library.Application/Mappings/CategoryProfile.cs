using AutoMapper;
using Library.Application.DTOs.Categories;
using Library.Domain.Entities;

namespace Library.Application.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryResponse>();

        CreateMap<CategoryRequest, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Books, o => o.Ignore());
    }
}
