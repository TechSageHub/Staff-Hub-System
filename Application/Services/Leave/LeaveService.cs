using Application.ContractMapping;
using Application.Dtos;
using Application.Dtos.Paging;
using Data.Context;
using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Leave;

public class LeaveService(EmployeeAppDbContext _context, ILogger<LeaveService> _logger) : ILeaveService
{
    private const int AnnualLeaveEntitlement = 20;

    public async Task<LeaveRequestDto> RequestLeaveAsync(CreateLeaveRequestDto dto)
    {
        _logger.LogInformation("Creating leave request for EmployeeId: {EmployeeId}", dto.EmployeeId);

        var startDate = dto.StartDate.Date;
        var endDate = dto.EndDate.Date;

        if (endDate < startDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        var workingDaysRequested = CountWorkingDays(startDate, endDate);
        if (workingDaysRequested <= 0)
        {
            throw new InvalidOperationException("Leave request must include at least one working day.");
        }

        var hasPending = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == dto.EmployeeId && l.Status == LeaveStatus.Pending);

        if (hasPending)
        {
            throw new InvalidOperationException("You already have a pending leave request.");
        }

        var today = DateTime.Today;
        var hasActive = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == dto.EmployeeId
                           && l.Status == LeaveStatus.Approved
                           && l.StartDate.Date <= today
                           && l.EndDate.Date >= today);

        if (hasActive)
        {
            throw new InvalidOperationException("You currently have an active leave.");
        }

