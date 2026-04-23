namespace Application.Dtos;

public class AnalyticsFilter
{
    public DateTime From { get; set; } = DateTime.Today.AddDays(-90);
    public DateTime To { get; set; } = DateTime.Today;
    public Guid? DepartmentId { get; set; }
}

public class AnalyticsBucket
{
    public string Label { get; set; } = default!;
    public double Value { get; set; }
}

public class AnalyticsPageDto
{
    public AnalyticsFilter Filter { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();
    public LeaveAnalyticsDto Leave { get; set; } = new();
    public AttendanceAnalyticsDto Attendance { get; set; } = new();
    public TicketAnalyticsDto Tickets { get; set; } = new();
    public WorkforceAnalyticsDto Workforce { get; set; } = new();
}

public class LeaveAnalyticsDto
{
    public int TotalRequests { get; set; }
    public int TotalDaysTaken { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public List<AnalyticsBucket> ByType { get; set; } = new();
    public List<AnalyticsBucket> MonthlyDaysTaken { get; set; } = new();
    public List<AnalyticsBucket> TopTakers { get; set; } = new();
}

public class AttendanceAnalyticsDto
{
    public double AttendanceRatePct { get; set; }
    public int UniqueActiveEmployees { get; set; }
    public int TotalClockIns { get; set; }
    public int LateClockIns { get; set; }
    public TimeSpan LateThreshold { get; set; } = new(9, 0, 0);
    public List<AnalyticsBucket> DailyTrend { get; set; } = new();
    public List<AnalyticsBucket> ByDepartment { get; set; } = new();
}

public class TicketAnalyticsDto
{
    public int TotalTickets { get; set; }
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
    public double AvgResolutionHours { get; set; }
    public List<AnalyticsBucket> ByCategory { get; set; } = new();
    public List<AnalyticsBucket> ByPriority { get; set; } = new();
    public List<AnalyticsBucket> MonthlyCreated { get; set; } = new();
}

public class WorkforceAnalyticsDto
{
    public int Headcount { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int OtherGenderCount { get; set; }
    public decimal TotalPayroll { get; set; }
    public decimal AverageSalary { get; set; }
    public decimal MedianSalary { get; set; }
    public List<AnalyticsBucket> TenureBuckets { get; set; } = new();
    public List<AnalyticsBucket> SalaryBuckets { get; set; } = new();
    public List<AnalyticsBucket> HiresByMonth { get; set; } = new();
    public List<AnalyticsBucket> HeadcountByDepartment { get; set; } = new();
}
