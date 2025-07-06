using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Queries;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Notifications.Domain.Services;

namespace BuildTruckBack.Notifications.Application.Internal.QueryServices;

public class NotificationPreferenceQueryService : INotificationPreferenceQueryService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public NotificationPreferenceQueryService(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    public async Task<IEnumerable<NotificationPreference>> Handle(GetUserPreferencesQuery query)
    {
        return await _preferenceRepository.FindByUserIdAsync(query.UserId);
    }
}