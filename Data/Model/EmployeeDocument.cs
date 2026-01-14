using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class EmployeeDocument
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    [Required]
    public string DocumentType { get; set; } = default!; // e.g. ID Card, Contract, Certificate

    [Required]
    public string FileName { get; set; } = default!;

    [Required]
    public string FileUrl { get; set; } = default!;

    public DateTime UploadedDate { get; set; } = DateTime.Now;

    public DateTime? ExpiryDate { get; set; }

    public string? Notes { get; set; }
}
