namespace BuildTruckBack.Notifications.Domain.Model.ValueObjects;

public record NotificationChannel
{
    public static readonly NotificationChannel InApp = new("IN_APP");
    public static readonly NotificationChannel Email = new("EMAIL");
    public static readonly NotificationChannel WebSocket = new("WEBSOCKET");
    public static readonly NotificationChannel Push = new("PUSH");

    public string Value { get; init; }

    private NotificationChannel(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsInApp() => Value == InApp.Value;
    public bool IsEmail() => Value == Email.Value;
    public bool IsWebSocket() => Value == WebSocket.Value;
    public bool IsPush() => Value == Push.Value;

    public static NotificationChannel FromString(string value)
    {
        return GetAllChannels().FirstOrDefault(c => c.Value == value) 
               ?? throw new ArgumentException($"Invalid notification channel: {value}");
    }

    public static IEnumerable<NotificationChannel> GetAllChannels()
    {
        return new[] { InApp, Email, WebSocket, Push };
    }

    public static IEnumerable<NotificationChannel> GetActiveChannels()
    {
        return new[] { InApp, Email, WebSocket };
    }
    
    private NotificationChannel() { Value = string.Empty; }
}