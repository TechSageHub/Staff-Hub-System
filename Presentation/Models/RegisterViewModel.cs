using System.ComponentModel.DataAnnotations;

namespace Presentation.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password do not match")]
        public required string ConfirmPassword { get; set; }
    }
}