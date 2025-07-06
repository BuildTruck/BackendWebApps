using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Model.Commands;
using BuildTruckBack.Configurations.Domain.Services;
using BuildTruckBack.Configurations.Interfaces.ACL;

namespace BuildTruckBack.Configurations.Application.ACL;

/// <summary>
/// Facade for ConfigurationSettings operations
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsFacade : IConfigurationSettingsFacade
{
    private readonly IConfigurationSettingsCommandService _commandService;
    private readonly IConfigurationSettingsQueryService _queryService;

    public ConfigurationSettingsFacade(IConfigurationSettingsCommandService commandService, IConfigurationSettingsQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    public async Task<ConfigurationSettings> CreateConfigurationSettingsAsync(CreateConfigurationSettingsCommand command)
    {
        return await _commandService.Handle(command);
    }

    public async Task<ConfigurationSettings> UpdateConfigurationSettingsAsync(UpdateConfigurationSettingsCommand command)
    {
        return await _commandService.Handle(command);
    }

    public async Task<ConfigurationSettings?> GetConfigurationSettingsByUserIdAsync(int userId)
    {
        return await _queryService.GetByUserIdAsync(userId);
    }
}