using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Interfaces.REST.Resources;
namespace BuildTruckBack.Machinery.Interfaces.REST.Transform;

public static class MachineryResourceAssembler
{
    public static MachineryResource ToResourceFromEntity(Domain.Model.Aggregates.Machinery machinery)
    {
        return new MachineryResource
        {
            Id = machinery.Id,
            Name = machinery.Name,
            LicensePlate = machinery.LicensePlate,
            RegisterDate = machinery.RegisterDate,
            Status = machinery.Status,
            Provider = machinery.Provider,
            Description = machinery.Description,
            ProjectId = machinery.ProjectId
        };
    }

    public static CreateMachineryCommand ToCommandFromSaveResource(SaveMachineryResource resource)
    {
        return new CreateMachineryCommand
        {
            Name = resource.Name,
            LicensePlate = resource.LicensePlate,
            RegisterDate = resource.RegisterDate,
            Status = resource.Status,
            Provider = resource.Provider,
            Description = resource.Description,
            ProjectId = resource.ProjectId
        };
    }

    public static UpdateMachineryCommand ToCommandFromUpdateResource(UpdateMachineryResource resource, int id)
    {
        return new UpdateMachineryCommand
        {
            Id = id,
            Name = resource.Name,
            LicensePlate = resource.LicensePlate,
            RegisterDate = resource.RegisterDate,
            Status = resource.Status,
            Provider = resource.Provider,
            Description = resource.Description,
            ProjectId = resource.ProjectId
        };
    }
}