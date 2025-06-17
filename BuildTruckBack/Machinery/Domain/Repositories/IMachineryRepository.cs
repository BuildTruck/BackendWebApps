using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Machinery.Domain.Model.Aggregates;

public interface IMachineryRepository: IBaseRepository<Machinery>
{
    Task<IEnumerable<Machinery>> FindByProjectIdAsync(string projectId);
    Task<Machinery?> FindByLicensePlateAsync(string licensePlate);
}