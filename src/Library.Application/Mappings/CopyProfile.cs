using AutoMapper;
using Library.Application.DTOs.Copies;
using Library.Domain.Entities;

namespace Library.Application.Mappings;

public class CopyProfile : Profile
{
    public CopyProfile()
    {
        CreateMap<BookCopy, CopyResponse>();

        // BookId מגיע מה-route, Status ו-Version מקבלים ערכי ברירת מחדל בישות
        CreateMap<CopyRequest, BookCopy>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.BookId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Version, o => o.Ignore())
            .ForMember(d => d.Book, o => o.Ignore())
            .ForMember(d => d.Loans, o => o.Ignore());
    }
}
