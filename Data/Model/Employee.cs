namespace Data.Model;

public class Employee
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
    public Department? Department { get; set; }
    public string? ImageUrl { get; set; }
    public string? UserId { get; set; }
    public EmployeeAddress? Address { get; set; }
}
