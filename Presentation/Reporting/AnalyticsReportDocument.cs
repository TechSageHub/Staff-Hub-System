using Application.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Presentation.Reporting;

public class AnalyticsReportDocument(AnalyticsPageDto data, string orgName) : IDocument
{
    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"{orgName} · Analytics Report",
        Author = orgName,
        CreationDate = DateTime.Now
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeBody);
            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Page ").FontSize(9).FontColor(Colors.Grey.Darken1);
                t.CurrentPageNumber().FontSize(9);
                t.Span(" of ").FontSize(9).FontColor(Colors.Grey.Darken1);
                t.TotalPages().FontSize(9);
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text($"{orgName} · Analytics Report").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
            col.Item().Text($"Period: {data.Filter.From:MMM dd, yyyy} → {data.Filter.To:MMM dd, yyyy}")
                .FontSize(10).FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeBody(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            col.Spacing(16);
            col.Item().Element(c => Section(c, "Leave", () => LeaveSection(c)));
            col.Item().Element(c => Section(c, "Attendance", () => AttendanceSection(c)));
            col.Item().Element(c => Section(c, "HR tickets", () => TicketSection(c)));
            col.Item().Element(c => Section(c, "Workforce", () => WorkforceSection(c)));
        });
    }

    private static void Section(IContainer c, string title, Action body)
    {
        c.Column(col =>
        {
            col.Item().PaddingBottom(6).Text(title).FontSize(13).SemiBold().FontColor(Colors.Blue.Darken2);
            body();
        });
    }

    private void LeaveSection(IContainer c)
    {
        var l = data.Leave;
        c.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(x => Kpi(x, "Requests", l.TotalRequests.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Days taken", l.TotalDaysTaken.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Pending", l.PendingCount.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Approved", l.ApprovedCount.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Rejected", l.RejectedCount.ToString()));
            });
            col.Item().Element(x => BucketTable(x, "By type (days)", l.ByType));
            col.Item().Element(x => BucketTable(x, "Top 5 leave takers (days)", l.TopTakers));
        });
    }

    private void AttendanceSection(IContainer c)
    {
        var a = data.Attendance;
        c.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(x => Kpi(x, "Attendance rate", $"{a.AttendanceRatePct}%"));
                row.RelativeItem().Element(x => Kpi(x, "Clock-ins", a.TotalClockIns.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Active staff", a.UniqueActiveEmployees.ToString()));
                row.RelativeItem().Element(x => Kpi(x, $"Late (>{a.LateThreshold:hh\\:mm})", a.LateClockIns.ToString()));
            });
            col.Item().Element(x => BucketTable(x, "Clock-ins by department", a.ByDepartment));
        });
    }

    private void TicketSection(IContainer c)
    {
        var t = data.Tickets;
        c.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(x => Kpi(x, "Total", t.TotalTickets.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Open", t.OpenCount.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "In progress", t.InProgressCount.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Resolved", t.ResolvedCount.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Avg resolve (h)", t.AvgResolutionHours.ToString("0.0")));
            });
            col.Item().Element(x => BucketTable(x, "By category", t.ByCategory));
            col.Item().Element(x => BucketTable(x, "By priority", t.ByPriority));
        });
    }

    private void WorkforceSection(IContainer c)
    {
        var w = data.Workforce;
        c.Column(col =>
        {
            col.Spacing(6);
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(x => Kpi(x, "Headcount", w.Headcount.ToString()));
                row.RelativeItem().Element(x => Kpi(x, "Payroll", w.TotalPayroll.ToString("C0")));
                row.RelativeItem().Element(x => Kpi(x, "Avg salary", w.AverageSalary.ToString("C0")));
                row.RelativeItem().Element(x => Kpi(x, "Median salary", w.MedianSalary.ToString("C0")));
            });
            col.Item().Element(x => BucketTable(x, "Tenure distribution", w.TenureBuckets));
            col.Item().Element(x => BucketTable(x, "Salary distribution", w.SalaryBuckets));
            col.Item().Element(x => BucketTable(x, "Headcount by department", w.HeadcountByDepartment));
        });
    }

    private static void Kpi(IContainer c, string label, string value)
    {
        c.PaddingRight(6).Background(Colors.Grey.Lighten4).Padding(8).Column(col =>
        {
            col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
            col.Item().Text(value).FontSize(13).SemiBold();
        });
    }

    private static void BucketTable(IContainer c, string title, List<AnalyticsBucket> rows)
    {
        c.Column(col =>
        {
            col.Item().PaddingTop(4).Text(title).FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
            if (rows.Count == 0)
            {
                col.Item().Text("No data for selected period.").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                return;
            }
            col.Item().Table(tbl =>
            {
                tbl.ColumnsDefinition(cols => { cols.RelativeColumn(3); cols.RelativeColumn(1); });
                foreach (var row in rows)
                {
                    tbl.Cell().PaddingVertical(2).Text(row.Label).FontSize(9);
                    tbl.Cell().PaddingVertical(2).AlignRight()
                        .Text(row.Value.ToString("0.##")).FontSize(9).SemiBold();
                }
            });
        });
    }
}
