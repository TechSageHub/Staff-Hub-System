using Application.Dtos;
using Application.Dtos.Paging;

namespace Application.Services.Leave;

public interface ILeaveService
{
    Task<LeaveRequestDto> RequestLeaveAsync(CreateLeaveRequestDto dto);
    Task<List<LeaveRequestDto>> GetEmployeeLeaveHistoryAsync(Guid employeeId);
    Task<List<LeaveRequestDto>> GetAllLeaveRequestsAsync(string? status = null);
    Task<PagedResult<LeaveRequestDto>> GetAllLeaveRequestsPagedAsync(LeaveQuery query);
    Task<List<LeaveRequestDto>> GetAllPendingRequestsAsync();
    Task<bool> ProcessLeaveRequestAsync(LeaveApprovalDto approvalDto);
    Task<int> GetRemainingLeaveDaysAsync(Guid employeeId);
    Task<ActiveLeaveStatusDto?> GetActiveLeaveStatusAsync(Guid employeeId);
    Task<List<LeaveRequestDto>> GetLeavesInRangeAsync(DateTime from, DateTime to, Guid? departmentId = null, IEnumerable<string>? statuses = null);
}
