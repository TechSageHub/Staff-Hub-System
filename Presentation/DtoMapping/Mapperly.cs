using Application.Dtos;
using Presentation.Models;

namespace Presentation.DtoMapping;

public static class Mapperly
{
    // EmployeeViewModel <-> EmployeeDto
    public static EmployeeViewModel ToViewModel(this EmployeeDto dto)
    {
        return new EmployeeViewModel()
        {
            Id = dto.Id,
            Salary = dto.Salary,
            Email = dto.Email,
            HireDate = dto.HireDate,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DepartmentName = dto.DepartmentName,
        };
    }
    public static EmployeeDto ToDto(this EmployeeViewModel vm)
    {
        return new EmployeeDto()
        {
            Id = vm.Id,
            LastName = vm.LastName,
            FirstName = vm.FirstName,
            Email = vm.Email,
            HireDate = vm.HireDate,
            Salary = vm.Salary
        };
    }
    public static EmployeesViewModel ToViewModel(this EmployeesDto dto)
    {
        if (dto == null)
            return new EmployeesViewModel { Employees = new List<EmployeeViewModel>() };

        return new EmployeesViewModel()
        {
            Employees = dto.Employees.Select(d => d.ToViewModel()).ToList()
        };
    }

    public static EmployeesDto ToDto(this EmployeesViewModel vm)
    {
        return new EmployeesDto()
        {
            Employees = vm.Employees.Select(d => d.ToDto()).ToList()
        };
    }
    // DepartmentViewModel <-> DepartmentDto
    public static DepartmentViewModel ToViewModel(this DepartmentDto dto)
    {
        return new DepartmentViewModel()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }  
    public static DepartmentDto ToDto(this DepartmentViewModel vm)
    {
        return new DepartmentDto()
        {
            Id = vm.Id,
            Name = vm.Name,
            Description = vm.Description,
            Employees = []
        };
    }  
  
    public static DepartmentsViewModel ToViewModel(this DepartmentsDto dto)
    {
        if (dto == null)
            return new DepartmentsViewModel { Departments = new List<DepartmentViewModel>() };

        return new DepartmentsViewModel()
        {
            Departments = dto.Departments.Select(d => d.ToViewModel()).ToList()
        };
    }

  

    public static DepartmentsDto ToDto(this DepartmentsViewModel vm)
    {
        return new DepartmentsDto()
        {
            Departments = vm.Departments.Select(d => d.ToDto()).ToList()
        };
    }

   
}
