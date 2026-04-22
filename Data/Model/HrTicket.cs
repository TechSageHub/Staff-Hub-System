using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class HrTicket
{
    public Guid Id { get; set; }

    [Required]
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    [Required]
    [MaxLength(80)]
    public string Category { get; set; } = default!;

    [Required]
    [MaxLength(180)]
    public string Subject { get; set; } = default!;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = default!;

    public HrTicketPriority Priority { get; set; } = HrTicketPriority.Medium;
    public HrTicketStatus Status { get; set; } = HrTicketStatus.Pending;

    [MaxLength(800)]
    public string? AdminComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public ICollection<HrTicketComment> Comments { get; set; } = new List<HrTicketComment>();
}

public enum HrTicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum HrTicketStatus
{
    Pending,
    InProgress,
    Resolved,
    Rejected
}
