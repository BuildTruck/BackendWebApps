using System.IO;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Application.ACL.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;
using Microsoft.Extensions.Logging;

namespace BuildTruckBack.Incidents.Infrastructure.ACL;

public class CloudinaryService : ICloudinaryService
{
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(ICloudinaryImageService cloudinaryImageService, ILogger<CloudinaryService> logger)
    {
        _cloudinaryImageService = cloudinaryImageService;
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(string imagePath)
    {
        _logger.LogInformation("Uploading image for incident: {ImagePath}", imagePath);
        string folder = "incidents";
        string fileName = Path.GetFileName(imagePath);
        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
        return await _cloudinaryImageService.UploadImageAsync(imageBytes, folder, fileName);
    }
}