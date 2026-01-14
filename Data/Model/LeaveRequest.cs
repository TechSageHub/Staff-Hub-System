using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class LeaveRequest
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public string LeaveType { get; set; } = default!; // e.g. Annual, Sick, Maternity

    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateTime DateRequested { get; set; } = DateTime.Now;

    public string? AdminComment { get; set; }
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}
