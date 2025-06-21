using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Projects.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Application.ACL.Services;
namespace BuildTruckBack.Machinery.Application.ACL.Services;

public class ProjectContextService(IProjectRepository projectRepository) : IProjectContextService
{   
    public async Task<Project?> GetProjectByIdAsync(string projectId)
    {
        return await projectRepository.FindByIdAsync(projectId);
    }
}

