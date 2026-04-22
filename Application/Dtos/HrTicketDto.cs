using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class HrTicketDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string EmployeeEmail { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Priority { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<HrTicketCommentDto> Comments { get; set; } = new();
}

public class HrTicketCommentDto
{
    public Guid Id { get; set; }
    public Guid HrTicketId { get; set; }
    public string CommenterName { get; set; } = default!;
    public bool IsAdminComment { get; set; }
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class CreateHrTicketDto
{
    public Guid EmployeeId { get; set; }

    [Required]
    [MaxLength(80)]
    public string Category { get; set; } = default!;

    [Required]
    [MaxLength(180)]
    public string Subject { get; set; } = default!;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = default!;

    [Required]
    public string Priority { get; set; } = "Medium";
}

public class UpdateHrTicketStatusDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Status { get; set; } = default!;

    [MaxLength(800)]
    public string? AdminComment { get; set; }
}

public class AddHrTicketCommentDto
{
    [Required]
    public Guid HrTicketId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = default!;
}
