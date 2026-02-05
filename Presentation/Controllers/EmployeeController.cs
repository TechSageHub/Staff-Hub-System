using Application.Dtos;
using Application.Services.Department;
using Application.Services.Employee;
using Application.Services.Address;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.DtoMapping;
using Presentation.Models;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly INotyfService _notyf;
    private readonly IAddressService _addressService;

    public EmployeeController(IEmployeeService employeeService, INotyfService notyf, IDepartmentService departmentService, IAddressService addressService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _notyf = notyf;
        _addressService = addressService;
    }

    

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin"))
        {
            var self = await _employeeService.GetEmployeeByUserIdAsync(userId!);
            if (self == null)
            {
                return RedirectToAction("EditProfile", "Account");
            }

            return RedirectToAction(nameof(Details), new { id = self.Id });
        }

        var employees = await _employeeService.GetAllEmployeesAsync(userId);
        var viewModel = employees.ToViewModel();
        return View(viewModel);
    }

    public async Task<IActionResult> MyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var self = await _employeeService.GetEmployeeByUserIdAsync(userId!);
        if (self == null)
        {
            return RedirectToAction("EditProfile", "Account");
        }

        return RedirectToAction(nameof(Details), new { id = self.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin"))
        {
            var currentEmployee = await _employeeService.GetEmployeeByUserIdAsync(userId!);
            if (currentEmployee == null || currentEmployee.Id != id)
            {
                return Forbid();
            }
        }

        var employeeDto = await _employeeService.GetEmployeeByIdAsync(id, userId);
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
            Gender = employeeDto.Gender,
            PhoneNumber = employeeDto.PhoneNumber,
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateEmployeeViewModel();
        await PopulateCreateData(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
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
            Gender = model.Gender,
            PhoneNumber = model.PhoneNumber,
            Photo = model.Photo,
            UserId = null,
            Street = model.Street,
            City = model.City,
            State = model.State,
            Country = model.Country
        };

        var result = await _employeeService.CreateEmployeeAsync(dto);

        if (result == null)
        {
            await PopulateCreateData(model);
            _notyf.Error("Email already exists. Please use a different email.");
            return View(model);
        }

        _notyf.Success("Employee created successfully");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditAsync(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employeeDto = await _employeeService.GetEmployeeByIdAsync(id, userId);
        if (employeeDto == null)
        {
            return NotFound();
        }

        var viewModel = new UpdateEmployeeViewModel
        {
            Id = employeeDto.Id,
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
            HireDate = employeeDto.HireDate,
            Salary = employeeDto.Salary,
            Gender = employeeDto.Gender,
            PhoneNumber = employeeDto.PhoneNumber,
            DepartmentId = employeeDto.DepartmentId,
            ImageUrl = employeeDto.ImageUrl,
            Street = employeeDto.Address?.Street,
            City = employeeDto.Address?.City,
            State = employeeDto.Address?.State,
            Country = employeeDto.Address?.Country
        };

        await PopulateUpdateData(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAsync(Guid id, UpdateEmployeeViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateUpdateData(model);

            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    _notyf.Warning($"Field '{entry.Key}': {error.ErrorMessage}");
                }
            }

            return View(model);
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
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                DepartmentId = model.DepartmentId,
                Photo = model.Photo,
                ImageUrl = model.ImageUrl,
                Street = model.Street,
                City = model.City,
                State = model.State
            };

            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(updateDto);

            if (updatedEmployee == null)
                return NotFound();

            _notyf.Success("Employee updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await PopulateUpdateData(model);
            ModelState.AddModelError("", "An error occurred while updating the employee.");
            return View(model);
        }
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employeeDto = await _employeeService.GetEmployeeByIdAsync(id, userId);

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
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
        _notyf.Success("Employee Deleted successfully");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid employeeId)
    {
        await _employeeService.DeleteImageAsync(employeeId);
        return RedirectToAction("Details", "Employee", new { id = employeeId });
    }

    private async Task PopulateCreateData(CreateEmployeeViewModel model)
    {
        var departmentsDto = await _departmentService.GetAllDepartmentsAsync();
        model.Departments = departmentsDto.Departments.Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = d.Name
        }).ToList();

        var states = _addressService.GetAllStates();
        model.States = states.Select(s => new SelectListItem
        {
            Value = s,
            Text = s
        }).ToList();
    }

    private async Task PopulateUpdateData(UpdateEmployeeViewModel model)
    {
        var departmentsDto = await _departmentService.GetAllDepartmentsAsync();
        model.Departments = departmentsDto.Departments.Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = d.Name
        }).ToList();

        var states = _addressService.GetAllStates();
        model.States = states.Select(s => new SelectListItem
        {
            Value = s,
            Text = s
        }).ToList();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult BulkUpload()
    {
        return View(new BulkUploadViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpload(BulkUploadViewModel model)
    {
        if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
        {
            _notyf.Error("Please select a valid CSV file.");
            return View(model);
        }

        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            int successCount = 0;
            int failCount = 0;

            using (var reader = new StreamReader(model.File.OpenReadStream()))
            {
                // Skip header
                await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 3)
                    {
                        failCount++;
                        continue;
                    }

                    try
                    {
                        // Format: FirstName, LastName, Email, Salary, DepartmentName, Street, City, State, Country
                        var firstName = values[0].Trim();
                        var lastName = values[1].Trim();
                        var email = values[2].Trim();
                        var salaryStr = values.Length > 3 ? values[3].Trim() : "0";
                        var salary = decimal.TryParse(salaryStr, out var s) ? s : 0;
                        var deptName = values.Length > 4 ? values[4].Trim() : "";
                        
                        var deptId = departments.Departments
                            .FirstOrDefault(d => d.Name.Equals(deptName, StringComparison.OrdinalIgnoreCase))?.Id 
                            ?? departments.Departments.FirstOrDefault()?.Id 
                            ?? Guid.Empty;

                        var dto = new CreateEmployeeDto
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            Salary = salary,
                            DepartmentId = deptId,
                            HireDate = DateTime.Now,
                            Street = values.Length > 5 ? values[5].Trim() : null,
                            City = values.Length > 6 ? values[6].Trim() : null,
                            State = values.Length > 7 ? values[7].Trim() : null,
                            Country = values.Length > 8 ? values[8].Trim() : "Nigeria"
                        };

                        var result = await _employeeService.CreateEmployeeAsync(dto);
                        if (result != null) successCount++;
                        else failCount++;
                    }
                    catch
                    {
                        failCount++;
                    }
                }
            }

            if (successCount > 0) 
                _notyf.Success($"{successCount} employees uploaded successfully.");
            
            if (failCount > 0) 
                _notyf.Warning($"{failCount} rows failed to upload.");

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _notyf.Error("An error occurred during bulk upload: " + ex.Message);
            return View(model);
        }
    }
}
