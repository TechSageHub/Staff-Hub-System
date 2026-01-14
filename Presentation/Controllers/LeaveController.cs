using Application.Dtos;
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        
        if (employee == null)
        {
            _notyf.Warning("Please complete your profile before requesting leave.");
            return RedirectToAction("Index", "Home");
        }

        var history = await _leaveService.GetEmployeeLeaveHistoryAsync(employee.Id);
        ViewBag.RemainingDays = await _leaveService.GetRemainingLeaveDaysAsync(employee.Id);
        
        return View(history);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        
        if (employee == null) return RedirectToAction("Index", "Home");

        return View(new CreateLeaveRequestDto { EmployeeId = employee.Id, StartDate = DateTime.Today.AddDays(7), EndDate = DateTime.Today.AddDays(14) });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLeaveRequestDto dto)
    {
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

        await _leaveService.RequestLeaveAsync(dto);
        _notyf.Success("Leave request submitted successfully!");
        
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        var pendingRequests = await _leaveService.GetAllPendingRequestsAsync();
        return View(pendingRequests);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Process(LeaveApprovalDto dto)
    {
        var result = await _leaveService.ProcessLeaveRequestAsync(dto);
        if (result)
        {
            _notyf.Success(dto.Approved ? "Leave approved!" : "Leave rejected.");
        }
        else
        {
            _notyf.Error("Failed to process leave request.");
        }
        
        return RedirectToAction(nameof(AdminDashboard));
    }
}
