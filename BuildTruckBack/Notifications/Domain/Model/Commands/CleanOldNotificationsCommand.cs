namespace BuildTruckBack.Notifications.Domain.Model.Commands;

public record CleanOldNotificationsCommand(
    int UserId,
    int? DaysOld = 30
);