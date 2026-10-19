namespace Library.Application.DTOs.Loans;

public class LoanResponse
{
    public int Id { get; init; }
    public int BookCopyId { get; init; }
    public string Barcode { get; init; } = string.Empty;
    public int BookId { get; init; }
    public string BookTitle { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public DateTime LoanedAtUtc { get; init; }
    public DateTime DueAtUtc { get; init; }
    public DateTime? ReturnedAtUtc { get; init; }
    public bool IsOverdue { get; init; }
}
