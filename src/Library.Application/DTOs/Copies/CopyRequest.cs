using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Copies;

public class CopyRequest
{
    [Required]
    [StringLength(50)]
    public string Barcode { get; set; } = string.Empty;
}
