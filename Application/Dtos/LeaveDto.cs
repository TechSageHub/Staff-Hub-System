namespace Application.Dtos;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; } = default! ;
    public string? Reason { get; set; }
    public string Status { get; set; } = default!;
    public DateTime DateRequested { get; set; }
    public string? AdminComment { get; set; }
    public int DurationInDays => (EndDate - StartDate).Days + 1;
}

public class CreateLeaveRequestDto
{
    public Guid EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; } = default!;
    public string? Reason { get; set; }
}

public class LeaveApprovalDto
{
    public Guid LeaveId { get; set; }
    public string Status { get; set; } = default!;
    public string? AdminComment { get; set; }
}
