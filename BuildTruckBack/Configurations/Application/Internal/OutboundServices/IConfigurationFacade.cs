namespace BuildTruckBack.Configurations.Application.Internal.OutboundServices;

public interface IConfigurationFacade
{
    Task<ConfigurationDto?> GetConfigurationSettingsByUserIdAsync(int userId);
}

public record ConfigurationDto(
    int Id,
    int UserId,
    string Theme,
    string Plan,
    string NotificationsEnables,
    string EmailNotification,
    string TutorialsCompleted)
{
    public bool NotificationsEnabled =>
        bool.TryParse(NotificationsEnables, out var enabled) && enabled;

    public bool EmailNotificationsEnabled =>
        bool.TryParse(EmailNotification, out var enabled) && enabled;
}
