using Application.Dtos;
using Data.Context;
using Data.Model;
using Application.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Announcement;

public class AnnouncementService(EmployeeAppDbContext _context, UserManager<IdentityUser> _userManager, IEmailService _emailService) : IAnnouncementService
{
    public async Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementDto dto)
    {
        var announcement = new Data.Model.Announcement
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Content = dto.Content,
            AuthorId = dto.AuthorId,
            IsPinned = dto.IsPinned,
            DatePosted = DateTime.UtcNow
        };

        await _context.Announcements.AddAsync(announcement);
        await _context.SaveChangesAsync();

        var author = await _userManager.FindByIdAsync(dto.AuthorId);

        var recipients = await _userManager.Users
            .Where(u => !string.IsNullOrEmpty(u.Email))
            .Select(u => u.Email!)
            .ToListAsync();

        var subject = $"Announcement: {announcement.Title}";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <h2 style='color: #0d6efd;'>{announcement.Title}</h2>
                <p>{announcement.Content}</p>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='font-size: 12px; color: #777;'>Sent from StaffHub Employee Management System.</p>
            </div>";

        foreach (var email in recipients)
        {
            await _emailService.SendEmailAsync(email, subject, body);
        }
        
        return new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            DatePosted = announcement.DatePosted,
            AuthorName = author?.UserName ?? "System",
            IsPinned = announcement.IsPinned
        };
    }

    public async Task<List<AnnouncementDto>> GetRecentAnnouncementsAsync(int count = 5)
    {
        var announcements = await _context.Announcements
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.DatePosted)
            .Take(count)
            .ToListAsync();

        var authorIds = announcements.Select(a => a.AuthorId).Distinct().ToList();
        var authors = await _userManager.Users
            .Where(u => authorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        return announcements.Select(announcement => new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            DatePosted = announcement.DatePosted,
            AuthorName = authors.TryGetValue(announcement.AuthorId, out var name) ? name : "System",
            IsPinned = announcement.IsPinned
        }).ToList();
    }

    public async Task<List<AnnouncementDto>> GetAllAnnouncementsAsync()
    {
        var announcements = await _context.Announcements
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.DatePosted)
            .ToListAsync();

        var authorIds = announcements.Select(a => a.AuthorId).Distinct().ToList();
        var authors = await _userManager.Users
            .Where(u => authorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        return announcements.Select(announcement => new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            DatePosted = announcement.DatePosted,
            AuthorName = authors.TryGetValue(announcement.AuthorId, out var name) ? name : "System",
            IsPinned = announcement.IsPinned
        }).ToList();
    }

    public async Task<bool> DeleteAnnouncementAsync(Guid id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null) return false;

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();
        return true;
    }
}
