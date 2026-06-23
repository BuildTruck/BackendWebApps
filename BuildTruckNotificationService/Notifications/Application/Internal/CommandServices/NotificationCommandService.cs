using BuildTruckNotificationService.Notifications.Application.ACL;
using BuildTruckNotificationService.Notifications.Domain.Model.Aggregates;
using BuildTruckNotificationService.Notifications.Domain.Model.Commands;
using BuildTruckNotificationService.Notifications.Domain.Model.ValueObjects;
using BuildTruckNotificationService.Notifications.Domain.Repositories;
using BuildTruckNotificationService.Notifications.Domain.Services;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckNotificationService.Notifications.Application.Internal.CommandServices;

public class NotificationCommandService : INotificationCommandService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDeliveryService _deliveryService;
    private readonly IWebSocketService _webSocketService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationCommandService(
        INotificationRepository notificationRepository,
        INotificationDeliveryService deliveryService,
        IWebSocketService webSocketService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _deliveryService = deliveryService;
        _webSocketService = webSocketService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateNotificationCommand command)
    {
        var content = new NotificationContent(command.Title, command.Message, command.ActionUrl, command.ActionText);
        var notification = new Notification(
            command.UserId, command.Type, command.Context, command.Priority, content,
            command.TargetRole, command.Scope, command.RelatedProjectId, command.RelatedEntityId,
            command.RelatedEntityType, command.MetadataJson);

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.CompleteAsync();

        try
        {
            await _webSocketService.SendToUserAsync(notification.UserId, notification);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando WebSocket: {ex.Message}");
        }

        await CreateDeliveriesAsync(notification);
        return notification.Id;
    }

    public async Task Handle(MarkAsReadCommand command)
    {
        var notification = await _notificationRepository.FindByIdAsync(command.NotificationId);
        if (notification == null || notification.UserId != command.UserId)
            throw new InvalidOperationException("Notification not found or access denied");

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(BulkMarkAsReadCommand command)
    {
        await _notificationRepository.BulkMarkAsReadAsync(command.NotificationIds, command.UserId);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(DeliverNotificationCommand command)
    {
        var notification = await _notificationRepository.FindByIdAsync(command.NotificationId);
        if (notification == null) throw new InvalidOperationException("Notification not found");
        await _deliveryService.DeliverAsync(notification, command.Channel);
    }

    public async Task Handle(CleanOldNotificationsCommand command)
    {
        await _notificationRepository.DeleteOldNotificationsAsync(command.UserId, command.DaysOld);
        await _unitOfWork.CompleteAsync();
    }

    private async Task CreateDeliveriesAsync(Notification notification)
    {
        try
        {
            await _deliveryService.DeliverAsync(notification, NotificationChannel.InApp);
            await _deliveryService.DeliverAsync(notification, NotificationChannel.WebSocket);
            if (notification.IsCritical())
                await _deliveryService.DeliverAsync(notification, NotificationChannel.Email);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating deliveries: {ex.Message}");
        }
    }
}
