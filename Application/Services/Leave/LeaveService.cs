using Application.ContractMapping;
using Application.Dtos;
using Data.Context;
using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Leave;

public class LeaveService(EmployeeAppDbContext _context, ILogger<LeaveService> _logger) : ILeaveService
{
    public async Task<LeaveRequestDto> RequestLeaveAsync(CreateLeaveRequestDto dto)
    {
        _logger.LogInformation("Creating leave request for EmployeeId: {EmployeeId}", dto.EmployeeId);
        
        var leave = dto.ToModel();
        
        await _context.LeaveRequests.AddAsync(leave);
        await _context.SaveChangesAsync();
        
        // Fetch again to include employee info for the DTO
        var savedLeave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == leave.Id);
            
        return savedLeave!.ToDto();
    }

    public async Task<List<LeaveRequestDto>> GetEmployeeLeaveHistoryAsync(Guid employeeId)
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.DateRequested)
            .ToListAsync();
            
        return leaves.Select(l => l.ToDto()).ToList();
    }

    public async Task<List<LeaveRequestDto>> GetAllPendingRequestsAsync()
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.Status == LeaveStatus.Pending)
            .OrderBy(l => l.DateRequested)
            .ToListAsync();
            
        return leaves.Select(l => l.ToDto()).ToList();
    }

    public async Task<bool> ProcessLeaveRequestAsync(LeaveApprovalDto approvalDto)
    {
        var leave = await _context.LeaveRequests.FindAsync(approvalDto.LeaveId);
        if (leave == null) return false;
        
        leave.Status = approvalDto.Approved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.AdminComment = approvalDto.AdminComment;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetRemainingLeaveDaysAsync(Guid employeeId)
    {
        // Simple logic: 21 days annual leave total
        var approvedLeaveDays = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Approved && l.LeaveType == "Annual")
            .ToListAsync();
            
        int usedDays = approvedLeaveDays.Sum(l => (l.EndDate - l.StartDate).Days + 1);
        
        return Math.Max(0, 21 - usedDays);
    }
}
