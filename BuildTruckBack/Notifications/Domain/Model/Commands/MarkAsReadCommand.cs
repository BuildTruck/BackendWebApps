namespace BuildTruckBack.Notifications.Domain.Model.Commands;

public record MarkAsReadCommand(
    int NotificationId,
    int UserId
);