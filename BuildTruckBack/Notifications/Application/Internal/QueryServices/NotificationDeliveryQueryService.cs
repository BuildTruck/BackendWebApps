using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Queries;
using BuildTruckBack.Notifications.Domain.Repositories;

namespace BuildTruckBack.Notifications.Application.Internal.QueryServices;

public class NotificationDeliveryQueryService
{
    private readonly INotificationDeliveryRepository _deliveryRepository;

    public NotificationDeliveryQueryService(INotificationDeliveryRepository deliveryRepository)
    {
        _deliveryRepository = deliveryRepository;
    }

    public async Task<IEnumerable<NotificationDelivery>> Handle(GetPendingDeliveriesQuery query)
    {
        if (query.Channel != null)
        {
            return await _deliveryRepository.FindPendingDeliveriesAsync(query.Channel);
        }

        return await _deliveryRepository.FindFailedDeliveriesAsync(query.MaxAttempts ?? 3);
    }
}