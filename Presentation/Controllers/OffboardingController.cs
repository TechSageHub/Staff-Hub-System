using Application.Dtos;
using Application.Services.Employee;
using Application.Services.Offboarding;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize(Roles = "Admin")]
public class OffboardingController(
    IOffboardingService _offboardingService,
    IEmployeeService _employeeService,
    INotyfService _notyf) : Controller
{
    public async Task<IActionResult> Index(string? status, string? search, int page = 1, int pageSize = 20)
    {
        var paged = await _offboardingService.GetListAsync(new OffboardingQuery
        {
            Status = status,
            Search = search,
            Page = page,
            PageSize = pageSize
        });

        ViewBag.SelectedStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.ToLowerInvariant();
        ViewBag.Search = search;
        return View(paged);
    }

    [HttpGet]
    public async Task<IActionResult> Start(Guid employeeId)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
        {
            _notyf.Error("Employee not found.");
            return RedirectToAction(nameof(Index));
        }

        var existing = await _offboardingService.GetSnapshotAsync(employeeId);
        if (existing != null)
        {
            return RedirectToAction(nameof(Detail), new { employeeId });
        }

        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.DepartmentName = employee.DepartmentName;
        return View(new StartOffboardingDto
        {
            EmployeeId = employeeId,
            LastWorkingDay = DateTime.Today.AddDays(14)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(StartOffboardingDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Reason))
        {
            ModelState.AddModelError(nameof(dto.Reason), "Please provide a reason.");
            var employee = await _employeeService.GetEmployeeByIdAsync(dto.EmployeeId);
            ViewBag.EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Employee";
            ViewBag.DepartmentName = employee?.DepartmentName;
            return View(dto);
        }

        try
        {
            var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _offboardingService.StartAsync(dto, actor);
            _notyf.Success("Offboarding started.");
            return RedirectToAction(nameof(Detail), new { employeeId = dto.EmployeeId });
        }
        catch (InvalidOperationException ex)
        {
            _notyf.Error(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(Guid employeeId)
    {
        var snapshot = await _offboardingService.GetSnapshotAsync(employeeId);
        if (snapshot == null)
        {
            _notyf.Warning("No offboarding found for this employee.");
            return RedirectToAction(nameof(Index));
        }

        return View(snapshot);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItem(UpdateOffboardingItemDto dto)
    {
        try
        {
            var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var snapshot = await _offboardingService.UpdateItemAsync(dto, actor);
            return Json(new
            {
                ok = true,
                progressPercent = snapshot.ProgressPercent,
                canComplete = snapshot.CanComplete
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { ok = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid employeeId)
    {
        try
        {
            var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _offboardingService.CompleteAsync(employeeId, actor);
            _notyf.Success("Offboarding completed.");
        }
        catch (InvalidOperationException ex)
        {
            _notyf.Error(ex.Message);
        }
        return RedirectToAction(nameof(Detail), new { employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid employeeId, string? cancellationReason)
    {
        try
        {
            await _offboardingService.CancelAsync(employeeId, cancellationReason);
            _notyf.Success("Offboarding cancelled.");
        }
        catch (InvalidOperationException ex)
        {
            _notyf.Error(ex.Message);
        }
        return RedirectToAction(nameof(Detail), new { employeeId });
    }
}
