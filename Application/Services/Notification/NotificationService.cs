using Application.ContractMapping;
using Application.Dtos;
using Application.Dtos.Paging;
using Application.Services.Email;
using Data.Context;
using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Notification;

public class NotificationService(
    EmployeeAppDbContext _context,
    IEmailService _emailService,
    ILogger<NotificationService> _logger) : INotificationService
{
    public async Task NotifyAsync(string userId, NotificationType type, string title, string body, string? linkUrl = null, bool sendEmail = false)
    {
        var notification = new Data.Model.Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            LinkUrl = linkUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        if (sendEmail)
        {
            await SendEmailSafelyAsync(userId, title, body);
        }
    }

    public async Task NotifyManyAsync(IEnumerable<string> userIds, NotificationType type, string title, string body, string? linkUrl = null, bool sendEmail = false)
    {
        var notifications = userIds.Distinct().Select(userId => new Data.Model.Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            LinkUrl = linkUrl,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Count == 0) return;

        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        if (sendEmail)
        {
            foreach (var userId in notifications.Select(n => n.UserId))
            {
                await SendEmailSafelyAsync(userId, title, body);
            }
        }
    }

    public Task<int> GetUnreadCountAsync(string userId)
    {
        return _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<List<NotificationDto>> GetUnreadAsync(string userId, int take = 10)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

        return notifications.Select(n => n.ToDto()).ToList();
    }

    public async Task<PagedResult<NotificationDto>> GetPagedAsync(string userId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<NotificationDto>
        {
            Items = items.Select(n => n.ToDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<bool> MarkReadAsync(string userId, Guid id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllReadAsync(string userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<PendingDigestDto>> GetPendingDigestsAsync()
    {
        var pending = await _context.Notifications
            .AsNoTracking()
            .Where(n => !n.IsRead && n.DigestedAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(1000)
            .ToListAsync();

        if (pending.Count == 0) return new List<PendingDigestDto>();

        var emails = await _context.Users
            .Where(u => pending.Select(n => n.UserId).Distinct().Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();

        var emailByUserId = emails
            .Where(u => !string.IsNullOrWhiteSpace(u.Email))
            .ToDictionary(u => u.Id, u => u.Email!);

        return pending
            .GroupBy(n => n.UserId)
            .Where(g => emailByUserId.ContainsKey(g.Key))
            .Select(g => new PendingDigestDto
            {
                UserId = g.Key,
                Email = emailByUserId[g.Key],
                Items = g.Select(n => n.ToDto()).ToList()
            })
            .ToList();
    }

    public async Task MarkDigestedAsync(IEnumerable<Guid> ids)
    {
        var now = DateTime.UtcNow;
        var notifications = await _context.Notifications
            .Where(n => ids.Contains(n.Id) && n.DigestedAt == null)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.DigestedAt = now;
        }

        await _context.SaveChangesAsync();
    }

    private async Task SendEmailSafelyAsync(string userId, string subject, string body)
    {
        try
        {
            var email = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Skipping notification email for user {UserId}: no email on record.", userId);
                return;
            }

            await _emailService.SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification email to user {UserId}.", userId);
        }
    }
}
