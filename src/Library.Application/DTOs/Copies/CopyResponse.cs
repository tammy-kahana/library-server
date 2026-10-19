using Library.Domain.Enums;

namespace Library.Application.DTOs.Copies;

public class CopyResponse
{
    public int Id { get; init; }
    public int BookId { get; init; }
    public string Barcode { get; init; } = string.Empty;
    public CopyStatus Status { get; init; }
}
