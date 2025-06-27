namespace Application.Dtos
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }  // Links to the employee
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; } // This will work with your dropdown
    }

    public class CreateAddressDto
    {
        public Guid EmployeeId { get; set; }

        public string Street { get; set; } = default!;

        public string City { get; set; } = default!;
        public string Country { get; set; } = default!;

        public string State { get; set; } = default!;
    }

    public class UpdateAddressDto
    {
        public Guid Id { get; set; }    // ID of the Address
        public Guid EmployeeId { get; set; }   // Foreign key
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
