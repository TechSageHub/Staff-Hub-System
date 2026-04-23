using System.Globalization;
using System.Text;
using Application.Dtos;
using Application.Services.Analytics;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Reporting;
using QuestPDF.Fluent;

namespace Presentation.Controllers;

[Authorize]
public class AnalyticsController(IAnalyticsService _analytics) : BaseController
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, Guid? dept)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        var filter = BuildFilter(from, to, dept);
        var data = await _analytics.GetAnalyticsAsync(filter);
        return View(data);
    }

    public async Task<IActionResult> ExportCsv(string section, DateTime? from, DateTime? to, Guid? dept)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        var filter = BuildFilter(from, to, dept);
        var data = await _analytics.GetAnalyticsAsync(filter);

        var (rows, fileName) = section?.ToLowerInvariant() switch
        {
            "leave" => (LeaveRows(data.Leave), "leave-analytics.csv"),
            "attendance" => (AttendanceRows(data.Attendance), "attendance-analytics.csv"),
            "tickets" => (TicketRows(data.Tickets), "ticket-analytics.csv"),
            "workforce" => (WorkforceRows(data.Workforce), "workforce-analytics.csv"),
            _ => (Array.Empty<(string, string, string)>() as IEnumerable<(string, string, string)>, "analytics.csv")
        };

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("Section");
            csv.WriteField("Label");
            csv.WriteField("Value");
            csv.NextRecord();
            foreach (var (s, l, v) in rows)
            {
                csv.WriteField(s);
                csv.WriteField(l);
                csv.WriteField(v);
                csv.NextRecord();
            }
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }

    public async Task<IActionResult> ExportPdf(DateTime? from, DateTime? to, Guid? dept)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        var filter = BuildFilter(from, to, dept);
        var data = await _analytics.GetAnalyticsAsync(filter);
        var doc = new AnalyticsReportDocument(data, "EmployeeApp");
        var bytes = doc.GeneratePdf();
        return File(bytes, "application/pdf", $"analytics-{DateTime.Today:yyyy-MM-dd}.pdf");
    }

    private static AnalyticsFilter BuildFilter(DateTime? from, DateTime? to, Guid? dept) => new()
    {
        From = (from ?? DateTime.Today.AddDays(-90)).Date,
        To = (to ?? DateTime.Today).Date,
        DepartmentId = dept
    };

    private static IEnumerable<(string Section, string Label, string Value)> LeaveRows(LeaveAnalyticsDto l)
    {
        yield return ("Summary", "Total requests", l.TotalRequests.ToString());
        yield return ("Summary", "Days taken", l.TotalDaysTaken.ToString());
        yield return ("Summary", "Pending", l.PendingCount.ToString());
        yield return ("Summary", "Approved", l.ApprovedCount.ToString());
        yield return ("Summary", "Rejected", l.RejectedCount.ToString());
        yield return ("Summary", "Cancelled", l.CancelledCount.ToString());
        foreach (var b in l.ByType) yield return ("By type (days)", b.Label, b.Value.ToString("0.##"));
        foreach (var b in l.MonthlyDaysTaken) yield return ("Monthly days taken", b.Label, b.Value.ToString("0.##"));
        foreach (var b in l.TopTakers) yield return ("Top takers (days)", b.Label, b.Value.ToString("0.##"));
    }

    private static IEnumerable<(string, string, string)> AttendanceRows(AttendanceAnalyticsDto a)
    {
        yield return ("Summary", "Attendance rate %", a.AttendanceRatePct.ToString("0.0"));
        yield return ("Summary", "Total clock-ins", a.TotalClockIns.ToString());
        yield return ("Summary", "Unique active employees", a.UniqueActiveEmployees.ToString());
        yield return ("Summary", "Late clock-ins", a.LateClockIns.ToString());
        foreach (var b in a.DailyTrend) yield return ("Daily trend", b.Label, b.Value.ToString("0"));
        foreach (var b in a.ByDepartment) yield return ("By department", b.Label, b.Value.ToString("0"));
    }

    private static IEnumerable<(string, string, string)> TicketRows(TicketAnalyticsDto t)
    {
        yield return ("Summary", "Total", t.TotalTickets.ToString());
        yield return ("Summary", "Open", t.OpenCount.ToString());
        yield return ("Summary", "In progress", t.InProgressCount.ToString());
        yield return ("Summary", "Resolved", t.ResolvedCount.ToString());
        yield return ("Summary", "Closed", t.ClosedCount.ToString());
        yield return ("Summary", "Avg resolution (h)", t.AvgResolutionHours.ToString("0.0"));
        foreach (var b in t.ByCategory) yield return ("By category", b.Label, b.Value.ToString("0"));
        foreach (var b in t.ByPriority) yield return ("By priority", b.Label, b.Value.ToString("0"));
        foreach (var b in t.MonthlyCreated) yield return ("Monthly created", b.Label, b.Value.ToString("0"));
    }

    private static IEnumerable<(string, string, string)> WorkforceRows(WorkforceAnalyticsDto w)
    {
        yield return ("Summary", "Headcount", w.Headcount.ToString());
        yield return ("Summary", "Total payroll", w.TotalPayroll.ToString("0.##"));
        yield return ("Summary", "Average salary", w.AverageSalary.ToString("0.##"));
        yield return ("Summary", "Median salary", w.MedianSalary.ToString("0.##"));
        yield return ("Summary", "Male", w.MaleCount.ToString());
        yield return ("Summary", "Female", w.FemaleCount.ToString());
        yield return ("Summary", "Other", w.OtherGenderCount.ToString());
        foreach (var b in w.TenureBuckets) yield return ("Tenure", b.Label, b.Value.ToString("0"));
        foreach (var b in w.SalaryBuckets) yield return ("Salary band", b.Label, b.Value.ToString("0"));
        foreach (var b in w.HiresByMonth) yield return ("Hires by month", b.Label, b.Value.ToString("0"));
        foreach (var b in w.HeadcountByDepartment) yield return ("Headcount by dept", b.Label, b.Value.ToString("0"));
    }
}
