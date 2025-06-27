using Application.Dtos;
using Application.Services.Department;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.DtoMapping;
using Presentation.Models;

namespace Presentation.Controllers;
[Authorize]
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

        var viewModel = new DepartmentViewModel
        {
            Id = departmentDto.Id,
            Name = departmentDto.Name,
            Description = departmentDto.Description
        };

        return View(viewModel);
    }

    public ActionResult Create()
    {
        return View(); 
    }

    [HttpPost]
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
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditAsync(Guid id, UpdateDepartmentViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
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

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating the department.");

            return View(model);
        }
    }

     
    public async Task<IActionResult> Delete(Guid id)
    {
        var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);

        if (departmentDto == null)
        {
            return NotFound();
        }

        var viewModel = new DepartmentViewModel
        {
            Id = departmentDto.Id,
            Name = departmentDto.Name,
            Description = departmentDto.Description
        };

        return View(viewModel);
    }

    [HttpPost("Department/Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _departmentService.DeleteDepartmentAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
