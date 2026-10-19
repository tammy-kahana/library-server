using System.ComponentModel.DataAnnotations;
using Library.Domain.Common;
using Library.Domain.Enums;

namespace Library.Domain.Entities;

public class User : IVersioned
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Reader;
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>מגן על מכסת ההשאלות מפני שתי השאלות מקבילות של אותה קוראת.</summary>
    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
