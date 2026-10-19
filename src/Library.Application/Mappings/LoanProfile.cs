using AutoMapper;
using Library.Application.DTOs.Loans;
using Library.Domain.Entities;

namespace Library.Application.Mappings;

public class LoanProfile : Profile
{
    public LoanProfile()
    {
        // דורש שההשאלה נטענה עם BookCopy → Book ועם User (Include/ThenInclude ב-LoanRepository)
        CreateMap<Loan, LoanResponse>()
            .ForMember(d => d.Barcode, o => o.MapFrom(s => s.BookCopy.Barcode))
            .ForMember(d => d.BookId, o => o.MapFrom(s => s.BookCopy.BookId))
            .ForMember(d => d.BookTitle, o => o.MapFrom(s => s.BookCopy.Book.Title))
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User.FullName))
            // אותה הגדרה של "באיחור" כמו בסינון overdue שב-LoanRepository (שם היא רצה כ-SQL)
            .ForMember(d => d.IsOverdue, o => o.MapFrom(s => s.ReturnedAtUtc == null && s.DueAtUtc < DateTime.UtcNow));
    }
}
