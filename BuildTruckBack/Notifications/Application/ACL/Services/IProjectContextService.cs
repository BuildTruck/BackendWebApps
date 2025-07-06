namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IProjectContextService
{
    Task<bool> ProjectExistsAsync(int projectId);
    Task<string> GetProjectNameAsync(int projectId);
    Task<int> GetProjectManagerIdAsync(int projectId);
    Task<int?> GetProjectSupervisorIdAsync(int projectId);
    Task<IEnumerable<int>> GetProjectsByManagerAsync(int managerId);
    Task<int?> GetProjectBySupervisorAsync(int supervisorId);
    Task<IEnumerable<int>> GetAllActiveProjectsAsync();
    Task<bool> UserHasAccessToProjectAsync(int userId, int projectId);
}