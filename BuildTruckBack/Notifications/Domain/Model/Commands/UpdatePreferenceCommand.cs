using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Model.Commands;

public record UpdatePreferenceCommand(
    int UserId,
    NotificationContext Context,
    bool InAppEnabled,
    bool EmailEnabled,
    NotificationPriority MinimumPriority
);