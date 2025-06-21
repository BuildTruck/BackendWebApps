using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Model.ValueObjects;
using BuildTruckBack.Machinery.Interfaces.REST.Resources;
namespace BuildTruckBack.Machinery.Interfaces.REST.Transform;

public static class MachineryResourceAssembler
{
    public static MachineryResource ToResource(this Domain.Model.Aggregates.Machinery machinery)
    {
        return new MachineryResource
        {
            Id = machinery.Id,
            ProjectId = machinery.ProjectId,
            Name = machinery.Name,
            LicensePlate = machinery.LicensePlate,
            MachineryType = machinery.MachineryType,
            Status = Enum.Parse<MachineryStatus>(machinery.Status),
            Provider = machinery.Provider,
            Description = machinery.Description,
            PersonnelId = machinery.PersonnelId,
            ImageUrl = machinery.ImageUrl,
            CreatedAt = machinery.CreatedAt,
            UpdatedAt = machinery.UpdatedAt,
            RegisterDate = machinery.RegisterDate
        };
    }

    public static Domain.Model.Aggregates.Machinery ToDomain(this CreateMachineryResource resource)
    {
        return new Domain.Model.Aggregates.Machinery
        {
            ProjectId = resource.ProjectId,
            Name = resource.Name,
            LicensePlate = resource.LicensePlate,
            MachineryType = resource.MachineryType,
            Status = resource.Status.ToString(),
            Provider = resource.Provider,
            Description = resource.Description,
            PersonnelId = resource.PersonnelId,
            RegisterDate = resource.RegisterDate
        };
    }
}