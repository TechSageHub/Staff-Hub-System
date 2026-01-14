using Application.Dtos;
using Application.Services.Announcement;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize(Roles = "Admin")]
public class AnnouncementController(IAnnouncementService _announcementService, INotyfService _notyf) : Controller
{
    public async Task<IActionResult> Index()
    {
        var announcements = await _announcementService.GetAllAnnouncementsAsync();
        return View(announcements);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        dto.AuthorId = userId!;

        await _announcementService.CreateAnnouncementAsync(dto);
        _notyf.Success("Announcement posted successfully!");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _announcementService.DeleteAnnouncementAsync(id);
        if (result)
        {
            _notyf.Success("Announcement deleted.");
        }
        else
        {
            _notyf.Error("Failed to delete announcement.");
        }

        return RedirectToAction(nameof(Index));
    }
}
