using Application.Dtos;
using Data.Context;
using Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Attendance;

public class AttendanceService(EmployeeAppDbContext _context) : IAttendanceService
{
    public async Task<AttendanceLogDto> ClockInAsync(Guid employeeId)
    {
        var today = DateTime.Today;
        var existingLog = await _context.AttendanceLogs
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ClockInTime.Date == today && !a.ClockOutTime.HasValue);

        if (existingLog != null)
        {
            throw new InvalidOperationException("You are already clocked in for today.");
        }

        var log = new AttendanceLog
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ClockInTime = DateTime.Now
        };

        await _context.AttendanceLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        var employee = await _context.Employees.FindAsync(employeeId);

        return new AttendanceLogDto
        {
            Id = log.Id,
            EmployeeId = log.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
            ClockInTime = log.ClockInTime,
            ClockOutTime = log.ClockOutTime
        };
    }

    public async Task<AttendanceLogDto?> ClockOutAsync(Guid employeeId)
    {
        var today = DateTime.Today;
        var log = await _context.AttendanceLogs
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ClockInTime.Date == today && !a.ClockOutTime.HasValue);

        if (log == null) return null;

        log.ClockOutTime = DateTime.Now;
        await _context.SaveChangesAsync();

        var totalHours = log.TotalHours.HasValue ? $"{log.TotalHours.Value.Hours}h {log.TotalHours.Value.Minutes}m" : null;

        return new AttendanceLogDto
        {
            Id = log.Id,
            EmployeeId = log.EmployeeId,
            EmployeeName = $"{log.Employee.FirstName} {log.Employee.LastName}",
            ClockInTime = log.ClockInTime,
            ClockOutTime = log.ClockOutTime,
            TotalHours = totalHours
        };
    }

    public async Task<AttendanceLogDto?> GetTodayAttendanceAsync(Guid employeeId)
    {
        var today = DateTime.Today;
        var log = await _context.AttendanceLogs
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ClockInTime.Date == today);

        if (log == null) return null;

        var totalHours = log.TotalHours.HasValue ? $"{log.TotalHours.Value.Hours}h {log.TotalHours.Value.Minutes}m" : null;

        return new AttendanceLogDto
        {
            Id = log.Id,
            EmployeeId = log.EmployeeId,
            EmployeeName = $"{log.Employee.FirstName} {log.Employee.LastName}",
            ClockInTime = log.ClockInTime,
            ClockOutTime = log.ClockOutTime,
            TotalHours = totalHours
        };
    }

    public async Task<List<AttendanceLogDto>> GetEmployeeAttendanceHistoryAsync(Guid employeeId, int days = 30)
    {
        var cutoffDate = DateTime.Today.AddDays(-days);
        var logs = await _context.AttendanceLogs
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId && a.ClockInTime >= cutoffDate)
            .OrderByDescending(a => a.ClockInTime)
            .ToListAsync();

        return logs.Select(log => new AttendanceLogDto
        {
            Id = log.Id,
            EmployeeId = log.EmployeeId,
            EmployeeName = $"{log.Employee.FirstName} {log.Employee.LastName}",
            ClockInTime = log.ClockInTime,
            ClockOutTime = log.ClockOutTime,
            TotalHours = log.TotalHours.HasValue ? $"{log.TotalHours.Value.Hours}h {log.TotalHours.Value.Minutes}m" : null,
            Notes = log.Notes
        }).ToList();
    }

    public async Task<List<AttendanceLogDto>> GetAllAttendanceAsync(DateTime? from = null, DateTime? to = null, Guid? employeeId = null)
    {
        var query = _context.AttendanceLogs
            .Include(a => a.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(a => a.ClockInTime.Date >= from.Value.Date);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.ClockInTime.Date <= to.Value.Date);
        }

        var logs = await query
            .OrderByDescending(a => a.ClockInTime)
            .ToListAsync();

        return logs.Select(log => new AttendanceLogDto
        {
            Id = log.Id,
            EmployeeId = log.EmployeeId,
            EmployeeName = $"{log.Employee.FirstName} {log.Employee.LastName}",
            ClockInTime = log.ClockInTime,
            ClockOutTime = log.ClockOutTime,
            TotalHours = log.TotalHours.HasValue ? $"{log.TotalHours.Value.Hours}h {log.TotalHours.Value.Minutes}m" : null,
            Notes = log.Notes
        }).ToList();
    }
}
