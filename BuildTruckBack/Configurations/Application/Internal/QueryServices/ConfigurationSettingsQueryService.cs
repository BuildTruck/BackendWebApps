using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Repositories;
using BuildTruckBack.Configurations.Domain.Services;

namespace BuildTruckBack.Configurations.Application.Internal.QueryServices;

/// <summary>
/// Query service for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsQueryService : IConfigurationSettingsQueryService
{
    private readonly IConfigurationSettingsRepository _configurationSettingsRepository;

    public ConfigurationSettingsQueryService(IConfigurationSettingsRepository configurationSettingsRepository)
    {
        _configurationSettingsRepository = configurationSettingsRepository;
    }

    public async Task<ConfigurationSettings?> GetByUserIdAsync(int userId)
    {
        return await _configurationSettingsRepository.FindByUserIdAsync(userId);
    }
}