        var overlaps = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == dto.EmployeeId
                           && (l.Status == LeaveStatus.Pending || l.Status == LeaveStatus.Approved)
                           && startDate <= l.EndDate.Date
                           && endDate >= l.StartDate.Date);

        if (overlaps)
        {
            throw new InvalidOperationException("Requested dates overlap with an existing leave.");
        }

        var remainingBalance = await GetRemainingLeaveDaysAsync(dto.EmployeeId);
        if (workingDaysRequested > remainingBalance)
        {
            throw new InvalidOperationException("Requested leave exceeds remaining annual leave balance.");
        }

        var leave = dto.ToModel();
        leave.StartDate = startDate;
        leave.EndDate = endDate;

        await _context.LeaveRequests.AddAsync(leave);
        await _context.SaveChangesAsync();

        // Fetch again to include employee info for the DTO
        var savedLeave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == leave.Id);

        var dtoResult = savedLeave!.ToDto();
        dtoResult.DurationInDays = CountWorkingDays(savedLeave.StartDate, savedLeave.EndDate);
        return dtoResult;
    }

    public async Task<List<LeaveRequestDto>> GetEmployeeLeaveHistoryAsync(Guid employeeId)
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.DateRequested)
            .ToListAsync();

        return leaves.Select(l =>
        {
            var dto = l.ToDto();
            dto.DurationInDays = CountWorkingDays(l.StartDate, l.EndDate);
            return dto;
        }).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetAllLeaveRequestsAsync(string? status = null)
    {
        IQueryable<LeaveRequest> query = _context.LeaveRequests.Include(l => l.Employee);

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<LeaveStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(l => l.Status == parsedStatus);
            }
        }

        var leaves = await query
            .OrderByDescending(l => l.DateRequested)
            .ToListAsync();

        return leaves.Select(l =>
        {
            var dto = l.ToDto();
            dto.DurationInDays = CountWorkingDays(l.StartDate, l.EndDate);
            return dto;
        }).ToList();
    }

    public async Task<PagedResult<LeaveRequestDto>> GetAllLeaveRequestsPagedAsync(LeaveQuery q)
    {
        IQueryable<LeaveRequest> query = _context.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e!.Department);

        if (!string.IsNullOrWhiteSpace(q.Status) && !string.Equals(q.Status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<LeaveStatus>(q.Status, true, out var parsedStatus))
            {
                query = query.Where(l => l.Status == parsedStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            query = query.Where(l =>
                l.Employee != null &&
                (EF.Functions.Like(l.Employee.FirstName, pattern) ||
                 EF.Functions.Like(l.Employee.LastName, pattern) ||
                 EF.Functions.Like(l.LeaveType, pattern)));
        }

        query = query.OrderByDescending(l => l.DateRequested);

        var total = await query.CountAsync();
        var leaves = await query.Skip(q.Skip).Take(q.PageSize).ToListAsync();

        var items = leaves.Select(l =>
        {
            var dto = l.ToDto();
            dto.DurationInDays = CountWorkingDays(l.StartDate, l.EndDate);
            return dto;
        }).ToList();

        return new PagedResult<LeaveRequestDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<List<LeaveRequestDto>> GetAllPendingRequestsAsync()
    {
        return await GetAllLeaveRequestsAsync(LeaveStatus.Pending.ToString());
    }

    public async Task<bool> ProcessLeaveRequestAsync(LeaveApprovalDto approvalDto)
    {
        var leave = await _context.LeaveRequests.FindAsync(approvalDto.LeaveId);
        if (leave == null) return false;

        if (leave.Status != LeaveStatus.Pending) return false;

        if (!Enum.TryParse<LeaveStatus>(approvalDto.Status, true, out var parsedStatus))
        {
            return false;
        }

        if (parsedStatus == LeaveStatus.Approved)
        {
            var requestedWorkingDays = CountWorkingDays(leave.StartDate, leave.EndDate);
            if (requestedWorkingDays <= 0)
            {
                return false;
            }

            var today = DateTime.Today;
            var adjustedStart = leave.StartDate.Date <= today ? today.AddDays(1) : leave.StartDate.Date;
            var adjustedEnd = AddWorkingDays(adjustedStart, requestedWorkingDays);

            var overlap = await _context.LeaveRequests
                .AnyAsync(l => l.EmployeeId == leave.EmployeeId
                               && l.Id != leave.Id
                               && l.Status == LeaveStatus.Approved
                               && adjustedStart <= l.EndDate.Date
                               && adjustedEnd >= l.StartDate.Date);

            if (overlap)
            {
                return false;
            }

            var remaining = await GetRemainingLeaveDaysAsync(leave.EmployeeId);
            if (requestedWorkingDays > remaining)
            {
                return false;
            }

            leave.StartDate = adjustedStart;
            leave.EndDate = adjustedEnd;
        }

        leave.Status = parsedStatus;
        leave.AdminComment = approvalDto.AdminComment;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetRemainingLeaveDaysAsync(Guid employeeId)
    {
        var year = DateTime.Today.Year;
        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = new DateTime(year, 12, 31);

        var approvedLeaveDays = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Approved)
            .ToListAsync();

        var usedDays = approvedLeaveDays.Sum(l =>
        {
            var start = l.StartDate.Date < yearStart ? yearStart : l.StartDate.Date;
            var end = l.EndDate.Date > yearEnd ? yearEnd : l.EndDate.Date;
            if (end < start) return 0;
            return CountWorkingDays(start, end);
        });

        return Math.Max(0, AnnualLeaveEntitlement - usedDays);
    }

    public async Task<ActiveLeaveStatusDto?> GetActiveLeaveStatusAsync(Guid employeeId)
    {
        var today = DateTime.Today;
        var leave = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Approved)
            .OrderByDescending(l => l.StartDate)
            .FirstOrDefaultAsync(l => l.StartDate.Date <= today && l.EndDate.Date >= today);

        if (leave == null)
        {
            return null;
        }

        return new ActiveLeaveStatusDto
        {
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            RemainingWorkingDays = CountWorkingDays(today, leave.EndDate.Date)
        };
    }

    public async Task<List<LeaveRequestDto>> GetLeavesInRangeAsync(DateTime from, DateTime to, Guid? departmentId = null, IEnumerable<string>? statuses = null)
    {
        var fromDate = from.Date;
        var toDate = to.Date;

        IQueryable<LeaveRequest> query = _context.LeaveRequests
            .Include(l => l.Employee)
            .ThenInclude(e => e!.Department)
            .Where(l => l.StartDate.Date <= toDate && l.EndDate.Date >= fromDate);

        if (departmentId.HasValue)
        {
            query = query.Where(l => l.Employee != null && l.Employee.DepartmentId == departmentId.Value);
        }

        var parsedStatuses = statuses?
            .Select(s => Enum.TryParse<LeaveStatus>(s, true, out var ps) ? (LeaveStatus?)ps : null)
            .Where(ps => ps.HasValue)
            .Select(ps => ps!.Value)
            .ToList();
        if (parsedStatuses != null && parsedStatuses.Count > 0)
        {
            query = query.Where(l => parsedStatuses.Contains(l.Status));
        }

        var leaves = await query.OrderBy(l => l.StartDate).ToListAsync();

        return leaves.Select(l =>
        {
            var dto = l.ToDto();
            dto.DurationInDays = CountWorkingDays(l.StartDate, l.EndDate);
            return dto;
        }).ToList();
    }

    private static int CountWorkingDays(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        if (end < start) return 0;

        var count = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private static DateTime AddWorkingDays(DateTime startDate, int workingDays)
    {
        if (workingDays <= 0) return startDate.Date;

        var date = startDate.Date;
        var count = 0;
        while (true)
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                count++;
                if (count >= workingDays)
                {
                    return date;
                }
            }

            date = date.AddDays(1);
        }
    }
}
