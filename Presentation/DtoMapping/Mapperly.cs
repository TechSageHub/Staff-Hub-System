//using Application.Dtos;
//using Presentation.Models;

//namespace Presentation.DtoMapping;

//public static class Mapperly
//{
//    // EmployeeViewModel <-> EmployeeDto
//    public static EmployeeViewModel ToViewModel(this EmployeeDto dto)
//    {
//        return new EmployeeViewModel()
//        {
//            Id = dto.Id,
//            Salary = dto.Salary,
//            Email = dto.Email,
//            HireDate = dto.HireDate,
//            FirstName = dto.FirstName,
//            LastName = dto.LastName,
//            DepartmentName = dto.DepartmentName,
//            ImageUrl = dto.ImageUrl,
//        };
//    }
//    public static EmployeeDto ToDto(this EmployeeViewModel vm)
//    {
//        return new EmployeeDto()
//        {
//            Id = vm.Id,
//            LastName = vm.LastName,
//            FirstName = vm.FirstName,
//            Email = vm.Email,
//            HireDate = vm.HireDate,
//            Salary = vm.Salary,
//            ImageUrl = vm.ImageUrl
//        };
//    }
//    public static EmployeesViewModel ToViewModel(this EmployeesDto dto)
//    {
//        if (dto == null)
//            return new EmployeesViewModel { Employees = new List<EmployeeViewModel>() };

//        return new EmployeesViewModel()
//        {
//            Employees = dto.Employees.Select(d => d.ToViewModel()).ToList()
//        };
//    }

//    public static EmployeesDto ToDto(this EmployeesViewModel vm)
//    {
//        return new EmployeesDto()
//        {
//            Employees = vm.Employees.Select(d => d.ToDto()).ToList()
//        };
//    }
//    // DepartmentViewModel <-> DepartmentDto
//    public static DepartmentViewModel ToViewModel(this DepartmentDto dto)
//    {
//        return new DepartmentViewModel()
//        {
//            Id = dto.Id,
//            Name = dto.Name,
//            Description = dto.Description
//        };
//    }
//    public static DepartmentDto ToDto(this DepartmentViewModel vm)
//    {
//        return new DepartmentDto()
//        {
//            Id = vm.Id,
//            Name = vm.Name,
//            Description = vm.Description,
//            Employees = []
//        };
//    }

//    public static DepartmentsViewModel ToViewModel(this DepartmentsDto dto)
//    {
//        if (dto == null)
//            return new DepartmentsViewModel { Departments = new List<DepartmentViewModel>() };

//        return new DepartmentsViewModel()
//        {
//            Departments = dto.Departments.Select(d => d.ToViewModel()).ToList()
//        };
//    }



//    public static DepartmentsDto ToDto(this DepartmentsViewModel vm)
//    {
//        return new DepartmentsDto()
//        {
//            Departments = vm.Departments.Select(d => d.ToDto()).ToList()
//        };
//    }

//    public static AddressDto ToDto(this AddressViewModel model)
//    {
//        if (model == null) return null!;

//        return new AddressDto
//        {
//            Id = model.Id,
//            Street = model.Street,
//            City = model.City,
//            State = model.State,
//            EmployeeId = model.EmployeeId
//        };
//    }

//    public static UpdateAddressViewModel ToUpdateViewModel(this AddressDto dto)
//    {
//        if (dto == null) return null!;
//        return new UpdateAddressViewModel
//        {
//            Id = dto.Id,
//            Street = dto.Street,
//            City = dto.City,
//            State = dto.State,
//            Country = dto.Country,
//            EmployeeId = dto.EmployeeId
//        };
//    }


//    public static AddressViewModel ToViewModel(this AddressDto dto)
//    {
//        if (dto == null) return null!;
//        return new AddressViewModel
//        {
//            Id = dto.Id,
//            Street = dto.Street,
//            City = dto.City,
//            State = dto.State,
//            Country = dto.Country,
//            EmployeeId = dto.EmployeeId
//        };
//    }

//    public static UpdateAddressViewModel ToUpdateAddressViewModel(this AddressDto dto)
//    {
//        if (dto == null) return null!;

//        return new UpdateAddressViewModel
//        {
//            Id = dto.Id,
//            EmployeeId = dto.EmployeeId,
//            Street = dto.Street,
//            City = dto.City,
//            State = dto.State,
//            Country = dto.Country
//        };
//    }


//    public static UpdateAddressDto ToDto(this UpdateAddressViewModel model)
//    {
//        if (model == null) return null!;

//        return new UpdateAddressDto
//        {
//            Id = model.Id,
//            EmployeeId = model.EmployeeId,
//            Street = model.Street,
//            City = model.City,
//            State = model.State,
//            Country = model.Country
//        };
//    }

//}


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
            ImageUrl = dto.ImageUrl,
            Address = dto.Address?.ToViewModel()
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
            Salary = vm.Salary,
            ImageUrl = vm.ImageUrl,
            Address = vm.Address?.ToDto()
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

    // Address Mapping
    public static AddressDto ToDto(this AddressViewModel model)
    {
        if (model == null) return null!;

        return new AddressDto
        {
            Id = model.Id,
            Street = model.Street,
            City = model.City,
            State = model.State,
            Country = model.Country,
            EmployeeId = model.EmployeeId
        };
    }

    public static AddressViewModel ToViewModel(this AddressDto dto)
    {
        if (dto == null) return null!;

        return new AddressViewModel
        {
            Id = dto.Id,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            EmployeeId = dto.EmployeeId
        };
    }

    public static UpdateAddressViewModel ToUpdateViewModel(this AddressDto dto)
    {
        if (dto == null) return null!;

        return new UpdateAddressViewModel
        {
            Id = dto.Id,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            EmployeeId = dto.EmployeeId
        };
    }

    public static UpdateAddressViewModel ToUpdateAddressViewModel(this AddressDto dto)
    {
        if (dto == null) return null!;

        return new UpdateAddressViewModel
        {
            Id = dto.Id,
            EmployeeId = dto.EmployeeId,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country
        };
    }

    public static UpdateAddressDto ToDto(this UpdateAddressViewModel model)
    {
        if (model == null) return null!;

        return new UpdateAddressDto
        {
            Id = model.Id,
            EmployeeId = model.EmployeeId,
            Street = model.Street,
            City = model.City,
            State = model.State,
            Country = model.Country
        };
    }

    public static CreateAddressDto ToDto(this CreateAddressViewModel model)
    {
        if (model == null) return null!;

        return new CreateAddressDto
        {
            EmployeeId = model.EmployeeId,
            Street = model.Street,
            City = model.City,
            State = model.State,
            Country = model.Country
        };
    }
}
