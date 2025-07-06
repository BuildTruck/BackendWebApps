using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Configurations.Interfaces.ACL;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class ConfigurationContextService : IConfigurationContextService
{
    private readonly IConfigurationSettingsFacade _configurationFacade;
    public ConfigurationContextService(IConfigurationSettingsFacade configurationFacade)  // ← AGREGAR
    {
        _configurationFacade = configurationFacade;  // ← AGREGAR
    }
    public async Task<bool> IsEmailNotificationGloballyEnabledAsync(int userId)
    {
        try
        {
            var config = await _configurationFacade.GetConfigurationSettingsByUserIdAsync(userId);
            return config?.EmailNotifications ?? true; // Default true si no tiene config
        }
        catch
        {
            return true; // Default en caso de error
        }
    }

    public async Task<bool> IsDigestEmailEnabledAsync(int userId)
    {
        try
        {
            var config = await _configurationFacade.GetConfigurationSettingsByUserIdAsync(userId);
            return config?.NotificationsEnable ?? true; // Default true
        }
        catch
        {
            return true; // Default en caso de error
        }
    }

    public async Task<TimeSpan> GetDigestTimeAsync(int userId)
    {
        await Task.CompletedTask;
        return new TimeSpan(8, 0, 0);
    }

    public async Task<string> GetUserTimezoneAsync(int userId)
    {
        await Task.CompletedTask;
        return "America/Lima";
    }

    public async Task<string> GetUserLanguageAsync(int userId)
    {
        await Task.CompletedTask;
        return "es";
    }
}