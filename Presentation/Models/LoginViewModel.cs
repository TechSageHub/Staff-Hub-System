using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Presentation.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
        public bool RememberMe { get; set; }

    }
}