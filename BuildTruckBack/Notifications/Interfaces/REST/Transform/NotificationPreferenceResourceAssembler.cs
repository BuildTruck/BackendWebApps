using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Commands;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Interfaces.REST.Resources;

namespace BuildTruckBack.Notifications.Interfaces.REST.Transform;

public static class NotificationPreferenceResourceAssembler
{
    public static NotificationPreferenceResource ToResourceFromEntity(NotificationPreference preference)
    {
        return new NotificationPreferenceResource(
            preference.Id,
            preference.UserId,
            preference.Context.Value,
            preference.InAppEnabled,
            preference.EmailEnabled,
            preference.MinimumPriority.Value,
            preference.CreatedDate?.DateTime ?? DateTime.MinValue,
            preference.UpdatedDate?.DateTime
        );
    }

    public static IEnumerable<NotificationPreferenceResource> ToResourceFromEntity(IEnumerable<NotificationPreference> preferences)
    {
        return preferences.Select(ToResourceFromEntity);
    }

    public static UpdatePreferenceCommand ToCommandFromResource(int userId, UpdatePreferenceResource resource)
    {
        return new UpdatePreferenceCommand(
            userId,
            NotificationContext.FromString(resource.Context),
            resource.InAppEnabled,
            resource.EmailEnabled,
            NotificationPriority.FromString(resource.MinimumPriority)
        );
    }
}