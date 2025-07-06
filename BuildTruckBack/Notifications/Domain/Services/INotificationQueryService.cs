using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Queries;

namespace BuildTruckBack.Notifications.Domain.Services;

public interface INotificationQueryService
{
    Task<IEnumerable<Notification>> Handle(GetNotificationsByUserQuery query);
    Task<Dictionary<string, object>> Handle(GetNotificationSummaryQuery query);
    Task<Notification?> Handle(GetNotificationByIdQuery query);
}