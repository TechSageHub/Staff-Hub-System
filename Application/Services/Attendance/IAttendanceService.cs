using Application.Dtos;

namespace Application.Services.Attendance;

public interface IAttendanceService
{
    Task<AttendanceLogDto> ClockInAsync(Guid employeeId);
    Task<AttendanceLogDto?> ClockOutAsync(Guid employeeId);
    Task<AttendanceLogDto?> GetTodayAttendanceAsync(Guid employeeId);
    Task<List<AttendanceLogDto>> GetEmployeeAttendanceHistoryAsync(Guid employeeId, int days = 30);
    Task<List<AttendanceLogDto>> GetAllAttendanceAsync(DateTime? from = null, DateTime? to = null, Guid? employeeId = null);
}
