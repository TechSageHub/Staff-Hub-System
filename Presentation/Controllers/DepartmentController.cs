using Application.Dtos;
using Application.Services.Department;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.DtoMapping;
using Presentation.Models;

namespace Presentation.Controllers;
[Authorize(Roles = "Admin")]
public class DepartmentController : BaseController
{
    private readonly IDepartmentService _departmentService;
    private readonly INotyfService _notyf;
    public DepartmentController(IDepartmentService departmentService, INotyfService notyf)
    {
        _departmentService = departmentService; 
        _notyf = notyf; 
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();

        var viewModel = departments.ToViewModel();

        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);

        if (departmentDto == null)
        {
            return NotFound();
        }

        var viewModel = departmentDto.ToViewModel();

        return View(viewModel);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult Create()
    {
        return View(); 
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _notyf.Information("Please fill in all required fields correctly.");
            return View(model);
        }

        var viewModel = new CreateDepartmentDto
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description
        };

        var result = await _departmentService.CreateDepartmentAsync(viewModel);

        if (result == null)
        {
            _notyf.Error("An error occured");
            return View(model);
        }
        _notyf.Success("Department created successfully");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditAsync(Guid id)
    {
        var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);

        if (departmentDto == null)
        {
            return NotFound();
        }

        var viewModel = new UpdateDepartmentViewModel
        {
            Id = departmentDto.Id,
            Name = departmentDto.Name,
            Description = departmentDto.Description
        };

        return View(viewModel);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditAsync(Guid id, UpdateDepartmentViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            _notyf.Warning("Please correct the highlighted fields.");
            return View(model);
        }

        try
        {
            var viewModel = new UpdateDepartmentDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };
            var updatedDepartment = await _departmentService.UpdateDepartmentAsync(viewModel);

            if (updatedDepartment == null)
            {
                return NotFound();
            }

            _notyf.Success("Department updated successfully");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating the department.");
            _notyf.Error("An error occurred while updating the department.");
            return View(model);
        }
    }

     
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);

        if (departmentDto == null)
        {
            return NotFound();
        }

        var viewModel = departmentDto.ToViewModel();

        return View(viewModel);
    }

    [HttpPost("Department/Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id);
            _notyf.Success("Department deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            _notyf.Warning(ex.Message);
        }
        catch (Exception)
        {
            _notyf.Error("An error occurred while deleting the department.");
        }
        
        return RedirectToAction(nameof(Index));
    }
}
