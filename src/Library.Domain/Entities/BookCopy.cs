using System.ComponentModel.DataAnnotations;
using Library.Domain.Common;
using Library.Domain.Enums;

namespace Library.Domain.Entities;

/// <summary>המשאב המוגבל: כל עותק מושאל לקוראת אחת בלבד בכל רגע.</summary>
public class BookCopy : IVersioned
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public CopyStatus Status { get; set; } = CopyStatus.Available;

    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    public Book Book { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
