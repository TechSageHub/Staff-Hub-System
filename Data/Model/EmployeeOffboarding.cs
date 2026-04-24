namespace Data.Model;

public class EmployeeOffboarding
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public DateTime LastWorkingDay { get; set; }
    public string Reason { get; set; } = default!;
    public string? Notes { get; set; }
    public OffboardingStatus Status { get; set; } = OffboardingStatus.InProgress;
    public DateTime StartedAt { get; set; }
    public string? StartedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public ICollection<EmployeeOffboardingProgress> ProgressEntries { get; set; } = new List<EmployeeOffboardingProgress>();
}

public enum OffboardingStatus
{
    InProgress = 0,
    Completed = 1,
    Cancelled = 2
}
