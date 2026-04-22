using Application.Dtos;
using Application.Dtos.Paging;
namespace Application.Services.Employee
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task DeleteEmployeeAsync(Guid employeeId);
        Task<EmployeesDto> GetAllEmployeesAsync(string? userId = null);
        Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(EmployeeQuery query, string? userId = null);
        Task<IReadOnlyList<string>> GetDepartmentNamesAsync();
        Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId, string? userId = null);
        Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto employeeDto);
        Task DeleteImageAsync(Guid employeeId);
        Task<EmployeeDto?> GetEmployeeByUserIdAsync(string userId);
    }
}
