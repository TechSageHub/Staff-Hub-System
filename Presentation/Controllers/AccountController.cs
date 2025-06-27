using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Presentation.Models;
using System.Text;
using Application.Dtos;
using Application.Services.Authentication;

namespace Presentation.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthenticationService _authService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly INotyfService _notyf;

        public AccountController(
            IAuthenticationService authService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            INotyfService notyf)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
        }
        [HttpGet]
        public IActionResult ConfirmPrompt(string email)
        {
            ViewBag.Email = email;
            return View("ConfirmEmailPrompt");
        }



        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Invalid input.");
                return View(model);
            }

            var result = await _authService.SignUp(new CreateUserDto(model.Email, model.Email, model.Password));

            if (result != null)
            {
                _notyf.Success("Account created! Please check your email to confirm.");
                return RedirectToAction("Login", "Account");
            }

            _notyf.Error("Account creation failed. Email may already be taken.");
            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Invalid form submission.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _notyf.Error("User not found.");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _notyf.Error("Please confirm your email before logging in.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

           
            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remaining = lockoutEnd?.LocalDateTime - DateTime.Now;
                _notyf.Error($"Your account is locked. Try again in {remaining?.Minutes} minutes.");
                return View(model);
            }

           
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

           
            var accessFailedCount = await _userManager.GetAccessFailedCountAsync(user);
            var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;

            var attemptsLeft = maxAttempts - accessFailedCount;

            _notyf.Warning($"Invalid login credentials. You have {attemptsLeft} more attempt(s) before your account is locked.");
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                _notyf.Error("Invalid confirmation link.");
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _notyf.Error("User not found.");
                return RedirectToAction("Login");
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                _notyf.Success("Email confirmed successfully. You can now log in.");
            }
            else
            {
                _notyf.Error("Email confirmation failed.");
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Please enter a valid email.");
                return View(model);
            }

            var result = await _authService.ForgotPasswordAsync(model.Email);
            if (result)
            {
                _notyf.Success("A reset link has been sent to your email.");
                return RedirectToAction("Login");
            }

            _notyf.Error("Password reset failed. Email may not exist or be confirmed.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Please fix the form errors.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                _notyf.Error("Passwords do not match.");
                return View(model);
            }

            var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            if (result == true)
            {
                _notyf.Success("Password reset successful. You can now log in.");
                return RedirectToAction("Login");
            }

            _notyf.Error("Password reset failed.");
            return View(model);
        }

    }
}
