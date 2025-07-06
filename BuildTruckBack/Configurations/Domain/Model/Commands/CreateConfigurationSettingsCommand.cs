using BuildTruckBack.Configurations.Domain.Model.ValueObjects;

namespace BuildTruckBack.Configurations.Domain.Model.Commands;

/// <summary>
/// Command to create a configuration settings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public record CreateConfigurationSettingsCommand(
    int UserIds,
    Theme Themes,
    Plan Plans,
    bool NotificationsEnables,
    bool EmailNotification)
{
}