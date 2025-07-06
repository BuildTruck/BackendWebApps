namespace BuildTruckBack.Notifications.Domain.Model.ValueObjects;

public record NotificationScope
{
    public static readonly NotificationScope System = new("SYSTEM");
    public static readonly NotificationScope Project = new("PROJECT");
    public static readonly NotificationScope User = new("USER");

    public string Value { get; init; }

    private NotificationScope(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsSystem() => Value == System.Value;
    public bool IsProject() => Value == Project.Value;
    public bool IsUser() => Value == User.Value;

    public static NotificationScope FromString(string value)
    {
        return GetAllScopes().FirstOrDefault(s => s.Value == value) 
               ?? throw new ArgumentException($"Invalid notification scope: {value}");
    }

    public static IEnumerable<NotificationScope> GetAllScopes()
    {
        return new[] { System, Project, User };
    }
    
    private NotificationScope() { Value = string.Empty; }
}