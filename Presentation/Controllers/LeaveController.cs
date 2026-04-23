using Application.Dtos;
using Application.Dtos.Paging;
using Application.Services.Department;
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
    IDepartmentService _departmentService,
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

    public async Task<IActionResult> Calendar(Guid? dept)
    {
        var isAdmin = User.IsInRole("Admin");
        Guid? scopedDept = dept;
        string? myDeptName = null;

        if (!isAdmin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var me = await _employeeService.GetEmployeeByUserIdAsync(userId!);
            if (me == null)
            {
                _notyf.Warning("Please complete your profile to see the team calendar.");
                return RedirectToAction(nameof(Index));
            }
            scopedDept = me.DepartmentId;
            myDeptName = me.DepartmentName;
        }

        var departments = isAdmin ? (await _departmentService.GetAllDepartmentsAsync()).Departments : new List<DepartmentDto>();

        ViewBag.IsAdmin = isAdmin;
        ViewBag.Departments = departments;
        ViewBag.SelectedDept = scopedDept;
        ViewBag.MyDeptName = myDeptName;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> CalendarEvents(DateTime start, DateTime end, Guid? dept, string? statuses)
    {
        var isAdmin = User.IsInRole("Admin");
        Guid? scopedDept = dept;

        if (!isAdmin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var me = await _employeeService.GetEmployeeByUserIdAsync(userId!);
            if (me == null) return Json(Array.Empty<object>());
            scopedDept = me.DepartmentId;
        }

        var statusList = (statuses ?? "Approved,Pending")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var leaves = await _leaveService.GetLeavesInRangeAsync(start, end, scopedDept, statusList);

        var events = leaves.Select(l => new
        {
            id = l.Id,
            title = $"{l.EmployeeName} · {l.LeaveType}",
            start = l.StartDate.ToString("yyyy-MM-dd"),
            end = l.EndDate.AddDays(1).ToString("yyyy-MM-dd"),
            allDay = true,
            color = StatusColor(l.Status),
            extendedProps = new
            {
                employee = l.EmployeeName,
                department = l.DepartmentName,
                leaveType = l.LeaveType,
                status = l.Status,
                reason = l.Reason,
                days = l.DurationInDays
            }
        });

        return Json(events);
    }

    private static string StatusColor(string status) => status switch
    {
        "Approved" => "#10b981",
        "Pending" => "#f59e0b",
        "Rejected" => "#ef4444",
        "Cancelled" => "#64748b",
        _ => "#1d4ed8"
    };

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
