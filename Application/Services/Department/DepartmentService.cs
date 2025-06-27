using Application.ContractMapping;
using Application.Dtos;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Department;

public class DepartmentService : IDepartmentService
{
    private readonly EmployeeAppDbContext _context;

    public DepartmentService(EmployeeAppDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto)
    {
        var data = new CreateDepartmentDto
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        var department = data.ToModel();

        try
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();

            return department.ToDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while creating the department.", ex);
            return new DepartmentDto();
        }
    }

    public async Task DeleteDepartmentAsync(Guid departmentId)
    {
        var department = await _context.Departments.FindAsync(departmentId);

        if (department == null)
        {
            throw new KeyNotFoundException("Department not found.");
        }
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
    }

    public async Task<DepartmentsDto> GetAllDepartmentsAsync()
    {
        var departments = await _context.Departments.ToListAsync();

        return departments.DepartmentsDto();
    }

    public async Task<DepartmentDto> GetDepartmentByIdAsync(Guid departmentId)
    {
        var department = await _context.Departments
       .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
            return null;


        var departmentDto = new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };

        return departmentDto;
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(UpdateDepartmentDto departmentDto) 
    {
        var department = await _context.Departments.FindAsync(departmentDto.Id);

        if (department == null)
        { 
            return null;
        }
        
        department.Name = departmentDto.Name;
        department.Description = departmentDto.Description;

        await _context.SaveChangesAsync();

        var updatedDto = new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };

        return updatedDto;
    }


}
