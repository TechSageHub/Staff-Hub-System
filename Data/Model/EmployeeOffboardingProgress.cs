namespace Data.Model;

public class EmployeeOffboardingProgress
{
    public Guid Id { get; set; }
    public Guid EmployeeOffboardingId { get; set; }
    public EmployeeOffboarding EmployeeOffboarding { get; set; } = default!;
    public Guid OffboardingChecklistItemId { get; set; }
    public OffboardingChecklistItem OffboardingChecklistItem { get; set; } = default!;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedByUserId { get; set; }
    public string? Notes { get; set; }
}
