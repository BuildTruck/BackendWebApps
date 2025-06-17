using BuildTruckBack.Machinery.Application.ACL.Services;
using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Machinery.Application.Internal.CommandServices;

public class MachineryCommandService(
    IMachineryRepository machineryRepository,
    IProjectContextService projectContextService,
    IUnitOfWork unitOfWork) : IMachineryCommandHandler
{
    public async Task<Domain.Model.Aggregates.Machinery?> Handle(CreateMachineryCommand command)
    {
        // Validate ProjectId exists
        var project = await projectContextService.GetProjectByIdAsync(command.ProjectId);
        if (project == null)
            throw new Exception($"Project with ID {command.ProjectId} not found.");

        // Validate LicensePlate uniqueness
        var existingMachinery = await machineryRepository.FindByLicensePlateAsync(command.LicensePlate);
        if (existingMachinery != null)
            throw new Exception($"Machinery with license plate {command.LicensePlate} already exists.");

        var machinery = new Domain.Model.Aggregates.Machinery
        {
            Name = command.Name,
            LicensePlate = command.LicensePlate,
            RegisterDate = command.RegisterDate,
            Status = command.Status,
            Provider = command.Provider,
            Description = command.Description,
            ProjectId = command.ProjectId
        };

        await machineryRepository.AddAsync(machinery);
        await unitOfWork.CompleteAsync();
        return machinery;
    }

    public async Task<Domain.Model.Aggregates.Machinery?> Handle(UpdateMachineryCommand command)
    {
        var existingMachinery = await machineryRepository.FindByIdAsync(command.Id);
        if (existingMachinery == null)
            return null;

        // Validate ProjectId exists
        var project = await projectContextService.GetProjectByIdAsync(command.ProjectId);
        if (project == null)
            throw new Exception($"Project with ID {command.ProjectId} not found.");

        // Update fields
        existingMachinery.Name = command.Name;
        existingMachinery.LicensePlate = command.LicensePlate;
        existingMachinery.RegisterDate = command.RegisterDate;
        existingMachinery.Status = command.Status;
        existingMachinery.Provider = command.Provider;
        existingMachinery.Description = command.Description;
        existingMachinery.ProjectId = command.ProjectId;

        machineryRepository.Update(existingMachinery);
        await unitOfWork.CompleteAsync();
        return existingMachinery;
    }
}