using Application.Dtos;

namespace Application.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<UserDto> SignUp(CreateUserDto createUserDto);
        Task<UserDto> SignIn(string credential, string password);
        Task<bool> ChangePassword(string email, string username, string password);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }

}
