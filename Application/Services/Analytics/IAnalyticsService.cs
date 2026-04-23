using Application.Dtos;

namespace Application.Services.Analytics;

public interface IAnalyticsService
{
    Task<AnalyticsPageDto> GetAnalyticsAsync(AnalyticsFilter filter);
    Task<LeaveAnalyticsDto> GetLeaveAnalyticsAsync(AnalyticsFilter filter);
    Task<AttendanceAnalyticsDto> GetAttendanceAnalyticsAsync(AnalyticsFilter filter);
    Task<TicketAnalyticsDto> GetTicketAnalyticsAsync(AnalyticsFilter filter);
    Task<WorkforceAnalyticsDto> GetWorkforceAnalyticsAsync(AnalyticsFilter filter);
}
