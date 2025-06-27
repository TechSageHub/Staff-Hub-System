namespace Data.Model
{
    public class EmployeeAddress
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public Guid EmployeeId { get; set; } // foreign key
        public Employee Employee { get; set; } // navigation property
    }
}
