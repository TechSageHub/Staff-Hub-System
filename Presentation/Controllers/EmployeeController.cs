using Application.Dtos;
using Application.Services.Department;
using Application.Services.Employee;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.DtoMapping;
using Presentation.Models;

namespace Presentation.Controllers;

[Authorize]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly INotyfService _notyf;

    public EmployeeController(IEmployeeService employeeService, INotyfService notyf, IDepartmentService departmentService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _notyf = notyf;
    }

    private async Task PopulateDepartments(CreateEmployeeViewModel model)
    {
        var departmentsDto = await _departmentService.GetAllDepartmentsAsync();
        model.Departments = departmentsDto.Departments.Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = d.Name
        }).ToList();
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        var viewModel = employees.ToViewModel();
        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var employeeDto = await _employeeService.GetEmployeeByIdAsync(id);
        if (employeeDto == null)
            return NotFound();

        var viewModel = new EmployeeViewModel
        {
            Id = employeeDto.Id,
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
            HireDate = employeeDto.HireDate,
            Salary = employeeDto.Salary,
            ImageUrl = employeeDto.ImageUrl,
            DepartmentName = employeeDto.DepartmentName,
            Address = employeeDto.Address == null ? null : new AddressViewModel
            {
                Id = employeeDto.Address.Id,
                State = employeeDto.Address.State,
                City = employeeDto.Address.City,
                Country = employeeDto.Address.Country,
                EmployeeId = employeeDto.Address.EmployeeId,
                Street = employeeDto.Address.Street
            }
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var departmentsDto = await _departmentService.GetAllDepartmentsAsync();

        var model = new CreateEmployeeViewModel
        {
            Departments = departmentsDto.Departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel model)
    {
        var dto = new CreateEmployeeDto
        {
            DepartmentId = model.DepartmentId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Salary = model.Salary,
            Email = model.Email,
            HireDate = model.HireDate,
            Photo = model.Photo
        };

        var result = await _employeeService.CreateEmployeeAsync(dto);

        if (result == null)
        {
            await PopulateDepartments(model);
            _notyf.Error("Email already exists. Please use a different email.");
            return View(model);
        }

        _notyf.Success("Employee created successfully");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditAsync(Guid id)
    {
        var employeeDto = await _employeeService.GetEmployeeByIdAsync(id);
        if (employeeDto == null)
        {
            return NotFound();
        }

        var departmentsDto = await _departmentService.GetAllDepartmentsAsync();

        var viewModel = new UpdateEmployeeViewModel
        {
            Id = employeeDto.Id,
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
            HireDate = employeeDto.HireDate,
            Salary = employeeDto.Salary,
            DepartmentId = employeeDto.DepartmentId,
            ImageUrl = employeeDto.ImageUrl,
            Departments = departmentsDto.Departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAsync(Guid id, UpdateEmployeeViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var departmentsDto = await _departmentService.GetAllDepartmentsAsync();
            model.Departments = departmentsDto.Departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    _notyf.Warning($"Field '{entry.Key}': {error.ErrorMessage}");
                }
            }
        }

        try
        {
            var updateDto = new UpdateEmployeeDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                HireDate = model.HireDate,
                Salary = model.Salary,
                DepartmentId = model.DepartmentId,
                Photo = model.Photo,
                ImageUrl = model.ImageUrl
            };

            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(updateDto);

            if (updatedEmployee == null)
                return NotFound();

            _notyf.Success("Employee updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError("", "An error occurred while updating the employee.");
            return View(model);
        }
    }


    public async Task<IActionResult> Delete(Guid id)
    {
        var employeeDto = await _employeeService.GetEmployeeByIdAsync(id);

        if (employeeDto == null)
        {
            return NotFound();
        }

        var viewModel = new EmployeeViewModel
        {
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            HireDate = employeeDto.HireDate,
            Email = employeeDto.Email,
            Salary = employeeDto.Salary
        };
        return View(viewModel);
    }

    [HttpPost("Employee/Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
        _notyf.Success("Employee Deleted successfully");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(Guid employeeId)
    {
        await _employeeService.DeleteImageAsync(employeeId);
        return RedirectToAction("Details", "Employee", new { id = employeeId });
    }
}
