using Microsoft.AspNetCore.Http;
namespace Application.Services.UploadImage
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);

        Task<bool> DeleteImageAsync(string publicId);

        string ExtractPublicIdFromUrl(string imageUrl);
    }
}
