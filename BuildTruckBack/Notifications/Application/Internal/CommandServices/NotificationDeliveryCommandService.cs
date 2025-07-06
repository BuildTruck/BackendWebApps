using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Notifications.Domain.Services;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Notifications.Application.Internal.CommandServices;

public class NotificationDeliveryCommandService : INotificationDeliveryService
{
    private readonly INotificationDeliveryRepository _deliveryRepository;
    private readonly ISharedEmailService _emailService;
    private readonly IWebSocketService _webSocketService;
    private readonly IUserContextService _userContextService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationDeliveryCommandService(
        INotificationDeliveryRepository deliveryRepository,
        ISharedEmailService emailService,
        IWebSocketService webSocketService,
        IUserContextService userContextService,
        IUnitOfWork unitOfWork)
    {
        _deliveryRepository = deliveryRepository;
        _emailService = emailService;
        _webSocketService = webSocketService;
        _userContextService = userContextService;
        _unitOfWork = unitOfWork;
    }

    public async Task DeliverAsync(Notification notification, NotificationChannel channel)
    {
        var delivery = new NotificationDelivery(notification.Id, channel);

        try
        {
            switch (channel.Value)
            {
                case "WEBSOCKET":
                    await _webSocketService.SendToUserAsync(notification.UserId, notification);
                    break;
                
                case "EMAIL":
                    var userEmail = await _userContextService.GetUserEmailAsync(notification.UserId);
                    var userName = await _userContextService.GetUserNameAsync(notification.UserId);
                    await _emailService.SendNotificationEmailAsync(userEmail, userName, 
                        notification.Type, notification.Content.Title, notification.Content.Message, 
                        notification.Content.ActionUrl);
                    break;
                
                case "IN_APP":
                    break;
            }

            delivery.MarkAsSent();
        }
        catch (Exception ex)
        {
            delivery.MarkAsFailed(ex.Message);
        }

        await _deliveryRepository.AddAsync(delivery);
        await _unitOfWork.CompleteAsync();
    }

    public async Task RetryFailedDeliveriesAsync()
    {
        var retryableDeliveries = await _deliveryRepository.FindRetryableDeliveriesAsync();

        foreach (var delivery in retryableDeliveries)
        {
            if (delivery.ShouldRetryNow())
            {
                delivery.MarkAsRetrying();
                _deliveryRepository.Update(delivery);
            }
        }

        await _unitOfWork.CompleteAsync();
    }

    public async Task<bool> CanDeliverAsync(Notification notification, NotificationChannel channel)
    {
        var existingDelivery = await _deliveryRepository.FindByNotificationIdAndChannelAsync(notification.Id, channel);
        return existingDelivery == null || !existingDelivery.IsSuccessful();
    }

    public async Task<Dictionary<string, int>> GetDeliveryStatsAsync()
    {
        return await _deliveryRepository.GetDeliveryStatsAsync();
    }
}