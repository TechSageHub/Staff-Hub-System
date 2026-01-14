using Application.Dtos;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Announcement;

public class AnnouncementService(EmployeeAppDbContext _context, UserManager<IdentityUser> _userManager) : IAnnouncementService
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
            DatePosted = DateTime.Now
        };

        await _context.Announcements.AddAsync(announcement);
        await _context.SaveChangesAsync();

        var author = await _userManager.FindByIdAsync(dto.AuthorId);
        
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

        var dtos = new List<AnnouncementDto>();
        foreach (var announcement in announcements)
        {
            var author = await _userManager.FindByIdAsync(announcement.AuthorId);
            dtos.Add(new AnnouncementDto
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                DatePosted = announcement.DatePosted,
                AuthorName = author?.UserName ?? "System",
                IsPinned = announcement.IsPinned
            });
        }

        return dtos;
    }

    public async Task<List<AnnouncementDto>> GetAllAnnouncementsAsync()
    {
        var announcements = await _context.Announcements
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.DatePosted)
            .ToListAsync();

        var dtos = new List<AnnouncementDto>();
        foreach (var announcement in announcements)
        {
            var author = await _userManager.FindByIdAsync(announcement.AuthorId);
            dtos.Add(new AnnouncementDto
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                DatePosted = announcement.DatePosted,
                AuthorName = author?.UserName ?? "System",
                IsPinned = announcement.IsPinned
            });
        }

        return dtos;
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
