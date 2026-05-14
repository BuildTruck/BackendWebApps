using BuildTruckBack.Projects.Application.Internal.OutboundServices;

namespace BuildTruckBack.Machinery.Application.ACL.Services;

public interface IProjectContextService
{
    Task<ProjectInfo?> GetProjectByIdAsync(int projectId);
    Task<bool> ExistsAsync(int projectId);
}