using Application.Dtos;
using Application.Services.Attendance;
using Application.Services.Employee;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
public class AttendanceController(
    IAttendanceService _attendanceService,
    IEmployeeService _employeeService,
    INotyfService _notyf) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);

        if (employee == null)
        {
            _notyf.Warning("Please complete your profile first.");
            return RedirectToAction("Index", "Home");
        }

        var todayAttendance = await _attendanceService.GetTodayAttendanceAsync(employee.Id);
        var history = await _attendanceService.GetEmployeeAttendanceHistoryAsync(employee.Id, 30);

        ViewBag.TodayAttendance = todayAttendance;
        return View(history);
    }

    [HttpPost]
    public async Task<IActionResult> ClockIn()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);

        if (employee == null)
        {
            _notyf.Error("Employee profile not found.");
            return RedirectToAction("Index", "Home");
        }

        try
        {
            await _attendanceService.ClockInAsync(employee.Id);
            _notyf.Success($"Clocked in at {DateTime.Now:HH:mm}");
        }
        catch (InvalidOperationException ex)
        {
            _notyf.Warning(ex.Message);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ClockOut()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);

        if (employee == null)
        {
            _notyf.Error("Employee profile not found.");
            return RedirectToAction("Index", "Home");
        }

        var result = await _attendanceService.ClockOutAsync(employee.Id);
        if (result != null)
        {
            _notyf.Success($"Clocked out at {DateTime.Now:HH:mm}. Total: {result.TotalHours}");
        }
        else
        {
            _notyf.Warning("No active clock-in found for today.");
        }

        return RedirectToAction(nameof(Index));
    }
}
