using Application.Dtos;
using Application.Services.Document;
using Application.Services.Employee;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[Authorize]
public class DocumentController(
    IDocumentService _documentService,
    IEmployeeService _employeeService,
    INotyfService _notyf) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Upload(UploadDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            _notyf.Error("Please fill all required fields.");
            return RedirectToAction("Details", "Employee", new { id = dto.EmployeeId });
        }

        try
        {
            await _documentService.UploadDocumentAsync(dto);
            _notyf.Success($"{dto.DocumentType} uploaded successfully!");
        }
        catch (Exception ex)
        {
            _notyf.Error($"Upload failed: {ex.Message}");
        }

        return RedirectToAction("Details", "Employee", new { id = dto.EmployeeId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, Guid employeeId)
    {
        var result = await _documentService.DeleteDocumentAsync(id);
        if (result)
        {
            _notyf.Success("Document deleted successfully.");
        }
        else
        {
            _notyf.Error("Failed to delete document.");
        }

        return RedirectToAction("Details", "Employee", new { id = employeeId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExpiringDocuments()
    {
        var documents = await _documentService.GetExpiringDocumentsAsync(30);
        return View(documents);
    }
}
