using System.ComponentModel.DataAnnotations;

namespace Library.Application.DTOs.Loans;

/// <summary>POST /api/loans. הקוראת נלקחת מה-token, לא מהגוף.</summary>
public class CreateLoanRequest
{
    [Range(1, int.MaxValue)]
    public int CopyId { get; set; }
}
