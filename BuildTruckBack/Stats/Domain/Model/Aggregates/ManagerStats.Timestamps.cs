namespace BuildTruckBack.Stats.Domain.Model.Aggregates;

using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

/// <summary>
/// ManagerStats partial class for timestamp management
/// </summary>
public partial class ManagerStats : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }

    /// <summary>
    /// Get current Peru time
    /// </summary>
    public static DateTime GetCurrentPeruTime()
    {
        return DateTime.UtcNow.AddHours(-5);
    }

    /// <summary>
    /// Update timestamps when recalculating
    /// </summary>
    public void UpdateTimestamps()
    {
        UpdatedDate = DateTimeOffset.UtcNow.AddHours(-5);
        CalculatedAt = GetCurrentPeruTime();
    }

    /// <summary>
    /// Check if stats are recent (calculated within last hour)
    /// </summary>
    public bool IsRecent()
    {
        var hourAgo = GetCurrentPeruTime().AddHours(-1);
        return CalculatedAt >= hourAgo;
    }

    /// <summary>
    /// Check if stats are outdated (older than 24 hours)
    /// </summary>
    public bool IsOutdated()
    {
        var dayAgo = GetCurrentPeruTime().AddDays(-1);
        return CalculatedAt < dayAgo;
    }

    /// <summary>
    /// Get age of the statistics in hours
    /// </summary>
    public double GetAgeInHours()
    {
        return (GetCurrentPeruTime() - CalculatedAt).TotalHours;
    }

    /// <summary>
    /// Get formatted calculation time
    /// </summary>
    public string GetCalculatedTimeFormatted()
    {
        return CalculatedAt.ToString("dd/MM/yyyy HH:mm:ss");
    }

    /// <summary>
    /// Get relative time since calculation
    /// </summary>
    public string GetRelativeCalculationTime()
    {
        var timeSpan = GetCurrentPeruTime() - CalculatedAt;
        
        return timeSpan.TotalMinutes switch
        {
            < 1 => "Hace menos de un minuto",
            < 60 => $"Hace {(int)timeSpan.TotalMinutes} minuto(s)",
            < 1440 => $"Hace {(int)timeSpan.TotalHours} hora(s)",
            _ => $"Hace {(int)timeSpan.TotalDays} día(s)"
        };
    }
}