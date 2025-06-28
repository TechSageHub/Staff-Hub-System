using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Data.Context;
using Microsoft.AspNetCore.Http;

namespace Application.Services.UploadImage
{
    public class ImageService : IImageService
    {

        private readonly Cloudinary _cloudinary;

        public ImageService(Cloudinary cloudinary)
        {

            _cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill"),
                Folder = "employees"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            return result.SecureUrl.AbsoluteUri;
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            return deletionResult.Result == "ok";
        }

        public string ExtractPublicIdFromUrl(string imageUrl)
        {

            if (string.IsNullOrEmpty(imageUrl))
                return null!;

            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');

            var uploadIndex = Array.IndexOf(segments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length)
                return null!;


            var publicIdWithExtension = string.Join("/", segments.Skip(uploadIndex + 2));

            var publicId = Path.Combine(Path.GetDirectoryName(publicIdWithExtension) ?? string.Empty,
                                        Path.GetFileNameWithoutExtension(publicIdWithExtension))
                           .Replace("\\", "/");

            return publicId;
        }
    }
}
