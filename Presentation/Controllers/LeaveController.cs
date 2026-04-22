using Application.Dtos;
using Application.Dtos.Paging;
using Application.Services.Employee;
using Application.Services.Leave;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
public class LeaveController(
    ILeaveService _leaveService, 
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
            _notyf.Warning("Please complete your profile before requesting leave.");
            return RedirectToAction("Index", "Home");
        }

        var history = await _leaveService.GetEmployeeLeaveHistoryAsync(employee.Id);
        ViewBag.RemainingDays = await _leaveService.GetRemainingLeaveDaysAsync(employee.Id);
        ViewBag.ActiveLeave = await _leaveService.GetActiveLeaveStatusAsync(employee.Id);
        
        return View(history);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        
        if (employee == null) return RedirectToAction("Index", "Home");

        ViewBag.RemainingDays = await _leaveService.GetRemainingLeaveDaysAsync(employee.Id);
        return View(new CreateLeaveRequestDto { EmployeeId = employee.Id, StartDate = DateTime.Today.AddDays(7), EndDate = DateTime.Today.AddDays(14) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLeaveRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        if (employee == null)
        {
            _notyf.Warning("Please complete your profile before requesting leave.");
            return RedirectToAction("Index", "Home");
        }

        dto.EmployeeId = employee.Id;

        if (!ModelState.IsValid) return View(dto);

        if (dto.StartDate < DateTime.Today)
        {
            ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            return View(dto);
        }

        if (dto.EndDate < dto.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date must be after Start date.");
            return View(dto);
        }

        var remaining = await _leaveService.GetRemainingLeaveDaysAsync(employee.Id);
        var requestedDays = CountWorkingDays(dto.StartDate, dto.EndDate);
        if (requestedDays > remaining)
        {
            ModelState.AddModelError("", $"Requested leave exceeds your remaining annual leave balance ({remaining} days).");
            return View(dto);
        }

        try
        {
            await _leaveService.RequestLeaveAsync(dto);
            _notyf.Success("Leave request submitted successfully!");
        }
        catch (InvalidOperationException ex)
        {
            _notyf.Error(ex.Message);
            return View(dto);
        }
        
        return RedirectToAction(nameof(Index));
    }

    private static int CountWorkingDays(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        if (end < start) return 0;

        var count = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard(string? status, string? search, int page = 1, int pageSize = 20)
    {
        var query = new LeaveQuery
        {
            Status = status,
            Search = search,
            Page = page,
            PageSize = pageSize
        };

        var paged = await _leaveService.GetAllLeaveRequestsPagedAsync(query);
        ViewBag.SelectedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status;
        ViewBag.Search = search;
        return View(paged);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(LeaveApprovalDto dto)
    {
        var result = await _leaveService.ProcessLeaveRequestAsync(dto);
        if (result)
        {
            _notyf.Success($"Leave marked as {dto.Status}.");
        }
        else
        {
            _notyf.Error("Failed to process leave request.");
        }
        
        return RedirectToAction(nameof(AdminDashboard));
    }
}
