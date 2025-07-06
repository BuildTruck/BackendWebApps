using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Machinery.Application.Internal.OutboundServices;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class MachineryContextService : IMachineryContextService
{
    private readonly IMachineryFacade _machineryFacade;

    public MachineryContextService(IMachineryFacade machineryFacade)
    {
        _machineryFacade = machineryFacade;
    }

    public async Task<bool> MachineryExistsAsync(int machineryId)
    {
        var machinery = await _machineryFacade.GetMachineryByIdAsync(machineryId);
        return machinery != null;
    }

    public async Task<string> GetMachineryNameAsync(int machineryId)
    {
        var machinery = await _machineryFacade.GetMachineryByIdAsync(machineryId);
        return machinery?.Name ?? string.Empty;
    }

    public async Task<int> GetMachineryProjectIdAsync(int machineryId)
    {
        var machinery = await _machineryFacade.GetMachineryByIdAsync(machineryId);
        return machinery?.ProjectId ?? 0;
    }

    public async Task<bool> MachineryBelongsToProjectAsync(int machineryId, int projectId)
    {
        return await _machineryFacade.ValidateMachineryExistsInProjectAsync(machineryId, projectId);
    }

    public async Task<string> GetMachineryStatusAsync(int machineryId)
    {
        var machinery = await _machineryFacade.GetMachineryByIdAsync(machineryId);
        return machinery?.Status ?? string.Empty;
    }

    public async Task<int?> GetAssignedPersonnelIdAsync(int machineryId)
    {
        var machinery = await _machineryFacade.GetMachineryByIdAsync(machineryId);
        return machinery?.PersonnelId;
    }

    public async Task<bool> IsMachineryActiveAsync(int machineryId)
    {
        var machinery = await _machineryFacade.GetMachineryByIdAsync(machineryId);
        return machinery?.IsActive() ?? false;
    }

    public async Task<int> GetActiveMachineryCountAsync(int projectId)
    {
        var activeMachinery = await _machineryFacade.GetActiveMachineryByProjectAsync(projectId);
        return activeMachinery.Count;
    }
}