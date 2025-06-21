using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Configurations.Domain.Repositories;

public interface IConfigurationRepository : IBaseRepository<Configuration>
{
    Task<Configuration?> FindByUserIdAsync(int userId);
}