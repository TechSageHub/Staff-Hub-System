using System.Security.Claims;
using Application.ContractMapping;
using Application.Dtos;
using Application.Services.UploadImage;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Application.Services.Email;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Application.Services.Employee;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeAppDbContext _context;
    private readonly Cloudinary _cloudinary;
    private readonly IImageService _ImageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;

    public EmployeeService(EmployeeAppDbContext context, Cloudinary cloudinary, IImageService ImageService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager, IEmailService emailService)
    {
        _context = context;
        _cloudinary = cloudinary;
        _ImageService = ImageService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _emailService = emailService;
    }

    private bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
    private string? CurrentUserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private static string GenerateTempPassword()
    {
        // Ensure password meets typical complexity requirements.
        return $"Aa1!{Guid.NewGuid():N}".Substring(0, 16);
    }

    public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        try
        {
            var emailExists = _context.Employees.Any(e => e.Email == dto.Email);
            if (emailExists)
            {
                return null;
            }

            IdentityUser? user = null;
            var isExistingIdentityUser = !string.IsNullOrWhiteSpace(dto.UserId);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (isExistingIdentityUser)
            {
                user = await _userManager.FindByIdAsync(dto.UserId!);
                if (user == null)
                {
                    return null;
                }

                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!addRoleResult.Succeeded)
                    {
                        return null;
                    }
                }

                dto.Email = user.Email ?? dto.Email;
            }
            else
            {
                var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUserByEmail != null)
                {
                    return null;
                }

                // Create Identity User for the employee
                var tempPassword = GenerateTempPassword();
                user = new IdentityUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createUserResult.Succeeded)
                {
                    return null;
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return null;
                }
            }

            var employee = dto.ToModel();
            employee.UserId = user.Id;

            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                var imageUrl = await _ImageService.UploadImageAsync(dto.Photo);
                employee.ImageUrl = imageUrl;
            }

            // Add address if provided
            if (!string.IsNullOrWhiteSpace(dto.Street) ||
                !string.IsNullOrWhiteSpace(dto.City) ||
                !string.IsNullOrWhiteSpace(dto.State))
            {
                employee.Address = new EmployeeAddress
                {
                    Street = dto.Street,
                    City = dto.City,
                    State = dto.State,
                    Country = dto.Country ?? "Nigeria",
                    EmployeeId = employee.Id
                };
            }

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (!isExistingIdentityUser)
            {
                // Send Onboarding Email with password reset link
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request == null ? string.Empty : $"{request.Scheme}://{request.Host}";
                var resetLink = string.IsNullOrEmpty(baseUrl)
                    ? "#"
                    : $"{baseUrl}/Account/ResetPassword?email={dto.Email}&token={encodedToken}";

                var subject = "Welcome to StaffHub - Set Your Password";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                        <h2 style='color: #0d6efd;'>Welcome to the Team, {dto.FirstName}!</h2>
                        <p>Hi {dto.FirstName},</p>
                        <p>Your employee account has been created in the <strong>StaffHub</strong> portal.</p>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0;'>Your Login Username:</h4>
                            <p style='margin-bottom: 0;'><strong>Email/Username:</strong> {dto.Email}</p>
                        </div>

                        <p>For security, please set your password using the link below:</p>
                        <a href='{resetLink}' style='display: inline-block; background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-top: 10px;'>Set Password</a>

                        <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='font-size: 12px; color: #777;'>Sent from StaffHub Employee Management System.</p>
                    </div>";

                await _emailService.SendEmailAsync(dto.Email, subject, body);
            }

            return employee.ToDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while creating the employee: " + ex.Message);
            return null;
        }
    }

    public async Task DeleteEmployeeAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Address)
            .FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        if (!IsAdmin && !string.Equals(employee.UserId, CurrentUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this employee.");
        }

        if (!string.IsNullOrEmpty(employee.ImageUrl))
        {
            var publicId = _ImageService.ExtractPublicIdFromUrl(employee.ImageUrl);
            if (!string.IsNullOrEmpty(publicId))
            {
                await _ImageService.DeleteImageAsync(publicId);
            }
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Address)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee?.Address != null)
        {
            _context.Addresses.Remove(employee.Address);
            await _context.SaveChangesAsync();
        }
    }


    public async Task<EmployeesDto> GetAllEmployeesAsync(string? userId = null)
    {
        var query = _context.Employees.Include(e => e.Department).AsQueryable();
        var user = _httpContextAccessor.HttpContext?.User;
        var isAdmin = user?.IsInRole("Admin") ?? false;

        if (!isAdmin && !string.IsNullOrEmpty(userId))
        {
            query = query.Where(e => e.UserId == userId);
        }
        else if (isAdmin)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = admins.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
            query = query.Where(e => e.UserId == null || !adminIds.Contains(e.UserId));
        }

        var employees = await query.ToListAsync();

        return employees.EmployeesDto();
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId, string? userId = null)
    {
        var query = _context.Employees.Include(e => e.Address).AsQueryable();
        var user = _httpContextAccessor.HttpContext?.User;
        var isAdmin = user?.IsInRole("Admin") ?? false;

        if (!isAdmin && !string.IsNullOrEmpty(userId))
        {
            query = query.Where(e => e.UserId == userId);
        }

        var employee = await query.FirstOrDefaultAsync(d => d.Id == employeeId);

        if (employee == null)
            return null;

        return new EmployeeDto
        {
            Id = employee.Id,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            DepartmentId = employee.DepartmentId,
            LastName = employee.LastName,
            FirstName = employee.FirstName,
            Email = employee.Email,
            Gender = employee.Gender,
            PhoneNumber = employee.PhoneNumber,
            ImageUrl = employee.ImageUrl,
            Address = employee.Address == null ? null : new AddressDto
            {
                Id = employee.Address.Id,
                City = employee.Address.City,
                Country = employee.Address.Country,
                EmployeeId = employee.Id,
                Street = employee.Address.Street,
                State = employee.Address.State
            }
        };
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto employeeDto)
    {
        var employee = await _context.Employees
            .Include(e => e.Address)
            .FirstOrDefaultAsync(e => e.Id == employeeDto.Id);

        if (employee == null)
            return null;

        if (!IsAdmin && !string.Equals(employee.UserId, CurrentUserId, StringComparison.Ordinal))
        {
            return null;
        }

        employee.FirstName = employeeDto.FirstName;
        employee.LastName = employeeDto.LastName;
        employee.Salary = employeeDto.Salary;
        employee.Email = employeeDto.Email;
        employee.HireDate = employeeDto.HireDate;
        employee.DepartmentId = employeeDto.DepartmentId;
        employee.Gender = employeeDto.Gender;
        employee.PhoneNumber = employeeDto.PhoneNumber;

        // Add or update address
        if (!string.IsNullOrWhiteSpace(employeeDto.Street) ||
            !string.IsNullOrWhiteSpace(employeeDto.City) ||
            !string.IsNullOrWhiteSpace(employeeDto.State))
        {
            if (employee.Address == null)
            {
                employee.Address = new EmployeeAddress
                {
                    Street = employeeDto.Street,
                    City = employeeDto.City,
                    State = employeeDto.State,
                    Country = "Nigeria", // Default for now
                    EmployeeId = employee.Id
                };
            }
            else
            {
                employee.Address.Street = employeeDto.Street;
                employee.Address.City = employeeDto.City;
                employee.Address.State = employeeDto.State;
                if (!string.IsNullOrEmpty(employeeDto.Country))
                {
                    employee.Address.Country = employeeDto.Country;
                }
            }
        }

        // Upload image if provided
        if (employeeDto.Photo != null && employeeDto.Photo.Length > 0)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(employee.ImageUrl))
            {
                var publicId = _ImageService.ExtractPublicIdFromUrl(employee.ImageUrl);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _ImageService.DeleteImageAsync(publicId);
                }
            }

            var imageUrl = await _ImageService.UploadImageAsync(employeeDto.Photo);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                employee.ImageUrl = imageUrl;
            }
        }
        else if (string.IsNullOrEmpty(employee.ImageUrl))
        {
            // If no photo uploaded and no existing image
            employee.ImageUrl = null;
        }

        await _context.SaveChangesAsync();

        return new EmployeeDto
        {
            Id = employee.Id,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            DepartmentId = employee.DepartmentId,
            HireDate = employee.HireDate,
            Salary = employee.Salary,
            Gender = employee.Gender,
            PhoneNumber = employee.PhoneNumber,
            ImageUrl = employee.ImageUrl
        };
    }

    public async Task DeleteImageAsync(Guid employeeId)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee != null && !string.IsNullOrEmpty(employee.ImageUrl))
        {
            if (!IsAdmin && !string.Equals(employee.UserId, CurrentUserId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this image.");
            }

            string publicId = _ImageService.ExtractPublicIdFromUrl(employee.ImageUrl);

            if (!string.IsNullOrEmpty(publicId))
            {
                var result = await _ImageService.DeleteImageAsync(publicId);

                if (result)
                {
                    employee.ImageUrl = null;
                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Failed to delete image from Cloudinary.");
                }
            }
        }
    }

    public async Task<EmployeeDto?> GetEmployeeByUserIdAsync(string userId)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == userId);
            
        return employee?.ToDto();
    }
}
