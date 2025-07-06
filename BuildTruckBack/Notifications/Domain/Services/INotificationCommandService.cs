using BuildTruckBack.Notifications.Domain.Model.Commands;

namespace BuildTruckBack.Notifications.Domain.Services;

public interface INotificationCommandService
{
    Task<int> Handle(CreateNotificationCommand command);
    Task Handle(MarkAsReadCommand command);
    Task Handle(BulkMarkAsReadCommand command);
    Task Handle(DeliverNotificationCommand command);
    Task Handle(CleanOldNotificationsCommand command);
}