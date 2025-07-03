using BuildTruckBack.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckBack.Machinery.Interfaces.REST.Resources;

public class CreateMachineryResource
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string MachineryType { get; set; } = string.Empty;
    public MachineryStatus Status { get; set; } = MachineryStatus.Active;
    public string Provider { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? PersonnelId { get; set; }
    public DateTime RegisterDate { get; set; } = DateTime.UtcNow.Date;
    public IFormFile? ImageFile { get; set; } // Added for image upload

}