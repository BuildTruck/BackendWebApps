namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IPersonnelContextService
{
    Task<bool> PersonnelExistsAsync(int personnelId);
    Task<string> GetPersonnelNameAsync(int personnelId);
    Task<int> GetPersonnelProjectIdAsync(int personnelId);
    Task<bool> PersonnelBelongsToProjectAsync(int personnelId, int projectId);
    Task<int> GetActivePersonnelCountAsync(int projectId);
    Task<decimal> GetAttendanceRateAsync(int projectId);
}