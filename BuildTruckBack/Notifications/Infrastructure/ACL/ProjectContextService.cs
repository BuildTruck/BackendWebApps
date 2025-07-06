using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Projects.Application.Internal.OutboundServices;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class ProjectContextService : IProjectContextService
{
    private readonly IProjectFacade _projectFacade;

    public ProjectContextService(IProjectFacade projectFacade)
    {
        _projectFacade = projectFacade;
    }

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        return await _projectFacade.ExistsByIdAsync(projectId);
    }

    public async Task<string> GetProjectNameAsync(int projectId)
    {
        var project = await _projectFacade.GetProjectByIdAsync(projectId);
        return project?.Name ?? string.Empty;
    }

    public async Task<int> GetProjectManagerIdAsync(int projectId)
    {
        var project = await _projectFacade.GetProjectByIdAsync(projectId);
        return project?.ManagerId ?? 0;
    }

    public async Task<int?> GetProjectSupervisorIdAsync(int projectId)
    {
        var project = await _projectFacade.GetProjectByIdAsync(projectId);
        return project?.SupervisorId;
    }

    public async Task<IEnumerable<int>> GetProjectsByManagerAsync(int managerId)
    {
        var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);
        return projects.Select(p => p.Id);
    }

    public async Task<int?> GetProjectBySupervisorAsync(int supervisorId)
    {
        var projects = await _projectFacade.GetProjectsBySupervisorAsync(supervisorId);
        return projects.FirstOrDefault()?.Id;
    }

    public async Task<IEnumerable<int>> GetAllActiveProjectsAsync()
    {
        var projects = await _projectFacade.GetProjectsByStateAsync("Activo");
        return projects.Select(p => p.Id);
    }

    public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
    {
        return await _projectFacade.UserHasAccessToProjectAsync(userId, projectId);
    }
}