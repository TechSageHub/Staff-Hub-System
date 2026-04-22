namespace Application.Dtos.Paging;

public class EmployeeQuery : PageRequest
{
    public string? Search { get; set; }
    public string? Department { get; set; }
    public string? Onboarding { get; set; }  // "complete" | "incomplete" | null
    public string Sort { get; set; } = "name_asc";
}

public class LeaveQuery : PageRequest
{
    public string? Status { get; set; }
    public string? Search { get; set; }  // matches employee name
}

public class HrTicketQuery : PageRequest
{
    public string? Status { get; set; }
    public string? Search { get; set; }  // matches subject / employee name
}
