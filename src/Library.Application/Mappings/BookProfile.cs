using AutoMapper;
using Library.Application.DTOs.Books;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Mappings;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookResponse>()
            .ForMember(d => d.TotalCopies, o => o.MapFrom(s => s.Copies.Count))
            .ForMember(d => d.AvailableCopies, o => o.MapFrom(s => s.Copies.Count(c => c.Status == CopyStatus.Available)));

        // הקטגוריות לא ממופות כאן: ה-Service טוען אותן לפי CategoryIds ובודק שהן קיימות
        CreateMap<BookRequest, Book>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Copies, o => o.Ignore())
            .ForMember(d => d.Categories, o => o.Ignore());
    }
}
