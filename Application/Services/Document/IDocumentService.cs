using Application.Dtos;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Document;

public interface IDocumentService
{
    Task<EmployeeDocumentDto> UploadDocumentAsync(UploadDocumentDto dto);
    Task<List<EmployeeDocumentDto>> GetEmployeeDocumentsAsync(Guid employeeId);
    Task<bool> DeleteDocumentAsync(Guid documentId);
    Task<List<EmployeeDocumentDto>> GetExpiringDocumentsAsync(int daysAhead = 30);
}
