using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class AttendanceLog
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    [Required]
    public DateTime ClockInTime { get; set; }

    public DateTime? ClockOutTime { get; set; }

    public TimeSpan? TotalHours => ClockOutTime.HasValue 
        ? ClockOutTime.Value - ClockInTime 
        : null;

    public string? Notes { get; set; }
}
