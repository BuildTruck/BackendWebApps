namespace BuildTruckBack.Stats.Domain.Model.Commands;

/// <summary>
/// Command to save a stats snapshot to history
/// </summary>
public record SaveStatsHistoryCommand(
    int ManagerStatsId,
    string? Notes = null,
    bool IsManualSnapshot = false,
    bool OverwriteExisting = false)
{
    /// <summary>
    /// Validate the command parameters
    /// </summary>
    public bool IsValid()
    {
        return ManagerStatsId > 0;
    }

    /// <summary>
    /// Get validation errors
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (ManagerStatsId <= 0)
            errors.Add("ManagerStatsId must be greater than 0");

        return errors;
    }

    /// <summary>
    /// Create command for automatic snapshot
    /// </summary>
    public static SaveStatsHistoryCommand Automatic(int managerStatsId, string? notes = null)
    {
        return new SaveStatsHistoryCommand(
            managerStatsId,
            notes ?? "Snapshot automático",
            false,
            false
        );
    }

    /// <summary>
    /// Create command for manual snapshot
    /// </summary>
    public static SaveStatsHistoryCommand Manual(int managerStatsId, string notes)
    {
        return new SaveStatsHistoryCommand(
            managerStatsId,
            notes,
            true,
            false
        );
    }

    /// <summary>
    /// Create command that overwrites existing snapshot for the same period
    /// </summary>
    public static SaveStatsHistoryCommand WithOverwrite(int managerStatsId, string? notes = null)
    {
        return new SaveStatsHistoryCommand(
            managerStatsId,
            notes,
            false,
            true
        );
    }
}