using System.ComponentModel.DataAnnotations;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string? ImageUrl { get; set; }
        public AddressViewModel? Address { get; set; }
    }
    public class CreateEmployeeViewModel
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime HireDate { get; set; } = DateTime.Now;
    public decimal Salary { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; } = new();
        public IFormFile? Photo { get; set; }

        // Address fields
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; } = "Nigeria";
        public List<SelectListItem> States { get; set; } = new();
    }
   

    public class UpdateEmployeeViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; } = new();
        public IFormFile? Photo { get; set; }
        public string? ImageUrl { get; set; }

        // Address fields
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public List<SelectListItem> States { get; set; } = new();
    }

    public class EmployeesViewModel
    {
        public List<EmployeeViewModel> Employees { get; set; } = default!;
    }

    public class EmployeeDetailsViewModel
    {
        public EmployeeDto Employee { get; set; } = default!;
        public AddressDto? Address { get; set; }

        public List<SelectListItem>? States { get; set; }

        public bool HasAddress => Address != null;
    }
}
