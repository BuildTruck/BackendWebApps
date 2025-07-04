using BuildTruckBack.Configurations.Domain.Model.Aggregates;

namespace BuildTruckBack.Configurations.Domain.Services;

/// <summary>
/// Query service interface for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public interface IConfigurationSettingsQueryService
{
    Task<ConfigurationSettings?> GetByUserIdAsync(int userId);
}