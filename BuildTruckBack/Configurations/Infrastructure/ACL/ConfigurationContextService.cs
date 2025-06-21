using BuildTruckBack.Configurations.Application.ACL.Services;
using BuildTruckBack.Configurations.Application.Internal.OutboundServices;

namespace BuildTruckBack.Configurations.Infrastructure.ACL;

public class ConfigurationContextService(IConfigurationFacade configurationFacade) : IConfigurationContextService
{
    public async Task<ConfigurationInfo?> GetConfigurationByUserIdAsync(int userId)
    {
        return await configurationFacade.GetConfigurationByUserIdAsync(userId);
    }
}