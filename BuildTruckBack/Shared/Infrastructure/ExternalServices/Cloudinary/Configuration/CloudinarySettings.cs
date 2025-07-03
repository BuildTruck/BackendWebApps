namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;

/// <summary>
/// Cloudinary configuration settings
/// </summary>
public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string ProfileImagesFolder { get; set; } = "buildtruck/profiles/";
    public string MachineryImagesFolder { get; set; } = "buildtruck/machinery/"; // Added for machinery images

    public int MaxFileSizeBytes { get; set; } = 5242880; // 5MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png" };
    public string TransformationPreset { get; set; } = "profile_pic";
    
    /// <summary>
    /// Validate that all required settings are present
    /// </summary>
    public bool IsValid => 
        !string.IsNullOrEmpty(CloudName) && 
        !string.IsNullOrEmpty(ApiKey) && 
        !string.IsNullOrEmpty(ApiSecret);
}