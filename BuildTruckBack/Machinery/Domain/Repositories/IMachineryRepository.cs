using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Machinery.Domain.Model.Aggregates;

public interface IMachineryRepository: IBaseRepository<Domain.Model.Aggregates.Machinery>
{
    
    Task<IEnumerable<Domain.Model.Aggregates.Machinery>> FindByProjectIdAsync(int projectId);
    Task<Domain.Model.Aggregates.Machinery?> FindByLicensePlateAsync(string licensePlate, int projectId);
    Task UpdateAsync(Machinery machinery);
}