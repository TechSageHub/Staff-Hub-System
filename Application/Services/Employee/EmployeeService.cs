using System.Security.Claims;
using Application.ContractMapping;
using Application.Dtos;
using Application.Dtos.Paging;
using Application.Services.UploadImage;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Application.Services.Email;

namespace Application.Services.Employee;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeAppDbContext _context;
    private readonly IImageService _ImageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;

    public EmployeeService(EmployeeAppDbContext context, IImageService ImageService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager, IEmailService emailService)
    {
        _context = context;
        _ImageService = ImageService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _emailService = emailService;
    }

    private bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
    private string? CurrentUserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private static string GenerateTempPassword()
    {
        const string allowed = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var bytes = new byte[8];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        var suffix = new char[bytes.Length];

        for (int i = 0; i < bytes.Length; i++)
        {
            suffix[i] = allowed[bytes[i] % allowed.Length];
        }

        // Ensures: uppercase, lowercase, digit, non-alphanumeric, and min length.
        return $"Aa1!{new string(suffix)}";
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
            string? tempPassword = null;
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
                tempPassword = GenerateTempPassword();
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
            employee.UserId = user!.Id;

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
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request == null ? string.Empty : $"{request.Scheme}://{request.Host}";
                var loginLink = string.IsNullOrEmpty(baseUrl)
                    ? "#"
                    : $"{baseUrl}/Account/Login";

                var subject = "Welcome to StaffHub - Your Login Details";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                        <h2 style='color: #0d6efd;'>Welcome to the Team, {dto.FirstName}!</h2>
                        <p>Hi {dto.FirstName},</p>
                        <p>Your employee account has been created in the <strong>StaffHub</strong> portal.</p>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h4 style='margin-top: 0;'>Your Login Username:</h4>
                            <p style='margin-bottom: 0;'><strong>Email/Username:</strong> {dto.Email}</p>
                            <p style='margin: 8px 0 0 0;'><strong>Temporary Password:</strong> {tempPassword}</p>
                        </div>

                        <p>Please log in with the temporary password below and complete your profile.</p>
                        <a href='{loginLink}' style='display: inline-block; background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-top: 10px;'>Go to StaffHub</a>

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

    public async Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(EmployeeQuery q, string? userId = null)
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

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            // EF.Functions.Like is provider-agnostic and case-insensitive on most providers.
            var pattern = $"%{term}%";
            query = query.Where(e =>
                EF.Functions.Like(e.FirstName, pattern) ||
                EF.Functions.Like(e.LastName, pattern) ||
                EF.Functions.Like(e.Email, pattern) ||
                (e.Department != null && EF.Functions.Like(e.Department.Name, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(q.Department))
        {
            var dept = q.Department.Trim();
            query = query.Where(e => e.Department != null && e.Department.Name == dept);
        }

        if (!string.IsNullOrWhiteSpace(q.Onboarding))
        {
            if (string.Equals(q.Onboarding, "complete", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.IsOnboardingComplete);
            }
            else if (string.Equals(q.Onboarding, "incomplete", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => !e.IsOnboardingComplete);
            }
        }

        query = (q.Sort ?? "name_asc").ToLowerInvariant() switch
        {
            "name_desc"   => query.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName),
            "salary_asc"  => query.OrderBy(e => e.Salary),
            "salary_desc" => query.OrderByDescending(e => e.Salary),
            "hire_newest" => query.OrderByDescending(e => e.HireDate),
            "hire_oldest" => query.OrderBy(e => e.HireDate),
            _             => query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
        };

        var total = await query.CountAsync();
        var page = await query.Skip(q.Skip).Take(q.PageSize).ToListAsync();

        return new PagedResult<EmployeeDto>
        {
            Items = page.Select(e => e.ToDto()).ToList(),
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<IReadOnlyList<string>> GetDepartmentNamesAsync()
    {
        return await _context.Departments
            .Where(d => d.Name != null && d.Name != "Unassigned")
            .OrderBy(d => d.Name)
            .Select(d => d.Name!)
            .Distinct()
            .ToListAsync();
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId, string? userId = null)
    {
        var query = _context.Employees
            .Include(e => e.Address)
            .Include(e => e.Department)
            .Include(e => e.Qualifications)
            .Include(e => e.NextOfKin)
            .Include(e => e.HrInfo)
            .AsQueryable();
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
            DepartmentName = employee.Department?.Name ?? "Unassigned",
            Address = employee.Address == null ? null : new AddressDto
            {
                Id = employee.Address.Id,
                City = employee.Address.City,
                Country = employee.Address.Country,
                EmployeeId = employee.Id,
                Street = employee.Address.Street,
                State = employee.Address.State
            },
            Qualifications = employee.Qualifications
                .OrderByDescending(q => q.Year)
                .Select(q => new QualificationDto
                {
                    Title = q.Title,
                    Institution = q.Institution,
                    Year = q.Year,
                    Grade = q.Grade
                })
                .ToList(),
            NextOfKin = employee.NextOfKin == null
                ? null
                : new NextOfKinDto
                {
                    FullName = employee.NextOfKin.FullName,
                    Relationship = employee.NextOfKin.Relationship,
                    PhoneNumber = employee.NextOfKin.PhoneNumber,
                    Address = employee.NextOfKin.Address
                },
            HrInfo = employee.HrInfo == null
                ? null
                : new HrInfoDto
                {
                    DateOfBirth = employee.HrInfo.DateOfBirth,
                    MaritalStatus = employee.HrInfo.MaritalStatus,
                    NationalId = employee.HrInfo.NationalId,
                    BloodGroup = employee.HrInfo.BloodGroup,
                    Genotype = employee.HrInfo.Genotype
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
