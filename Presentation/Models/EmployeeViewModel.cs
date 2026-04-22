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
        public bool IsOnboardingComplete { get; set; }
        public AddressViewModel? Address { get; set; }
        public List<QualificationViewModel> Qualifications { get; set; } = new();
        public NextOfKinViewModel? NextOfKin { get; set; }
        public HrInfoViewModel? HrInfo { get; set; }
        public List<EmployeeDocumentDto> Documents { get; set; } = new();
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
        public List<EmployeeViewModel> Employees { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public string? Search { get; set; }
        public string? Department { get; set; }
        public string? Onboarding { get; set; }
        public string Sort { get; set; } = "name_asc";
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(FilteredCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public int FirstItemNumber => FilteredCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
        public int LastItemNumber => Math.Min(Page * PageSize, FilteredCount);
    }

    public class EmployeeDetailsViewModel
    {
        public EmployeeDto Employee { get; set; } = default!;
        public AddressDto? Address { get; set; }

        public List<SelectListItem>? States { get; set; }

        public bool HasAddress => Address != null;
    }
}
