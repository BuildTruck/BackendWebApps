namespace BuildTruckBack.Notifications.Interfaces.REST.Resources;

public record NotificationSummaryResource(
    int UnreadCount,
    Dictionary<string, int> ByContext,
    DateTime LastUpdated
);