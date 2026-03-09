namespace Application;

internal static class CloudinaryHelper
{
    public static string? ExtractPublicIdFromUrl(string url)
    {
        // Cloudinary URL format: .../upload/<folder>/<publicId>.<ext>
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return null;
            }

            var lastSegment = segments[^1];
            var dotIndex = lastSegment.LastIndexOf('.');
            var publicIdWithFolder = dotIndex > 0 ? lastSegment[..dotIndex] : lastSegment;

            // Include folder path if exists (e.g., hanh-trang-so-avatars/<publicId>)
            if (segments.Length >= 2)
            {
                var folder = segments[^2];
                return $"{folder}/{publicIdWithFolder}";
            }

            return publicIdWithFolder;
        }
        catch
        {
            return null;
        }
    }
}
