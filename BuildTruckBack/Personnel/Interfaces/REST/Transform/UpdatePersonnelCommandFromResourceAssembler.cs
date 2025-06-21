using BuildTruckBack.Personnel.Domain.Model.Commands;
using BuildTruckBack.Personnel.Domain.Model.ValueObjects;
using BuildTruckBack.Personnel.Interfaces.REST.Resources;

namespace BuildTruckBack.Personnel.Interfaces.REST.Transform;

public static class UpdatePersonnelCommandFromResourceAssembler
{
    public static UpdatePersonnelCommand ToCommandFromResource(int personnelId, UpdatePersonnelResource resource)
    {
        if (!Enum.TryParse<PersonnelType>(resource.PersonnelType, out var personnelType))
            throw new ArgumentException($"Invalid personnel type: {resource.PersonnelType}");

        if (!Enum.TryParse<PersonnelStatus>(resource.Status, out var status))
            throw new ArgumentException($"Invalid status: {resource.Status}");

        return new UpdatePersonnelCommand(
            personnelId,
            resource.Name,
            resource.Lastname,
            resource.Position,
            resource.Department,
            personnelType,
            status,
            resource.MonthlyAmount,
            resource.Discount,
            resource.Bank ?? string.Empty,
            resource.AccountNumber ?? string.Empty,
            resource.StartDate,
            resource.EndDate,
            resource.Phone ?? string.Empty,
            resource.Email ?? string.Empty,
            null // AvatarUrl will be handled separately in controller
        );
    }
}