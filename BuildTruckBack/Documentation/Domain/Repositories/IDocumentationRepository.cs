using BuildTruckBack.Documentation.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Documentation.Domain.Repositories;

public interface IDocumentationRepository : IBaseRepository<Documentation.Domain.Model.Aggregates.Documentation>
{
    // Queries específicas del dominio Documentation
    Task<IEnumerable<Documentation.Domain.Model.Aggregates.Documentation>> FindByProjectIdAsync(int projectId);
    
    Task<Documentation.Domain.Model.Aggregates.Documentation?> FindByIdAndProjectAsync(int id, int projectId);
    
    Task<IEnumerable<Documentation.Domain.Model.Aggregates.Documentation>> FindByProjectIdOrderedByDateAsync(int projectId);
    
    Task<IEnumerable<Documentation.Domain.Model.Aggregates.Documentation>> FindRecentByProjectIdAsync(int projectId, int days = 7);
    
    Task<bool> ExistsByTitleAndProjectAsync(string title, int projectId, int? excludeId = null);
}