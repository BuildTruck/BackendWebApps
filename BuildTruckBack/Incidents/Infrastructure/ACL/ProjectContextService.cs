// Incidents/Infrastructure/ACL/ProjectContextService.cs
    using System.Threading.Tasks;
    using BuildTruckBack.Incidents.Application.ACL.Services;
    using BuildTruckBack.Incidents.Domain.Repositories;
    
    public class ProjectContextService : IProjectContextService
    {
        private readonly IProjectRepository _projectRepository;
    
        public ProjectContextService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }
    
        public async Task<bool> ExistsAsync(int projectId)
        {
            return await _projectRepository.FindByIdAsync(projectId) != null;
        }
    }