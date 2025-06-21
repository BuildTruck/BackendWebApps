using BuildTruckBack.Personnel.Application.ACL.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckBack.Personnel.Infrastructure.ACL;

/// <summary>
/// ACL Adapter for Cloudinary specific to Personnel context
/// Uses shared CloudinaryImageService with personnel/ folder
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly ILogger<CloudinaryService> _logger;
    
    private const string PERSONNEL_FOLDER = "personnel/";

    public CloudinaryService(ICloudinaryImageService cloudinaryImageService, ILogger<CloudinaryService> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _logger = logger;
    }

    public async Task<string> UploadPersonnelPhotoAsync(byte[] imageBytes, string fileName, int personnelId)
    {
        try
        {
            _logger.LogDebug("Uploading personnel photo: {FileName} for personnel: {PersonnelId}", fileName, personnelId);

            // Generate unique filename with personnel ID
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"personnel_{personnelId}_{timestamp}{extension}";

            var imageUrl = await _cloudinaryImageService.UploadImageAsync(imageBytes, uniqueFileName, PERSONNEL_FOLDER);
            
            _logger.LogInformation("✅ Personnel photo uploaded successfully: {ImageUrl}", imageUrl);
            return imageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to upload personnel photo for personnel: {PersonnelId}", personnelId);
            throw new InvalidOperationException($"Failed to upload personnel photo: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeletePersonnelPhotoAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
            {
                _logger.LogWarning("Invalid image URL for deletion: {ImageUrl}", imageUrl);
                return false;
            }

            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Could not extract public ID from URL: {ImageUrl}", imageUrl);
                return false;
            }

            var success = await _cloudinaryImageService.DeleteImageAsync(publicId);
            
            if (success)
            {
                _logger.LogInformation("✅ Personnel photo deleted successfully: {ImageUrl}", imageUrl);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to delete personnel photo: {ImageUrl}", imageUrl);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting personnel photo: {ImageUrl}", imageUrl);
            return false;
        }
    }

    public string GetOptimizedPhotoUrl(string imageUrl, int width = 200, int height = 200)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.Contains("cloudinary.com"))
                return imageUrl;

            var publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
                return imageUrl;

            var optimizedUrl = _cloudinaryImageService.GetOptimizedImageUrl(publicId, width, height);
            
            _logger.LogDebug("Generated optimized URL for personnel photo: {OptimizedUrl}", optimizedUrl);
            return optimizedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating optimized URL for: {ImageUrl}", imageUrl);
            return imageUrl;
        }
    }
}