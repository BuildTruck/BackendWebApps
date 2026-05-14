using BuildTruckBack.Projects.Application.Internal.OutboundServices;

namespace BuildTruckBack.Incidents.Domain.Repositories;

public interface IProjectRepository
{
    Task<bool> ExistsAsync(int projectId);
    Task<ProjectInfo?> FindByIdAsync(int projectId);
}