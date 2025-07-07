using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Commands;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Notifications.Domain.Services;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Notifications.Application.Internal.CommandServices;

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
        var content = new NotificationContent(command.Title, command.Message, 
            command.ActionUrl, command.ActionText, command.IconClass);

        var notification = new Notification(
            command.UserId,
            command.Type,
            command.Context,
            command.Priority,
            content,
            command.TargetRole,
            command.Scope,
            command.RelatedProjectId,
            command.RelatedEntityId,
            command.RelatedEntityType
        );

        if (command.Metadata != null)
        {
            notification.SetMetadata(command.Metadata);
        }

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.CompleteAsync();
        try
        {
            await _webSocketService.SendToUserAsync(notification.UserId, notification);
            Console.WriteLine($"🔊 WebSocket enviado para notificación {notification.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error enviando WebSocket: {ex.Message}");
        }
        await CreateDeliveriesForNotification(notification);

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
        if (notification == null)
            throw new InvalidOperationException("Notification not found");

        var delivery = new NotificationDelivery(command.NotificationId, command.Channel);
        
        try
        {
            delivery.MarkAsSent();
        }
        catch (Exception ex)
        {
            delivery.MarkAsFailed(ex.Message);
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(CleanOldNotificationsCommand command)
    {
        await _notificationRepository.DeleteOldNotificationsAsync(command.UserId, command.DaysOld ?? 30);
        await _unitOfWork.CompleteAsync();
    }
    private async Task CreateDeliveriesForNotification(Notification notification)
    {
        try
        {
            // 1. IN_APP delivery (siempre)
            await _deliveryService.DeliverAsync(notification, NotificationChannel.InApp);

            // 2. WEBSOCKET delivery (siempre)  
            await _deliveryService.DeliverAsync(notification, NotificationChannel.WebSocket);

            // 3. EMAIL delivery (solo para críticas)
            if (notification.IsCritical())
            {
                await _deliveryService.DeliverAsync(notification, NotificationChannel.Email);
            }

            Console.WriteLine($"✅ Deliveries procesados para notificación ID: {notification.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating deliveries: {ex.Message}");
        }
    }
} 