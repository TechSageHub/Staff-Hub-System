using Application.Dtos;

namespace Application.Services.Employee
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task DeleteEmployeeAsync(Guid employeeId);
        Task<EmployeesDto> GetAllEmployeesAsync();
        Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId);
        Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto employeeDto);
    }
}
