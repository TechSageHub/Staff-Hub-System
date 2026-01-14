using Application.Dtos;

namespace Application.Services.Announcement;

public interface IAnnouncementService
{
    Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementDto dto);
    Task<List<AnnouncementDto>> GetRecentAnnouncementsAsync(int count = 5);
    Task<List<AnnouncementDto>> GetAllAnnouncementsAsync();
    Task<bool> DeleteAnnouncementAsync(Guid id);
}
