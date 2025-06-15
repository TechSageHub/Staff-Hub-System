namespace Application.Dtos
{
    public record UserDto
    
        (string Id,
         string Email,
         string UserName
        );
    

    public record CreateUserDto
        (string Email,
         string UserName,
         string Password
        );

    public record ChangePasswordDto(
        string Id,
        string OldPassword,
        string NewPassword
        );
}
