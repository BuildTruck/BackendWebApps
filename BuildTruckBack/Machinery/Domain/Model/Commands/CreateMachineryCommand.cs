using BuildTruckBack.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckBack.Machinery.Domain.Model.Commands;

public record CreateMachineryCommand
{
    public string Name { get; init; } = string.Empty;
    public string LicensePlate { get; init; } = string.Empty;
    public DateTime RegisterDate { get; init; }
    public MachineryStatus Status { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ProjectId { get; init; } = string.Empty;
}