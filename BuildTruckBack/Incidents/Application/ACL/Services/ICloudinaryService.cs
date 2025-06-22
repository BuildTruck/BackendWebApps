using System.Threading.Tasks;

namespace BuildTruckBack.Incidents.Application.ACL.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(string imagePath);
}