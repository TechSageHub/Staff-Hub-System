using Application.Dtos;
using Application.Services.Attendance;
using Application.Services.Department;
using Application.Services.Employee;
using Application.Services.HrTicket;
using Application.Services.Leave;

namespace Application.Services.Analytics;

public class AnalyticsService(
    IEmployeeService employeeService,
    IDepartmentService departmentService,
    ILeaveService leaveService,
    IAttendanceService attendanceService,
    IHrTicketService ticketService) : IAnalyticsService
{
    private static readonly TimeSpan LateThreshold = new(9, 0, 0);

    public async Task<AnalyticsPageDto> GetAnalyticsAsync(AnalyticsFilter filter)
    {
        var departments = await departmentService.GetAllDepartmentsAsync();
        var leave = await GetLeaveAnalyticsAsync(filter);
        var attendance = await GetAttendanceAnalyticsAsync(filter);
        var tickets = await GetTicketAnalyticsAsync(filter);
        var workforce = await GetWorkforceAnalyticsAsync(filter);

        return new AnalyticsPageDto
        {
            Filter = filter,
            Departments = departments.Departments,
            Overview = new AnalyticsOverviewDto
            {
                Headcount = workforce.Headcount,
                AttendanceRatePct = attendance.AttendanceRatePct,
                PendingLeaveRequests = leave.PendingCount,
                OpenTickets = tickets.OpenCount,
                AvgResolutionHours = tickets.AvgResolutionHours
            },
            Leave = leave,
            Attendance = attendance,
            Tickets = tickets,
            Workforce = workforce
        };
    }

    public async Task<LeaveAnalyticsDto> GetLeaveAnalyticsAsync(AnalyticsFilter filter)
    {
        var all = await leaveService.GetAllLeaveRequestsAsync(null);

        var inRange = all.Where(l => l.StartDate.Date <= filter.To && l.EndDate.Date >= filter.From);
        if (filter.DepartmentId.HasValue)
        {
            var dept = await departmentService.GetDepartmentByIdAsync(filter.DepartmentId.Value);
            inRange = inRange.Where(l => string.Equals(l.DepartmentName, dept.Name, StringComparison.OrdinalIgnoreCase));
        }

        var list = inRange.ToList();
        var approvedOrTaken = list.Where(l => l.Status == "Approved").ToList();

        return new LeaveAnalyticsDto
        {
            TotalRequests = list.Count,
            TotalDaysTaken = approvedOrTaken.Sum(l => l.DurationInDays),
            PendingCount = list.Count(l => l.Status == "Pending"),
            ApprovedCount = list.Count(l => l.Status == "Approved"),
            RejectedCount = list.Count(l => l.Status == "Rejected"),
            CancelledCount = list.Count(l => l.Status == "Cancelled"),
            ByType = list
                .GroupBy(l => l.LeaveType)
                .Select(g => new AnalyticsBucket { Label = g.Key, Value = g.Sum(x => x.DurationInDays) })
                .OrderByDescending(b => b.Value)
                .ToList(),
            MonthlyDaysTaken = BuildMonthlyBuckets(filter.From, filter.To,
                approvedOrTaken.Select(l => (l.StartDate, (double)l.DurationInDays))),
            TopTakers = approvedOrTaken
                .GroupBy(l => l.EmployeeName)
                .Select(g => new AnalyticsBucket { Label = g.Key, Value = g.Sum(x => x.DurationInDays) })
                .OrderByDescending(b => b.Value)
                .Take(5)
                .ToList()
        };
    }

    public async Task<AttendanceAnalyticsDto> GetAttendanceAnalyticsAsync(AnalyticsFilter filter)
    {
        var logs = await attendanceService.GetAllAttendanceAsync(filter.From, filter.To, null);
        var employeesDto = await employeeService.GetAllEmployeesAsync(null);
        var employees = employeesDto.Employees;

        if (filter.DepartmentId.HasValue)
        {
            var deptIds = employees.Where(e => e.DepartmentId == filter.DepartmentId).Select(e => e.Id).ToHashSet();
            logs = logs.Where(l => deptIds.Contains(l.EmployeeId)).ToList();
            employees = employees.Where(e => e.DepartmentId == filter.DepartmentId).ToList();
        }

        var workdays = EnumerateDates(filter.From, filter.To).Count(IsWorkday);
        var expected = workdays * Math.Max(1, employees.Count);
        var present = logs.Select(l => new { l.EmployeeId, Day = l.ClockInTime.Date })
                          .Where(x => IsWorkday(x.Day))
                          .Distinct()
                          .Count();
        var rate = expected == 0 ? 0 : Math.Round(100.0 * present / expected, 1);

        var dailyTrend = logs
            .GroupBy(l => l.ClockInTime.Date)
            .OrderBy(g => g.Key)
            .Select(g => new AnalyticsBucket { Label = g.Key.ToString("MMM dd"), Value = g.Count() })
            .ToList();

        var empById = employees.ToDictionary(e => e.Id, e => e);
        var byDept = logs
            .Where(l => empById.ContainsKey(l.EmployeeId))
            .GroupBy(l => empById[l.EmployeeId].DepartmentName ?? "Unassigned")
            .Select(g => new AnalyticsBucket { Label = g.Key, Value = g.Count() })
            .OrderByDescending(b => b.Value)
            .ToList();

        return new AttendanceAnalyticsDto
        {
            AttendanceRatePct = rate,
            UniqueActiveEmployees = logs.Select(l => l.EmployeeId).Distinct().Count(),
            TotalClockIns = logs.Count,
            LateClockIns = logs.Count(l => l.ClockInTime.TimeOfDay > LateThreshold),
            LateThreshold = LateThreshold,
            DailyTrend = dailyTrend,
            ByDepartment = byDept
        };
    }

    public async Task<TicketAnalyticsDto> GetTicketAnalyticsAsync(AnalyticsFilter filter)
    {
        var all = await ticketService.GetAllTicketsAsync(null);
        var list = all.Where(t => t.CreatedAt.Date >= filter.From.Date && t.CreatedAt.Date <= filter.To.Date).ToList();

        if (filter.DepartmentId.HasValue)
        {
            var employeesDto = await employeeService.GetAllEmployeesAsync(null);
            var deptEmpIds = employeesDto.Employees
                .Where(e => e.DepartmentId == filter.DepartmentId)
                .Select(e => e.Id).ToHashSet();
            list = list.Where(t => deptEmpIds.Contains(t.EmployeeId)).ToList();
        }

        var resolved = list.Where(t => t.ResolvedAt.HasValue).ToList();
        var avgHours = resolved.Count == 0
            ? 0
            : Math.Round(resolved.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours), 1);

        return new TicketAnalyticsDto
        {
            TotalTickets = list.Count,
            OpenCount = list.Count(t => t.Status == "Open"),
            InProgressCount = list.Count(t => t.Status == "InProgress" || t.Status == "In Progress"),
            ResolvedCount = list.Count(t => t.Status == "Resolved"),
            ClosedCount = list.Count(t => t.Status == "Closed"),
            AvgResolutionHours = avgHours,
            ByCategory = list
                .GroupBy(t => string.IsNullOrWhiteSpace(t.Category) ? "Uncategorized" : t.Category)
                .Select(g => new AnalyticsBucket { Label = g.Key, Value = g.Count() })
                .OrderByDescending(b => b.Value)
                .ToList(),
            ByPriority = list
                .GroupBy(t => t.Priority)
                .Select(g => new AnalyticsBucket { Label = g.Key, Value = g.Count() })
                .OrderByDescending(b => b.Value)
                .ToList(),
            MonthlyCreated = BuildMonthlyBuckets(filter.From, filter.To,
                list.Select(t => (t.CreatedAt, 1.0)))
        };
    }

    public async Task<WorkforceAnalyticsDto> GetWorkforceAnalyticsAsync(AnalyticsFilter filter)
    {
        var employeesDto = await employeeService.GetAllEmployeesAsync(null);
        var employees = employeesDto.Employees;

        if (filter.DepartmentId.HasValue)
        {
            employees = employees.Where(e => e.DepartmentId == filter.DepartmentId).ToList();
        }

        var salaries = employees.Select(e => e.Salary).OrderBy(s => s).ToList();
        decimal median = 0;
        if (salaries.Count > 0)
        {
            var mid = salaries.Count / 2;
            median = salaries.Count % 2 == 0 ? (salaries[mid - 1] + salaries[mid]) / 2 : salaries[mid];
        }

        var now = DateTime.Today;
        int tenureBucket(DateTime hire)
        {
            var years = (now - hire).TotalDays / 365.25;
            if (years < 1) return 0;
            if (years < 3) return 1;
            if (years < 5) return 2;
            if (years < 10) return 3;
            return 4;
        }
        var tenureLabels = new[] { "0–1 yr", "1–3 yrs", "3–5 yrs", "5–10 yrs", "10+ yrs" };
        var tenureCounts = new int[5];
        foreach (var e in employees) tenureCounts[tenureBucket(e.HireDate)]++;

        var salaryLabels = new[] { "< 50k", "50k–100k", "100k–200k", "200k–500k", "500k+" };
        var salaryCounts = new int[5];
        foreach (var e in employees)
        {
            var s = e.Salary;
            int idx = s < 50_000m ? 0 : s < 100_000m ? 1 : s < 200_000m ? 2 : s < 500_000m ? 3 : 4;
            salaryCounts[idx]++;
        }

        return new WorkforceAnalyticsDto
        {
            Headcount = employees.Count,
            MaleCount = employees.Count(e => e.Gender == "Male"),
            FemaleCount = employees.Count(e => e.Gender == "Female"),
            OtherGenderCount = employees.Count(e => e.Gender != "Male" && e.Gender != "Female"),
            TotalPayroll = employees.Sum(e => e.Salary),
            AverageSalary = employees.Count == 0 ? 0 : Math.Round(employees.Average(e => e.Salary), 2),
            MedianSalary = median,
            TenureBuckets = tenureLabels
                .Select((l, i) => new AnalyticsBucket { Label = l, Value = tenureCounts[i] })
                .ToList(),
            SalaryBuckets = salaryLabels
                .Select((l, i) => new AnalyticsBucket { Label = l, Value = salaryCounts[i] })
                .ToList(),
            HiresByMonth = BuildMonthlyBuckets(filter.From, filter.To,
                employees.Select(e => (e.HireDate, 1.0))),
            HeadcountByDepartment = employees
                .GroupBy(e => string.IsNullOrWhiteSpace(e.DepartmentName) ? "Unassigned" : e.DepartmentName)
                .Select(g => new AnalyticsBucket { Label = g.Key, Value = g.Count() })
                .OrderByDescending(b => b.Value)
                .ToList()
        };
    }

    private static List<AnalyticsBucket> BuildMonthlyBuckets(DateTime from, DateTime to,
        IEnumerable<(DateTime When, double Amount)> rows)
    {
        var start = new DateTime(from.Year, from.Month, 1);
        var end = new DateTime(to.Year, to.Month, 1);
        var buckets = new List<AnalyticsBucket>();
        for (var m = start; m <= end; m = m.AddMonths(1))
        {
            buckets.Add(new AnalyticsBucket { Label = m.ToString("MMM yy"), Value = 0 });
        }
        foreach (var row in rows)
        {
            if (row.When.Date < from.Date || row.When.Date > to.Date) continue;
            var idx = ((row.When.Year - start.Year) * 12) + (row.When.Month - start.Month);
            if (idx >= 0 && idx < buckets.Count) buckets[idx].Value += row.Amount;
        }
        return buckets;
    }

    private static IEnumerable<DateTime> EnumerateDates(DateTime from, DateTime to)
    {
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1)) yield return d;
    }

    private static bool IsWorkday(DateTime d)
        => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday;
}
