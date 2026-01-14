namespace Application.Dtos;

public class AttendanceLogDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public DateTime ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public string? TotalHours { get; set; }
    public string? Notes { get; set; }
    public bool IsActive => !ClockOutTime.HasValue;
}
