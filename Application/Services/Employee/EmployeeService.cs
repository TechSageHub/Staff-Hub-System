using Application.ContractMapping;
using Application.Dtos;
using Application.Services.UploadImage;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Employee;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeAppDbContext _context;
    private readonly Cloudinary _cloudinary;
    private readonly IImageService _ImageService;

    public EmployeeService(EmployeeAppDbContext context, Cloudinary cloudinary, IImageService ImageService)
    {
        _context = context;
        _cloudinary = cloudinary;
        _ImageService = ImageService;
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

            var employee = dto.ToModel();

            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                var imageUrl = await _ImageService.UploadImageAsync(dto.Photo);
                employee.ImageUrl = imageUrl;
            }

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

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
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException("Employee not found.");
        }

        if (employee.Address != null)
            _context.Addresses.Remove(employee.Address); // explicitly remove the address

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


    public async Task<EmployeesDto> GetAllEmployeesAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();

        return employees.EmployeesDto();
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Address)
            .FirstOrDefaultAsync(d => d.Id == employeeId);

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

    //public async Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto employeeDto)
    //{
    //    var employee = await _context.Employees
    //        .Include(e => e.Address)
    //        .FirstOrDefaultAsync(e => e.Id == employeeDto.Id);

    //    if (employee == null)
    //    {
    //        return null;
    //    }

    //    employee.FirstName = employeeDto.FirstName;
    //    employee.LastName = employeeDto.LastName;
    //    employee.Salary = employeeDto.Salary;
    //    employee.Email = employeeDto.Email;
    //    employee.HireDate = employeeDto.HireDate;
    //    employee.DepartmentId = employeeDto.DepartmentId;

    //    // Add or update address
    //    if (!string.IsNullOrWhiteSpace(employeeDto.Street) ||
    //        !string.IsNullOrWhiteSpace(employeeDto.City) ||
    //        !string.IsNullOrWhiteSpace(employeeDto.State))
    //    {
    //        if (employee.Address == null)
    //        {
    //            employee.Address = new EmployeeAddress
    //            {
    //                Street = employeeDto.Street,
    //                City = employeeDto.City,
    //                State = employeeDto.State,
    //                EmployeeId = employee.Id
    //            };
    //        }
    //        else
    //        {
    //            employee.Address.Street = employeeDto.Street;
    //            employee.Address.City = employeeDto.City;
    //            employee.Address.State = employeeDto.State;
    //        }
    //    }

    //    // Upload image if provided
    //    if (employeeDto.Photo != null && employeeDto.Photo.Length > 0)
    //    {
    //        // Delete old image if exists
    //        if (!string.IsNullOrEmpty(employee.ImageUrl))
    //        {
    //            await _ImageService.DeleteImageAsync(employee.ImageUrl);
    //        }

    //        var imageUrl = await _ImageService.UploadImageAsync(employeeDto.Photo);
    //        if (!string.IsNullOrEmpty(imageUrl))
    //        {
    //            employee.ImageUrl = imageUrl;
    //        }
    //    }

    //    await _context.SaveChangesAsync();

    //    return new EmployeeDto
    //    {
    //        Id = employee.Id,
    //        Email = employee.Email,
    //        FirstName = employee.FirstName,
    //        LastName = employee.LastName,
    //        DepartmentId = employee.DepartmentId,
    //        HireDate = employee.HireDate,
    //        Salary = employee.Salary,
    //        ImageUrl = employee.ImageUrl
    //    };
    //}
    public async Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto employeeDto)
    {
        var employee = await _context.Employees
            .Include(e => e.Address)
            .FirstOrDefaultAsync(e => e.Id == employeeDto.Id);

        if (employee == null)
            return null;

        employee.FirstName = employeeDto.FirstName;
        employee.LastName = employeeDto.LastName;
        employee.Salary = employeeDto.Salary;
        employee.Email = employeeDto.Email;
        employee.HireDate = employeeDto.HireDate;
        employee.DepartmentId = employeeDto.DepartmentId;

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
                    EmployeeId = employee.Id
                };
            }
            else
            {
                employee.Address.Street = employeeDto.Street;
                employee.Address.City = employeeDto.City;
                employee.Address.State = employeeDto.State;
            }
        }

        // Upload image if provided
        if (employeeDto.Photo != null && employeeDto.Photo.Length > 0)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(employee.ImageUrl))
            {
                await _ImageService.DeleteImageAsync(employee.ImageUrl);
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
            ImageUrl = employee.ImageUrl
        };
    }


    private string ExtractPublicIdFromUrl(string imageUrl)
    {


        if (string.IsNullOrEmpty(imageUrl))
            return null;

        var uri = new Uri(imageUrl);
        var segments = uri.AbsolutePath.Split('/');


        var uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
            return null;


        var publicIdWithExtension = string.Join("/", segments.Skip(uploadIndex + 2));


        var publicId = Path.Combine(Path.GetDirectoryName(publicIdWithExtension) ?? string.Empty,
                                    Path.GetFileNameWithoutExtension(publicIdWithExtension))
                       .Replace("\\", "/");

        return publicId;
    }


    public async Task DeleteImageAsync(Guid employeeId)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee != null && !string.IsNullOrEmpty(employee.ImageUrl))
        {
            string publicId = ExtractPublicIdFromUrl(employee.ImageUrl);

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

}
