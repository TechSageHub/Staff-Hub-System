using System.ComponentModel.DataAnnotations;

namespace Presentation.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
