namespace BuildTruckBack.Personnel.Application.ACL.Services;

/// <summary>
/// Cloudinary service interface for Personnel context
/// Handles personnel photo uploads to cloudinary.com/personnel/
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload personnel photo
    /// </summary>
    /// <param name="imageBytes">Image as byte array</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="personnelId">Personnel ID for unique naming</param>
    /// <returns>Cloudinary URL</returns>
    Task<string> UploadPersonnelPhotoAsync(byte[] imageBytes, string fileName, int personnelId);

    /// <summary>
    /// Delete personnel photo
    /// </summary>
    /// <param name="imageUrl">Cloudinary URL</param>
    /// <returns>Success status</returns>
    Task<bool> DeletePersonnelPhotoAsync(string imageUrl);

    /// <summary>
    /// Get optimized personnel photo URL
    /// </summary>
    /// <param name="imageUrl">Original Cloudinary URL</param>
    /// <param name="width">Target width (default: 200)</param>
    /// <param name="height">Target height (default: 200)</param>
    /// <returns>Optimized URL</returns>
    string GetOptimizedPhotoUrl(string imageUrl, int width = 200, int height = 200);
}