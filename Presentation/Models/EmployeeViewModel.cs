using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models
{
    public class CreateEmployeeViewModel
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public Guid DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; }
    } 
    public class EmployeeViewModel
    {
        public Guid Id { get; set; } 
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public Guid DepartmentId { get; set; }  
        public string DepartmentName { get; set; } 
    }
    
    public class UpdateEmployeeViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public Guid DepartmentId { get; set; }
        public List<SelectListItem> Departments { get; set; }
    }

    public class EmployeesViewModel
    {
        public List<EmployeeViewModel> Employees { get; set; } = default!; 
    }

}
