using System.ComponentModel.DataAnnotations;
namespace Presentation.Models
{

    public class CreateAddressViewModel
    {
        public Guid EmployeeId { get; set; } // Link to the employee


        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Country { get; set; }
    }

    
    public class UpdateAddressViewModel
        {
            public Guid Id { get; set; } // Address Id
            public Guid EmployeeId { get; set; }

            [Required]
            public string Street { get; set; } = default!;

            [Required]
            public string City { get; set; } = default!;

            [Required]
            public string State { get; set; } = default!;

            [Required]
            public string Country { get; set; } = default!;
        }


    public class AddressViewModel
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string Street { get; set; } = default!;
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string Country { get; set; } = default!;
    }
}


