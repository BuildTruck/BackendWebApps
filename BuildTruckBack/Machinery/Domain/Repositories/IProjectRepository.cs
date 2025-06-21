using BuildTruckBack.Projects.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Machinery.Domain.Repositories;

public interface IProjectRepository: IBaseRepository<Project>
{
    Task<Project?> FindByIdAsync(string id);
}