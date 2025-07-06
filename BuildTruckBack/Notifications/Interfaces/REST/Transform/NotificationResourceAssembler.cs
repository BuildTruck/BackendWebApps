using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Interfaces.REST.Resources;

namespace BuildTruckBack.Notifications.Interfaces.REST.Transform;

public static class NotificationResourceAssembler
{
    public static NotificationResource ToResourceFromEntity(Notification notification)
    {
        return new NotificationResource(
            notification.Id,
            notification.UserId,
            notification.Type.Value,
            notification.Context.Value,
            notification.Priority.Value,
            notification.Content.Title,
            notification.Content.Message,
            notification.Content.ActionUrl,
            notification.Content.ActionText,
            notification.Content.IconClass,
            notification.Status.Value,
            notification.Scope.Value,
            notification.TargetRole.Value,
            notification.RelatedProjectId,
            notification.RelatedEntityId,
            notification.RelatedEntityType,
            notification.IsRead,
            notification.ReadAt,
            notification.CreatedDate?.DateTime ?? DateTime.MinValue,
            notification.UpdatedDate?.DateTime
        );
    }

    public static IEnumerable<NotificationResource> ToResourceFromEntity(IEnumerable<Notification> notifications)
    {
        return notifications.Select(ToResourceFromEntity);
    }

    public static NotificationSummaryResource ToSummaryResource(Dictionary<string, object> summary)
    {
        return new NotificationSummaryResource(
            (int)summary["unreadCount"],
            (Dictionary<string, int>)summary["byContext"],
            (DateTime)summary["lastUpdated"]
        );
    }
}