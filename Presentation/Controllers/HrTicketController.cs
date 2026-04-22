using Application.Dtos;
using Application.Dtos.Paging;
using Application.Services.Employee;
using Application.Services.HrTicket;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
public class HrTicketController(
    IHrTicketService _hrTicketService,
    IEmployeeService _employeeService,
    INotyfService _notyf) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(nameof(AdminDashboard));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        if (employee == null)
        {
            _notyf.Warning("Please complete your profile before submitting a support ticket.");
            return RedirectToAction("Index", "Home");
        }

        var tickets = await _hrTicketService.GetEmployeeTicketsAsync(employee.Id);
        return View(tickets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateHrTicketDto());
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var ticket = await _hrTicketService.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
            if (employee == null || employee.Id != ticket.EmployeeId)
            {
                return Forbid();
            }
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHrTicketDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        if (employee == null)
        {
            _notyf.Warning("Please complete your profile before submitting a support ticket.");
            return RedirectToAction("Index", "Home");
        }

        dto.EmployeeId = employee.Id;
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _hrTicketService.CreateTicketAsync(dto);
        _notyf.Success("Support ticket submitted. HR will review it shortly.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard(string? status, string? search, int page = 1, int pageSize = 20)
    {
        var query = new HrTicketQuery
        {
            Status = status,
            Search = search,
            Page = page,
            PageSize = pageSize
        };

        var paged = await _hrTicketService.GetAllTicketsPagedAsync(query);
        ViewBag.SelectedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status;
        ViewBag.Search = search;
        return View(paged);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateHrTicketStatusDto dto)
    {
        var result = await _hrTicketService.UpdateTicketStatusAsync(dto);
        if (result)
        {
            _notyf.Success("Ticket updated successfully.");
        }
        else
        {
            _notyf.Error("Failed to update ticket.");
        }

        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddHrTicketCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = dto.HrTicketId });
        }

        var ticket = await _hrTicketService.GetTicketByIdAsync(dto.HrTicketId);
        if (ticket == null)
        {
            return NotFound();
        }

        string commenterName;
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
            if (employee == null || employee.Id != ticket.EmployeeId)
            {
                return Forbid();
            }

            commenterName = $"{employee.FirstName} {employee.LastName}";
        }
        else
        {
            commenterName = User.Identity?.Name ?? "Admin";
        }

        var success = await _hrTicketService.AddCommentAsync(dto, commenterName, isAdmin);
        if (success)
        {
            _notyf.Success("Comment added.");
        }
        else
        {
            _notyf.Error("Failed to add comment.");
        }

        return RedirectToAction(nameof(Details), new { id = dto.HrTicketId });
    }
}
