using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class PerformanceAppraisal
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    [Required]
    public string Period { get; set; } = default!; // e.g. "Q1 2026", "2025 Annual"

    public int Rating { get; set; } // 1-5 scale

    public string? Strengths { get; set; }

    public string? AreasForImprovement { get; set; }

    public string? Goals { get; set; }

    public string? ManagerComments { get; set; }

    public string? ManagerId { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.Now;
}
