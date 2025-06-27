using Application.Dtos;
using Application.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Application.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ Sign Up
        public async Task<UserDto> SignUp(CreateUserDto createUserDto)
        {
            var user = new IdentityUser
            {
                UserName = createUserDto.UserName,
                Email = createUserDto.Email
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
                return null;

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var confirmationLink = $"{baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(user.Email, "Confirm Your Email", $"Click <a href='{confirmationLink}'>here</a> to confirm your email.");

            return new UserDto(user.Id, user.Email, user.UserName);
        }

        // ✅ Sign In
        public async Task<UserDto> SignIn(string credential, string password)
        {
            var user = await _userManager.FindByEmailAsync(credential);
            if (user == null)
                return null;

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return null;

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: false, lockoutOnFailure: true);
            if (!result.Succeeded)
                return null;

            return new UserDto(user.Id, user.Email, user.UserName);
        }

        // ✅ Change Password
        public async Task<bool> ChangePassword(string email, string username, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.UserName != username)
                return false;

            // You may want to require old password too, if so use: ChangePasswordAsync(user, old, new)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            return result.Succeeded;
        }
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var resetLink = $"{baseUrl}/Account/ResetPassword?email={email}&token={encodedToken}";

            await _emailService.SendEmailAsync(email, "Reset Password", $"Click <a href='{resetLink}'>here</a> to reset your password.");
            return true;
        }
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            return result.Succeeded;
        }
    }
}
