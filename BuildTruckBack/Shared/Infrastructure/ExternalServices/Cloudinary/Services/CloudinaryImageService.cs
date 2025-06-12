using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using Microsoft.Extensions.Options;

namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

/// <summary>
/// Cloudinary image service implementation
/// </summary>
public class CloudinaryImageService : ICloudinaryImageService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageService> _logger;

    public CloudinaryImageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!_settings.IsValid)
        {
            throw new InvalidOperationException("Cloudinary settings are not properly configured");
        }

        // Initialize Cloudinary client
        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    /// <summary>
    /// Upload image to Cloudinary with optimizations
    /// </summary>
    public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string folder)
    {
        try
        {
            // Validate image first
            var validation = ValidateImage(imageBytes, fileName);
            if (!validation.IsValid)
            {
                throw new ArgumentException(validation.ErrorMessage);
            }

            // Generate unique public ID
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var publicId = $"{folder}{fileNameWithoutExt}_{timestamp}";

            using var stream = new MemoryStream(imageBytes);
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Folder = folder,
                Transformation = new Transformation()
                    .Quality("auto")           // Auto quality optimization
                    .FetchFormat("auto")       // Auto format (WebP, etc.)
                    .Width(500)                // Max width
                    .Height(500)               // Max height
                    .Crop("limit"),            // Don't upscale, just limit size
                Overwrite = true,
                UseFilename = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("✅ Image uploaded successfully to Cloudinary: {PublicId}", publicId);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to upload image to Cloudinary");
            throw;
        }
    }

    /// <summary>
    /// Delete image from Cloudinary
    /// </summary>
    public async Task<bool> DeleteImageAsync(string publicId)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Cannot delete image: publicId is null or empty");
                return false;
            }

            var deleteParams = new DeletionParams(publicId);
            var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

            var success = deleteResult.Result == "ok";
            
            if (success)
            {
                _logger.LogInformation("✅ Image deleted successfully from Cloudinary: {PublicId}", publicId);
            }
            else
            {
                _logger.LogWarning("⚠️ Cloudinary delete result: {Result} for {PublicId}", deleteResult.Result, publicId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete image from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }

    /// <summary>
    /// Get optimized image URL with transformations
    /// </summary>
    public string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            var transformation = new Transformation()
                .Width(width)
                .Height(height)
                .Crop("fill")              // Crop to exact dimensions
                .Quality("auto")           // Auto quality
                .FetchFormat("auto");      // Auto format

            return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to generate optimized URL for: {PublicId}", publicId);
            return string.Empty;
        }
    }

    /// <summary>
    /// Validate image file according to settings
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateImage(byte[] imageBytes, string fileName)
    {
        // Check file size
        if (imageBytes.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
            return (false, $"File size exceeds maximum allowed size of {maxSizeMB:F1}MB");
        }

        // Check file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            var allowedExts = string.Join(", ", _settings.AllowedExtensions);
            return (false, $"File type {extension} not allowed. Allowed types: {allowedExts}");
        }

        // Basic image validation - check if it starts with valid image headers
        if (!IsValidImageFormat(imageBytes))
        {
            return (false, "File does not appear to be a valid image");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Extract public ID from Cloudinary URL for deletion
    /// </summary>
    public string ExtractPublicIdFromUrl(string cloudinaryUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(cloudinaryUrl))
                return string.Empty;

            // Example URL: https://res.cloudinary.com/demo/image/upload/v1234567890/buildtruck/profiles/user_123456.jpg
            // Public ID: buildtruck/profiles/user_123456

            var uri = new Uri(cloudinaryUrl);
            var pathSegments = uri.AbsolutePath.Split('/');

            // Find the upload segment
            var uploadIndex = Array.IndexOf(pathSegments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= pathSegments.Length)
                return string.Empty;

            // Skip version if present (starts with 'v' followed by numbers)
            var startIndex = uploadIndex + 1;
            if (pathSegments[startIndex].StartsWith('v') && pathSegments[startIndex].Skip(1).All(char.IsDigit))
            {
                startIndex++;
            }

            // Combine remaining segments (except the last one which we remove extension from)
            var publicIdSegments = pathSegments[startIndex..];
            var lastSegment = publicIdSegments[^1];
            var lastSegmentWithoutExt = Path.GetFileNameWithoutExtension(lastSegment);
            publicIdSegments[^1] = lastSegmentWithoutExt;

            return string.Join("/", publicIdSegments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract public ID from URL: {Url}", cloudinaryUrl);
            return string.Empty;
        }
    }

    /// <summary>
    /// Basic image format validation by checking file headers
    /// </summary>
    private static bool IsValidImageFormat(byte[] imageBytes)
    {
        if (imageBytes.Length < 4)
            return false;

        // Check for common image format signatures
        // JPEG: FF D8 FF
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47
        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            return true;

        return false;
    }
}