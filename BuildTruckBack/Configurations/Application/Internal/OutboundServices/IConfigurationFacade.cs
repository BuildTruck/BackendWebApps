using BuildTruckBack.Configurations.Application.Internal.OutboundServices;
using BuildTruckBack.Configurations.Domain.Model.Commands;

namespace BuildTruckBack.Configurations.Application.Internal.OutboundServices;

public interface IConfigurationFacade
{
    Task<ConfigurationInfo?> GetConfigurationByUserIdAsync(int userId);
    Task<ConfigurationInfo?> CreateConfigurationAsync(CreateConfigurationCommand command);
    Task<ConfigurationInfo?> UpdateConfigurationAsync(UpdateConfigurationCommand command);
}