using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Repositories;

namespace BuildTruckBack.Configurations.Application.Internal.QueryServices;

public class ConfigurationQueryService(IConfigurationRepository configurationRepository)
{
    public async Task<Configuration?> GetConfigurationByUserIdAsync(int userId)
    {
        return await configurationRepository.FindByUserIdAsync(userId);
    }
}