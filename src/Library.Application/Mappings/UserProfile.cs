using AutoMapper;
using Library.Application.DTOs.Auth;
using Library.Application.DTOs.Users;
using Library.Domain.Entities;

namespace Library.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>();

        // את המייל (מנורמל), את ה-hash ואת תאריך היצירה קובע AuthService.
        // Role לא ממופה: נשאר ברירת המחדל של הישות (Reader).
        CreateMap<RegisterRequest, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
            .ForMember(d => d.Version, o => o.Ignore())
            .ForMember(d => d.Loans, o => o.Ignore());
    }
}
