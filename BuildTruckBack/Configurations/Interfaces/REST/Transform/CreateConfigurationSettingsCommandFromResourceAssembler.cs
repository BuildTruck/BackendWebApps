using BuildTruckBack.Configurations.Domain.Model.Commands;
using BuildTruckBack.Configurations.Domain.Model.ValueObjects;
using BuildTruckBack.Configurations.Interfaces.REST.Resources;

namespace BuildTruckBack.Configurations.Interfaces.REST.Transform;

/// <summary>
/// Assembler for CreateConfigurationSettingsCommand from CreateConfigurationSettingsResource
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public static class CreateConfigurationSettingsCommandFromResourceAssembler
{
    public static CreateConfigurationSettingsCommand ToCommandFromResource(CreateConfigurationSettingsResource resource)
    {
        bool ParseToBoolean(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.Equals("true", StringComparison.OrdinalIgnoreCase) && !value.Equals("false", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid boolean value: {value}. Must be 'true' or 'false'.");
            return value.ToLower() == "true";
        }
        
        if (!Enum.TryParse<Theme>(resource.Theme, out var theme))
            throw new ArgumentException($"Invalid theme: {resource.Theme}");
        if (!Enum.TryParse<Plan>(resource.Plan, out var plan))
            throw new ArgumentException($"Invalid plan: {resource.Plan}");


        return new CreateConfigurationSettingsCommand(
            UserIds: resource.UserId, 
            theme,
            plan, 
            NotificationsEnables: ParseToBoolean(resource.NotificationsEnable),
            EmailNotification: ParseToBoolean(resource.EmailNotifications),
            TutorialsCompleted: new TutorialProgress(resource.TutorialsCompleted));
    }
}