using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Queries;

namespace BuildTruckBack.Notifications.Domain.Services;

public interface INotificationPreferenceQueryService
{
    Task<IEnumerable<NotificationPreference>> Handle(GetUserPreferencesQuery query);
}