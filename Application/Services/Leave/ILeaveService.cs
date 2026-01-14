using Application.Dtos;

namespace Application.Services.Leave;

public interface ILeaveService
{
    Task<LeaveRequestDto> RequestLeaveAsync(CreateLeaveRequestDto dto);
    Task<List<LeaveRequestDto>> GetEmployeeLeaveHistoryAsync(Guid employeeId);
    Task<List<LeaveRequestDto>> GetAllPendingRequestsAsync();
    Task<bool> ProcessLeaveRequestAsync(LeaveApprovalDto approvalDto);
    Task<int> GetRemainingLeaveDaysAsync(Guid employeeId);
}
