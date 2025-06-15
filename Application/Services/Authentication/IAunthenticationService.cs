using Application.Dtos;

namespace Application.Services.Authentication
{
    public interface IAunthenticationService
    {
        Task<UserDto> SignUp(UserDto createUserDto);
        Task<UserDto> SignIn(string credential, string Password);
        Task<bool> changePassword(string? email, string? username, string password);
    }
}
