using BuildTruckBack.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckBack.Machinery.Interfaces.REST.Resources;

public class MachineryResource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime RegisterDate { get; set; }
    public MachineryStatus Status { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}