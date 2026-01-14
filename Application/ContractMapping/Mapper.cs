//using Application.Dtos;
//using Data.Model;

//namespace Application.ContractMapping;
//public static class Mapper
//{
//    public static DepartmentDto ToDto(this Department department)
//    {
//        if (department == null) return null!;

//        return new DepartmentDto
//        {
//            Id = department.Id,
//            Name = department.Name,
//            Description = department.Description
//        };
//    }

//    public static EmployeeDto ToDto(this Employee employee)
//    {
//        if (employee == null) return null!;

//        return new EmployeeDto
//        {
//            Id = employee.Id,
//            Salary = employee.Salary,
//            HireDate = employee.HireDate,
//            Email = employee.Email,
//            FirstName = employee.FirstName,
//            LastName = employee.LastName,
//            DepartmentId = employee.DepartmentId,
//            ImageUrl = employee.ImageUrl,
//        };
//    }

//    public static AddressDto ToDto(this EmployeeAddress address)
//    {
//        if (address == null) return null!;
//        return new AddressDto
//        {
//            Id = address.Id,
//            EmployeeId = address.EmployeeId,
//            Street = address.Street,
//            City = address.City,
//            State = address.State
//        };
//    }

//    public static Department ToModel(this CreateDepartmentDto createDepartmentDto)
//    {
//        if (createDepartmentDto == null) return null!;
//        return new Department
//        {
//            Id = Guid.NewGuid(),
//            Name = createDepartmentDto.Name,
//            Description = createDepartmentDto.Description
//        };
//    }

//    public static Employee ToModel(this CreateEmployeeDto createEmployeeDto)
//    {
//        if (createEmployeeDto == null) return null!;
//        return new Employee
//        {
//            Id = Guid.NewGuid(),
//            Salary = createEmployeeDto.Salary,
//            HireDate = createEmployeeDto.HireDate,
//            DepartmentId = createEmployeeDto.DepartmentId,
//            Email = createEmployeeDto.Email,
//            FirstName = createEmployeeDto.FirstName,
//            LastName = createEmployeeDto.LastName,

//        };
//    }

//    public static EmployeeAddress ToModel(this CreateAddressDto createAddressDto)
//    {
//        if (createAddressDto == null) return null!;
//        return new EmployeeAddress
//        {
//            EmployeeId = createAddressDto.EmployeeId,
//            Street = createAddressDto.Street,
//            City = createAddressDto.City,
//            State = createAddressDto.State,
//            Country = createAddressDto.Country,
//        };
//    }


//    public static EmployeeAddress ToViewModel(this AddressDto dto)
//    {
//        if (dto == null) return null!;

//        return new EmployeeAddress
//        {
//            Id = dto.EmployeeId,
//            Street = dto.Street,
//            City = dto.City,
//            State = dto.State,
//            EmployeeId = dto.EmployeeId
//        };
//    }
//    public static DepartmentsDto DepartmentsDto(this List<Department> departments)
//    {
//        if (departments == null || !departments.Any()) return null!;
//        return new DepartmentsDto
//        {
//            Departments = departments.Select(d => d.ToDto()).ToList()
//        };
//    }
//    public static EmployeesDto EmployeesDto(this List<Employee> employees)
//    {
//        if (employees == null || !employees.Any()) return null!;
//        return new EmployeesDto
//        {
//            Employees = employees.Select(d => d.ToDto()).ToList()
//        };
//    }
//}


using Application.Dtos;
using Data.Model;

namespace Application.ContractMapping;

public static class Mapper
{
    public static DepartmentDto ToDto(this Department department)
    {
        if (department == null) return null!;
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            Employees = department.Employees?.Select(e => e.ToDto()).ToList() ?? []
        };
    }

    public static EmployeeDto ToDto(this Employee employee)
    {
        if (employee == null) return null!;
        return new EmployeeDto
        {
            Id = employee.Id,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            DepartmentId = employee.DepartmentId,
            ImageUrl = employee.ImageUrl,
            Address = employee.Address?.ToDto()
        };
    }

    public static AddressDto ToDto(this EmployeeAddress address)
    {
        if (address == null) return null!;
        return new AddressDto
        {
            Id = address.Id,
            EmployeeId = address.EmployeeId,
            Street = address.Street,
            City = address.City,
            State = address.State,
            Country = address.Country
        };
    }

    public static EmployeeAddress ToModel(this CreateAddressDto createAddressDto)
    {
        if (createAddressDto == null) return null!;
        return new EmployeeAddress
        {
            Id = Guid.NewGuid(),
            EmployeeId = createAddressDto.EmployeeId,
            Street = createAddressDto.Street,
            City = createAddressDto.City,
            State = createAddressDto.State,
            Country = createAddressDto.Country
        };
    }

    public static EmployeeAddress ToModel(this UpdateAddressDto dto)
    {
        if (dto == null) return null!;
        return new EmployeeAddress
        {
            Id = dto.Id,
            EmployeeId = dto.EmployeeId,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country
        };
    }

    public static EmployeeAddress ToModel(this AddressDto dto)
    {
        if (dto == null) return null!;
        return new EmployeeAddress
        {
            Id = dto.Id,
            EmployeeId = dto.EmployeeId,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country
        };
    }

    public static Department ToModel(this CreateDepartmentDto createDepartmentDto)
    {
        if (createDepartmentDto == null) return null!;
        return new Department
        {
            Id = Guid.NewGuid(),
            Name = createDepartmentDto.Name,
            Description = createDepartmentDto.Description
        };
    }

    public static Employee ToModel(this CreateEmployeeDto createEmployeeDto)
    {
        if (createEmployeeDto == null) return null!;
        return new Employee
        {
            Id = Guid.NewGuid(),
            Salary = createEmployeeDto.Salary,
            HireDate = createEmployeeDto.HireDate,
            DepartmentId = createEmployeeDto.DepartmentId,
            Email = createEmployeeDto.Email,
            FirstName = createEmployeeDto.FirstName,
            LastName = createEmployeeDto.LastName
        };
    }

    public static DepartmentsDto DepartmentsDto(this List<Department> departments)
    {
        return new DepartmentsDto
        {
            Departments = departments?.Select(d => d.ToDto()).ToList() ?? new List<DepartmentDto>()
        };
    }

    public static EmployeesDto EmployeesDto(this List<Employee> employees)
    {
        return new EmployeesDto
        {
            Employees = employees?.Select(d => d.ToDto()).ToList() ?? new List<EmployeeDto>()
        };
    }

    public static LeaveRequestDto ToDto(this LeaveRequest leave)
    {
        if (leave == null) return null!;
        return new LeaveRequestDto
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            EmployeeName = leave.Employee != null ? $"{leave.Employee.FirstName} {leave.Employee.LastName}" : "Unknown",
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            LeaveType = leave.LeaveType,
            Reason = leave.Reason,
            Status = leave.Status.ToString(),
            DateRequested = leave.DateRequested,
            AdminComment = leave.AdminComment
        };
    }

    public static LeaveRequest ToModel(this CreateLeaveRequestDto dto)
    {
        if (dto == null) return null!;
        return new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            LeaveType = dto.LeaveType,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            DateRequested = DateTime.Now
        };
    }
}
