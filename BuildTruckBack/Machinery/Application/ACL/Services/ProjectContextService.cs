using BuildTruckBack.Machinery.Application.ACL.Services;
using BuildTruckBack.Projects.Application.Internal.OutboundServices;

namespace BuildTruckBack.Machinery.Application.ACL.Services;

public class ProjectContextService(IProjectFacade projectFacade) : IProjectContextService
{
    public Task<ProjectInfo?> GetProjectByIdAsync(int projectId) =>
        projectFacade.GetProjectByIdAsync(projectId);

    public Task<bool> ExistsAsync(int projectId) =>
        projectFacade.ExistsByIdAsync(projectId);
}

