using Microsoft.AspNetCore.Identity;

namespace Data.Model
{
    public class User : IdentityUser
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string Password { get; set; }
    };
}
