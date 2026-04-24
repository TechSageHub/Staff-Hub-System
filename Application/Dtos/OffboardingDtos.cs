namespace Application.Dtos;

public class StartOffboardingDto
{
    public Guid EmployeeId { get; set; }
    public DateTime LastWorkingDay { get; set; }
    public string Reason { get; set; } = default!;
    public string? Notes { get; set; }
}

public class UpdateOffboardingItemDto
{
    public Guid EmployeeId { get; set; }
    public string ItemKey { get; set; } = default!;
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}

public class OffboardingChecklistStatusDto
{
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public class OffboardingSnapshotDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = default!;
    public DateTime LastWorkingDay { get; set; }
    public string Reason { get; set; } = default!;
    public string? Notes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; }
    public bool CanComplete { get; set; }
    public List<OffboardingChecklistStatusDto> Items { get; set; } = new();
}

public class OffboardingListItemDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = default!;
    public DateTime LastWorkingDay { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Reason { get; set; } = default!;
    public int ProgressPercent { get; set; }
}

public class OffboardingQuery
{
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
