namespace BuildTruckBack.Stats.Domain.Model.Aggregates;

using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

/// <summary>
/// StatsHistory partial class for timestamp management
/// </summary>
public partial class StatsHistory : IEntityWithCreatedUpdatedDate
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
    /// Check if this snapshot is from today
    /// </summary>
    public bool IsFromToday()
    {
        var today = GetCurrentPeruTime().Date;
        return SnapshotDate.Date == today;
    }

    /// <summary>
    /// Check if this snapshot is from this week
    /// </summary>
    public bool IsFromThisWeek()
    {
        var now = GetCurrentPeruTime();
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        
        return SnapshotDate.Date >= startOfWeek && SnapshotDate.Date < endOfWeek;
    }

    /// <summary>
    /// Get the age of this snapshot in days
    /// </summary>
    public int GetAgeInDays()
    {
        return (GetCurrentPeruTime().Date - SnapshotDate.Date).Days;
    }

    /// <summary>
    /// Get formatted snapshot date
    /// </summary>
    public string GetSnapshotDateFormatted()
    {
        return SnapshotDate.ToString("dd/MM/yyyy HH:mm");
    }

    /// <summary>
    /// Get relative time since snapshot
    /// </summary>
    public string GetRelativeSnapshotTime()
    {
        var timeSpan = GetCurrentPeruTime() - SnapshotDate;
        
        return timeSpan.TotalDays switch
        {
            < 1 => "Hoy",
            < 2 => "Ayer",
            < 7 => $"Hace {(int)timeSpan.TotalDays} días",
            < 30 => $"Hace {(int)(timeSpan.TotalDays / 7)} semana(s)",
            < 365 => $"Hace {(int)(timeSpan.TotalDays / 30)} mes(es)",
            _ => $"Hace {(int)(timeSpan.TotalDays / 365)} año(s)"
        };
    }

    /// <summary>
    /// Check if snapshot should be archived (older than 1 year)
    /// </summary>
    public bool ShouldBeArchived()
    {
        return GetAgeInDays() > 365;
    }

    /// <summary>
    /// Update notes with timestamp
    /// </summary>
    public void UpdateNotesWithTimestamp(string newNotes)
    {
        var timestamp = GetCurrentPeruTime().ToString("dd/MM/yyyy HH:mm");
        Notes = string.IsNullOrEmpty(Notes) 
            ? $"[{timestamp}] {newNotes}"
            : $"{Notes}\n[{timestamp}] {newNotes}";
        
        UpdatedDate = DateTimeOffset.UtcNow.AddHours(-5);
    }
}