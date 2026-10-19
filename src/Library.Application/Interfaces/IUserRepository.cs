using Library.Application.Common;
using Library.Application.DTOs.Common;
using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    /// <summary>קריאה בלבד (AsNoTracking). המייל צריך להגיע מנורמל.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);

    /// <summary>קריאה בלבד (AsNoTracking).</summary>
    Task<User?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>קריאה בלבד (AsNoTracking), ממוין לפי Id, Skip/Take בתוך השאילתה.</summary>
    Task<PagedResult<User>> GetPagedAsync(PagingQuery query, CancellationToken ct);

    /// <summary>עם tracking, לצורך עדכון (Version נבדק בשמירה).</summary>
    Task<User?> GetForUpdateAsync(int id, CancellationToken ct);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct);

    /// <summary>
    /// מסמנת את הקוראת כ"שונתה" גם בלי שינוי בשדות שלה, כדי שבשמירה ה-Version שלה יתחלף וייבדק.
    /// כך שתי השאלות מקבילות של אותה קוראת לא יעקפו יחד את המכסה: השנייה תקבל התנגשות.
    /// </summary>
    void MarkAsChanged(User user);
}
