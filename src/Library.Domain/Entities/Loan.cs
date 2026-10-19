namespace Library.Domain.Entities;

public class Loan
{
    public int Id { get; set; }
    public int BookCopyId { get; set; }
    public int UserId { get; set; }
    public DateTime LoanedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }

    /// <summary>ריק = השאלה פעילה.</summary>
    public DateTime? ReturnedAtUtc { get; set; }

    public BookCopy BookCopy { get; set; } = null!;
    public User User { get; set; } = null!;
}
