using Application.Dtos;
using Application.Dtos.Paging;

namespace Application.Services.Offboarding;

public interface IOffboardingService
{
    Task<PagedResult<OffboardingListItemDto>> GetListAsync(OffboardingQuery query);
    Task<OffboardingSnapshotDto?> GetSnapshotAsync(Guid employeeId);
    Task<OffboardingSnapshotDto> StartAsync(StartOffboardingDto dto, string? initiatorUserId);
    Task<OffboardingSnapshotDto> UpdateItemAsync(UpdateOffboardingItemDto dto, string? actorUserId);
    Task<OffboardingSnapshotDto> CompleteAsync(Guid employeeId, string? actorUserId);
    Task<OffboardingSnapshotDto> CancelAsync(Guid employeeId, string? reason);
}
