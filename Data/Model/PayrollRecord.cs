using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class PayrollRecord
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    [Required]
    public int Month { get; set; } // 1-12

    [Required]
    public int Year { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal TaxDeduction { get; set; }

    public decimal PensionDeduction { get; set; }

    public decimal OtherDeductions { get; set; }

    public decimal Bonuses { get; set; }

    public decimal NetSalary { get; set; }

    public DateTime GeneratedDate { get; set; } = DateTime.Now;

    public string? Notes { get; set; }
}
