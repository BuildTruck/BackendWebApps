using BuildTruckBack.Projects.Application.Internal.OutboundServices;

namespace BuildTruckBack.Machinery.Domain.Repositories;

public interface IProjectRepository
{
    Task<ProjectInfo?> FindByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
}