using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Model.Commands;

public record CreateNotificationCommand(
    int UserId,
    NotificationType Type,
    NotificationContext Context,
    NotificationPriority Priority,
    string Title,
    string Message,
    UserRole TargetRole,
    NotificationScope Scope,
    string? ActionUrl = null,
    string? ActionText = null,
    string? IconClass = null,
    int? RelatedProjectId = null,
    int? RelatedEntityId = null,
    string? RelatedEntityType = null,
    Dictionary<string, object>? Metadata = null
);