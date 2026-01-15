using Microsoft.AspNetCore.Http;

namespace Application.Dtos;

public class EmployeeDocumentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = default!;
    public string DocumentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string FileUrl { get; set; } = default!;
    public DateTime UploadedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Now.AddDays(30);
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
}

public class UploadDocumentDto
{
    public Guid EmployeeId { get; set; }
    public string DocumentType { get; set; } = default!;
    public IFormFile File { get; set; } = default!;
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}
