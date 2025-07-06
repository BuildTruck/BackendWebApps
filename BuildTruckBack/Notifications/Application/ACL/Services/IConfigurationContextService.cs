namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IConfigurationContextService
{
    Task<bool> IsEmailNotificationGloballyEnabledAsync(int userId);
    Task<bool> IsDigestEmailEnabledAsync(int userId);
    Task<TimeSpan> GetDigestTimeAsync(int userId);
    Task<string> GetUserTimezoneAsync(int userId);
    Task<string> GetUserLanguageAsync(int userId);
}