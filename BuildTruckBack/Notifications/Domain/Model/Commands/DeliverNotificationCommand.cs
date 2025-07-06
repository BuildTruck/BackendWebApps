using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Model.Commands;

public record DeliverNotificationCommand(
    int NotificationId,
    NotificationChannel Channel
);