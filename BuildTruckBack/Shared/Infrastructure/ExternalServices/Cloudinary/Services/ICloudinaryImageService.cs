namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

/// <summary>
/// Cloudinary image service interface for cloud image operations
/// </summary>
public interface ICloudinaryImageService
{
    /// <summary>
    /// Upload image to Cloudinary
    /// </summary>
    /// <param name="imageBytes">Image file bytes</param>
    /// <param name="fileName">File name with extension</param>
    /// <param name="folder">Folder path in Cloudinary</param>
    /// <returns>Public URL of uploaded image</returns>
    Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string folder);

    /// <summary>
    /// Delete image from Cloudinary
    /// </summary>
    /// <param name="publicId">Public ID of the image to delete</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteImageAsync(string publicId);

    /// <summary>
    /// Get optimized image URL with Cloudinary transformations
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <param name="width">Desired width</param>
    /// <param name="height">Desired height</param>
    /// <returns>Optimized image URL</returns>
    string GetOptimizedImageUrl(string publicId, int width = 200, int height = 200);

    /// <summary>
    /// Validate image file according to Cloudinary settings
    /// </summary>
    /// <param name="imageBytes">Image file bytes</param>
    /// <param name="fileName">File name with extension</param>
    /// <returns>Validation result with error message if invalid</returns>
    (bool IsValid, string ErrorMessage) ValidateImage(byte[] imageBytes, string fileName);

    /// <summary>
    /// Extract public ID from Cloudinary URL
    /// </summary>
    /// <param name="cloudinaryUrl">Full Cloudinary URL</param>
    /// <returns>Public ID for deletion</returns>
    string ExtractPublicIdFromUrl(string cloudinaryUrl);
}