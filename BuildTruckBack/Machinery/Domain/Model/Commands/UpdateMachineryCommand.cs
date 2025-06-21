using BuildTruckBack.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckBack.Machinery.Domain.Model.Commands;

public record UpdateMachineryCommand(
    int Id,
    int ProjectId,
    string Name,
    string LicensePlate,
    string MachineryType,
    MachineryStatus Status,
    string Provider,
    string Description,
    int? PersonnelId);