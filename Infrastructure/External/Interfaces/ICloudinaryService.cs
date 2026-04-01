using Microsoft.AspNetCore.Http;

namespace Infrastructure;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder = "fevent-images");
    Task<string> UploadLocalFileAsync(string filePath, string folder = "fevent-images");
    Task DeletePhotoAsync(string publicId);
}
