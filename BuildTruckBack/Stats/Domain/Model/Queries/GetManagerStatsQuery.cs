namespace BuildTruckBack.Stats.Domain.Model.Queries;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// Query to get manager statistics
/// </summary>
public record GetManagerStatsQuery(
    int ManagerId,
    StatsPeriod? Period = null,
    bool IncludeAlerts = true,
    bool IncludeRecommendations = true)
{
    /// <summary>
    /// Validate the query parameters
    /// </summary>
    public bool IsValid()
    {
        return ManagerId > 0;
    }

    /// <summary>
    /// Get validation errors
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (ManagerId <= 0)
            errors.Add("ManagerId must be greater than 0");

        return errors;
    }

    /// <summary>
    /// Create query for current month stats
    /// </summary>
    public static GetManagerStatsQuery ForCurrentMonth(int managerId)
    {
        return new GetManagerStatsQuery(
            managerId,
            StatsPeriod.CurrentMonth(),
            true,
            true
        );
    }

    /// <summary>
    /// Create query for specific period
    /// </summary>
    public static GetManagerStatsQuery ForPeriod(int managerId, StatsPeriod period)
    {
        return new GetManagerStatsQuery(
            managerId,
            period,
            true,
            true
        );
    }

    /// <summary>
    /// Create query for custom date range
    /// </summary>
    public static GetManagerStatsQuery ForDateRange(int managerId, DateTime startDate, DateTime endDate)
    {
        return new GetManagerStatsQuery(
            managerId,
            StatsPeriod.Custom(startDate, endDate),
            true,
            true
        );
    }

    /// <summary>
    /// Create minimal query (no alerts or recommendations)
    /// </summary>
    public static GetManagerStatsQuery Minimal(int managerId, StatsPeriod? period = null)
    {
        return new GetManagerStatsQuery(
            managerId,
            period ?? StatsPeriod.CurrentMonth(),
            false,
            false
        );
    }
}