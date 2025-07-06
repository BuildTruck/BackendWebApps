namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IMachineryContextService
{
    Task<bool> MachineryExistsAsync(int machineryId);
    Task<string> GetMachineryNameAsync(int machineryId);
    Task<int> GetMachineryProjectIdAsync(int machineryId);
    Task<bool> MachineryBelongsToProjectAsync(int machineryId, int projectId);
    Task<string> GetMachineryStatusAsync(int machineryId);
    Task<int?> GetAssignedPersonnelIdAsync(int machineryId);
    Task<bool> IsMachineryActiveAsync(int machineryId);
    Task<int> GetActiveMachineryCountAsync(int projectId);
}