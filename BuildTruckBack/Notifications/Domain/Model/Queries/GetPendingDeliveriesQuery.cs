using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Model.Queries;

public record GetPendingDeliveriesQuery(
    NotificationChannel? Channel = null,
    int? MaxAttempts = 3
);