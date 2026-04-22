using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application.Services.UploadImage
{
    public class ImageService : IImageService
    {
        private readonly CloudinarySettings _cloudinarySettings;

        public ImageService(IOptions<CloudinarySettings> cloudinaryOptions)
        {
            _cloudinarySettings = cloudinaryOptions.Value;
        }

        private Cloudinary CreateClient()
        {
            if (string.IsNullOrWhiteSpace(_cloudinarySettings.CloudName) ||
                string.IsNullOrWhiteSpace(_cloudinarySettings.ApiKey) ||
                string.IsNullOrWhiteSpace(_cloudinarySettings.ApiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary is not configured. Set CloudinarySettings:CloudName, CloudinarySettings:ApiKey, and CloudinarySettings:ApiSecret.");
            }

            var account = new Account(
                _cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret);

            return new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var cloudinary = CreateClient();
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill"),
                Folder = "employees"
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            return result.SecureUrl.AbsoluteUri;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var cloudinary = CreateClient();
            using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            return result.SecureUrl.AbsoluteUri;
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var cloudinary = CreateClient();

            if (string.IsNullOrEmpty(publicId))
                return false;

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await cloudinary.DestroyAsync(deletionParams);

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
