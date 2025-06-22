using BuildTruckBack.Projects.Domain.Model.Aggregates;

namespace BuildTruckBack.Incidents.Domain.Repositories;

public interface IProjectRepository
{
    Task<bool> ExistsAsync(int projectId);
    Task<Project> FindByIdAsync(int projectId);
}