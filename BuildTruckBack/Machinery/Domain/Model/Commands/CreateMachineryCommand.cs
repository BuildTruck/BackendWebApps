using BuildTruckBack.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckBack.Machinery.Domain.Model.Commands;

public record CreateMachineryCommand(
    int ProjectId,
    string Name,
    string LicensePlate,
    string MachineryType,
    MachineryStatus Status,
    string Provider,
    string Description,
    int? PersonnelId,
    DateTime RegisterDate)
{
    
}