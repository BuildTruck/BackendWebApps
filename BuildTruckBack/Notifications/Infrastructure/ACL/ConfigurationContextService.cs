using BuildTruckBack.Configurations.Application.Internal.OutboundServices;
using BuildTruckBack.Notifications.Application.ACL.Services;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class ConfigurationContextService : IConfigurationContextService
{
    private readonly IConfigurationFacade _configurationFacade;

    public ConfigurationContextService(IConfigurationFacade configurationFacade)
    {
        _configurationFacade = configurationFacade;
    }

    public async Task<bool> IsEmailNotificationGloballyEnabledAsync(int userId)
    {
        try
        {
            var config = await _configurationFacade.GetConfigurationSettingsByUserIdAsync(userId);
            return config?.EmailNotificationsEnabled ?? true;
        }
        catch
        {
            return true;
        }
    }

    public async Task<bool> IsDigestEmailEnabledAsync(int userId)
    {
        try
        {
            var config = await _configurationFacade.GetConfigurationSettingsByUserIdAsync(userId);
            return config?.NotificationsEnabled ?? true;
        }
        catch
        {
            return true;
        }
    }

    public Task<TimeSpan> GetDigestTimeAsync(int userId) =>
        Task.FromResult(new TimeSpan(8, 0, 0));

    public Task<string> GetUserTimezoneAsync(int userId) =>
        Task.FromResult("America/Lima");

    public Task<string> GetUserLanguageAsync(int userId) =>
        Task.FromResult("es");
}
