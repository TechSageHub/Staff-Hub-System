using Application.Dtos;
using Application.Dtos.Paging;
using Data.Model;

namespace Application.Services.Notification;

public interface INotificationService
{
    Task NotifyAsync(string userId, NotificationType type, string title, string body, string? linkUrl = null, bool sendEmail = false);

    Task NotifyManyAsync(IEnumerable<string> userIds, NotificationType type, string title, string body, string? linkUrl = null, bool sendEmail = false);

    Task<int> GetUnreadCountAsync(string userId);

    Task<List<NotificationDto>> GetUnreadAsync(string userId, int take = 10);

    Task<PagedResult<NotificationDto>> GetPagedAsync(string userId, int page, int pageSize);

    Task<bool> MarkReadAsync(string userId, Guid id);

    Task MarkAllReadAsync(string userId);

    Task<List<PendingDigestDto>> GetPendingDigestsAsync();

    Task MarkDigestedAsync(IEnumerable<Guid> ids);
}
