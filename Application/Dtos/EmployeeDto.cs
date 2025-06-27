using Data.Model;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public Guid DepartmentId { get; set; } = default!;
    public string DepartmentName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public AddressDto? Address { get; set; }
}

public class  EmployeesDto 
{
    public List<EmployeeDto> Employees { get; set; } = default!;
}

public class  CreateEmployeeDto 
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public Guid DepartmentId { get; set; } = default!;
    public IFormFile? Photo { get; set; }
}
public class  UpdateEmployeeDto 
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public Guid DepartmentId { get; set; } = default!;
    public IFormFile? Photo { get; set; }
    public string? ImageUrl { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

