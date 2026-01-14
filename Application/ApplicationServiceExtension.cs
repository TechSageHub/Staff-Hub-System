using Application.Services.Authentication;
using Application.Services.Department;
using Application.Services.Email;
using Application.Services.Employee;
using Application.Services.Leave;
using Application.Services.Announcement;
using Application.Services.Attendance;
using Microsoft.Extensions.DependencyInjection;

namespace Application;
public static class ApplicationServiceExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEmployeeService, EmployeeService>(); 
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IAttendanceService, AttendanceService>();

        return services;
    }
}
