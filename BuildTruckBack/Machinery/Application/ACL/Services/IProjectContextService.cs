using BuildTruckBack.Projects.Domain.Model.Aggregates;

namespace BuildTruckBack.Machinery.Application.ACL.Services;

public interface IProjectContextService
{
    Task<Project?> GetProjectByIdAsync(string projectId);
}