using Application.Services.Authentication;
using Application.Services.Department;
using Application.Services.Email;
using Application.Services.Employee;
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

        return services;
    }
}
