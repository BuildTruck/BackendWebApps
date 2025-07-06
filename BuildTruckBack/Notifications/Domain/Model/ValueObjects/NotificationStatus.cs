namespace BuildTruckBack.Notifications.Domain.Model.ValueObjects;

public record NotificationStatus
{
    public static readonly NotificationStatus Unread = new("UNREAD");
    public static readonly NotificationStatus Read = new("READ");
    public static readonly NotificationStatus Archived = new("ARCHIVED");

    public string Value { get; init; }

    private NotificationStatus(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsUnread() => Value == Unread.Value;
    public bool IsRead() => Value == Read.Value;
    public bool IsArchived() => Value == Archived.Value;

    public static NotificationStatus FromString(string value)
    {
        return GetAllStatuses().FirstOrDefault(s => s.Value == value) 
               ?? throw new ArgumentException($"Invalid notification status: {value}");
    }

    public static IEnumerable<NotificationStatus> GetAllStatuses()
    {
        return new[] { Unread, Read, Archived };
    }
    private NotificationStatus() { Value = string.Empty; }
}