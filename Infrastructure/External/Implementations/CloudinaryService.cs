using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Infrastructure;

[RegisterService(typeof(ICloudinaryService))]
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IAppConfiguration appConfiguration)
    {
        var cloudinary = appConfiguration.GetCloudinaryOptions();
        var account = new Account(
            cloudinary.CloudName,
            cloudinary.ApiKey,
            cloudinary.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder = "fevent-images")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be empty");

        ImageUploadResult uploadResult;

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = BuildTransformation(folder),
            Folder = folder
        };

        uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Cloudinary Error: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.AbsoluteUri;
    }

    public async Task<string> UploadLocalFileAsync(string filePath, string folder = "fevent-images")
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty");

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File does not exist", filePath);

        ImageUploadResult uploadResult;

        await using var stream = File.OpenRead(filePath);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(Path.GetFileName(filePath), stream),
            Transformation = BuildTransformation(folder),
            Folder = folder
        };

        uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception($"Cloudinary Error: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.AbsoluteUri;
    }

    public async Task DeletePhotoAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return;

        var deletionParams = new DeletionParams(publicId);
        var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

        if (deletionResult.Result != "ok" && deletionResult.Result != "not found")
            throw new Exception($"Cloudinary deletion failed: {deletionResult.Result}");
    }

    private static Transformation BuildTransformation(string folder)
    {
        if (folder.Contains("avatar", StringComparison.OrdinalIgnoreCase))
        {
            return new Transformation().Width(512).Height(512).Crop("fill").Gravity("face");
        }

        return new Transformation().Width(1200).Height(630).Crop("fill").Gravity("auto");
    }
}
