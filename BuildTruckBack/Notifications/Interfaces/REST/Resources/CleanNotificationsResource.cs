using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Notifications.Interfaces.REST.Resources;

public record CleanNotificationsResource(
    [Range(1, 365)] int DaysOld = 30
);