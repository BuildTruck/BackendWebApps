using BuildTruckBack.Configurations.Application.Internal.OutboundServices;

namespace BuildTruckBack.Configurations.Application.ACL.Services;

public interface IConfigurationContextService
{
    Task<ConfigurationInfo?> GetConfigurationByUserIdAsync(int userId);
}