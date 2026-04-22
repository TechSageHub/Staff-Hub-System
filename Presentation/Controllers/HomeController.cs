using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Application.Services.Employee;
using Application.Services.Department;
using Application.Services.Announcement;
using Application.Services.Leave;
using Application.Services.Attendance;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IAnnouncementService _announcementService;
    private readonly ILeaveService _leaveService;
    private readonly IAttendanceService _attendanceService;

    public HomeController(ILogger<HomeController> logger, IEmployeeService employeeService, IDepartmentService departmentService, IAnnouncementService announcementService, ILeaveService leaveService, IAttendanceService attendanceService)
    {
        _logger = logger;
        _employeeService = employeeService;
        _departmentService = departmentService;
        _announcementService = announcementService;
        _leaveService = leaveService;
        _attendanceService = attendanceService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var announcements = await _announcementService.GetRecentAnnouncementsAsync(3);
            ViewBag.Announcements = announcements;

            if (User.IsInRole("Admin"))
            {
                var employeesDto = await _employeeService.GetAllEmployeesAsync(userId);
                var employees = employeesDto.Employees;
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var pendingLeaves = await _leaveService.GetAllPendingRequestsAsync();
                var allAnnouncements = await _announcementService.GetAllAnnouncementsAsync();

                // 1. Summary Metrics
                ViewBag.TotalEmployees = employees.Count;
                ViewBag.ActiveEmployees = employees.Count(e => !string.IsNullOrEmpty(e.UserId));
                ViewBag.InactiveEmployees = employees.Count(e => string.IsNullOrEmpty(e.UserId));
                ViewBag.IncompleteOnboarding = employees.Count(e => !e.IsOnboardingComplete);
                
                // 2. Leave Tracking
                var today = DateTime.Today;
                var allApprovedLeaves = await _leaveService.GetAllLeaveRequestsAsync("Approved");
                ViewBag.OnLeaveToday = allApprovedLeaves.Count(l => l.StartDate.Date <= today && l.EndDate.Date >= today);
                ViewBag.PendingLeavesCount = pendingLeaves.Count;
                ViewBag.RecentLeaves = pendingLeaves.OrderBy(l => l.StartDate).ToList();

                // 3. System Health / Announcements
                ViewBag.TotalAnnouncements = allAnnouncements.Count;
                ViewBag.RecentEmployees = employees.OrderByDescending(e => e.HireDate).Take(10).ToList();
                
                // 4. Staff Progress (Cumulative Headcount)
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;
                var staffProgress = new int[12];
                int baselineCount = employees.Count(e => e.HireDate.Year < currentYear);
                int runningTotal = baselineCount;
                
                for (int i = 1; i <= 12; i++)
                {
                    runningTotal += employees.Count(e => e.HireDate.Year == currentYear && e.HireDate.Month == i);
                    staffProgress[i - 1] = runningTotal;
                }
                ViewBag.StaffProgress = staffProgress;
                ViewBag.MonthlyHeadcount = staffProgress;
                ViewBag.TotalSalaryBudget = employees.Sum(e => e.Salary);

                // 5. Weekly Attendance Activity (Bar Chart)
                var weeklyAttendance = new int[7];
                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(-(6 - i));
                    var attendanceCount = await _attendanceService.GetAllAttendanceAsync(date, date);
                    weeklyAttendance[i] = attendanceCount.Count;
                }
                ViewBag.WeeklyAttendance = weeklyAttendance;

                // 6. Department breakdown
                var deptStats = departments.Departments
                    .Select(d => new { Name = d.Name, Count = employees.Count(e => e.DepartmentName == d.Name) })
                    .OrderByDescending(x => x.Count)
                    .ToList();
                ViewBag.DeptNames = deptStats.Select(d => d.Name).ToList();
                ViewBag.DeptCounts = deptStats.Select(d => d.Count).ToList();

                // 6.5. Gender breakdown
                ViewBag.MaleCount = employees.Count(e => e.Gender == "Male");
                ViewBag.FemaleCount = employees.Count(e => e.Gender == "Female");
                ViewBag.OtherGenderCount = employees.Count(e => e.Gender != "Male" && e.Gender != "Female");

                // 7. Growth logic
                var thisMonthCount = employees.Count(e => e.HireDate.Year == currentYear && e.HireDate.Month == currentMonth);
                var lastMonthDate = DateTime.Now.AddMonths(-1);
                var lastMonthCount = employees.Count(e => e.HireDate.Year == lastMonthDate.Year && e.HireDate.Month == lastMonthDate.Month);
                ViewBag.Growth = lastMonthCount == 0 ? (thisMonthCount > 0 ? 100 : 0) : (int)((double)(thisMonthCount - lastMonthCount) / lastMonthCount * 100);

                // 8. Onboarding Progress Data (for top 5 incomplete)
                ViewBag.IncompleteProfileStats = employees.Where(e => !e.IsOnboardingComplete)
                                                          .OrderBy(e => e.HireDate)
                                                          .Take(5)
                                                          .Select(e => new { 
                                                              Name = $"{e.FirstName} {e.LastName}", 
                                                              Id = e.Id,
                                                              Completion = CalculateCompletion(e) 
                                                          }).ToList();
            }
            else
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
                if (employee != null)
                {
                    ViewBag.MyEmployee = employee;
                    ViewBag.LeaveBalance = await _leaveService.GetRemainingLeaveDaysAsync(employee.Id);
                    ViewBag.TodayAttendance = await _attendanceService.GetTodayAttendanceAsync(employee.Id);
                }
            }
        }
        else
        {
            ViewData["LayoutMode"] = "Lander";
            var landingAnnouncements = await _announcementService.GetAllAnnouncementsAsync();
            ViewBag.LandingAnnouncementCount = landingAnnouncements.Count;
        }

        return View();
    }

    private static int CalculateCompletion(Application.Dtos.EmployeeDto emp)
    {
        // Simple logic based on properties filled
        int score = 0;
        if (!string.IsNullOrEmpty(emp.PhoneNumber)) score += 20;
        if (!string.IsNullOrEmpty(emp.Gender)) score += 20;
        if (!string.IsNullOrEmpty(emp.ImageUrl)) score += 20;
        if (emp.DepartmentId != Guid.Empty && emp.DepartmentId != null) score += 20;
        if (emp.HireDate != default) score += 20;
        return score;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
