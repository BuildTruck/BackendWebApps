using BuildTruckBack.Configurations.Application.Internal.OutboundServices;
using BuildTruckBack.Configurations.Domain.Model.Commands;
using BuildTruckBack.Configurations.Interfaces.REST.Resources;

namespace BuildTruckBack.Configurations.Interfaces.REST.Transform;

public static class ConfigurationResourceAssembler
{
    public static ConfigurationResource ToResourceFromInfo(ConfigurationInfo info)
    {
        return new ConfigurationResource
        {
            Id = info.Id,
            UserId = info.UserId,
            Theme = info.Theme,
            Plan = info.Plan,
            NotificationsEnable = info.NotificationsEnable.ToString().ToLower(),
            EmailNotifications = info.EmailNotifications.ToString().ToLower(),
            CreatedAt = info.CreatedAt,
            UpdatedAt = info.UpdatedAt
        };
    }

    public static CreateConfigurationCommand ToCreateCommandFromResource(UpdateConfigurationResource resource, int userId)
    {
        return new CreateConfigurationCommand
        {
            UserId = userId,
            Theme = resource.Theme,
            Plan = resource.Plan,
            NotificationsEnable = bool.Parse(resource.NotificationsEnable),
            EmailNotifications = bool.Parse(resource.EmailNotifications)
        };
    }

    public static UpdateConfigurationCommand ToUpdateCommandFromResource(UpdateConfigurationResource resource, int userId)
    {
        return new UpdateConfigurationCommand
        {
            UserId = userId,
            Theme = resource.Theme,
            Plan = resource.Plan,
            NotificationsEnable = bool.Parse(resource.NotificationsEnable),
            EmailNotifications = bool.Parse(resource.EmailNotifications)
        };
    }
}