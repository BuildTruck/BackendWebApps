namespace BuildTruckBack.Stats.Domain.Model.Commands;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// Command to calculate comprehensive statistics for a manager
/// </summary>
public record CalculateManagerStatsCommand(
    int ManagerId,
    StatsPeriod Period,
    bool ForceRecalculation = false,
    bool SaveHistory = false,
    string? HistoryNotes = null)
{
    /// <summary>
    /// Validate the command parameters
    /// </summary>
    public bool IsValid()
    {
        return ManagerId > 0 && Period != null;
    }

    /// <summary>
    /// Get validation errors
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (ManagerId <= 0)
            errors.Add("ManagerId must be greater than 0");

        if (Period == null)
            errors.Add("Period is required");

        return errors;
    }

    /// <summary>
    /// Create command for current month
    /// </summary>
    public static CalculateManagerStatsCommand ForCurrentMonth(int managerId, bool saveHistory = false)
    {
        return new CalculateManagerStatsCommand(
            managerId,
            StatsPeriod.CurrentMonth(),
            false,
            saveHistory
        );
    }

    /// <summary>
    /// Create command for custom period
    /// </summary>
    public static CalculateManagerStatsCommand ForCustomPeriod(
        int managerId,
        DateTime startDate,
        DateTime endDate,
        bool saveHistory = false,
        string? notes = null)
    {
        return new CalculateManagerStatsCommand(
            managerId,
            StatsPeriod.Custom(startDate, endDate),
            false,
            saveHistory,
            notes
        );
    }

    /// <summary>
    /// Create command with forced recalculation
    /// </summary>
    public static CalculateManagerStatsCommand WithForceRecalculation(
        int managerId,
        StatsPeriod period,
        bool saveHistory = true)
    {
        return new CalculateManagerStatsCommand(
            managerId,
            period,
            true,
            saveHistory,
            "Recálculo forzado"
        );
    }
}