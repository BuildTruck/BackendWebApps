namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing a time period
/// </summary>
public record PeriodResource(
    DateTime StartDate,
    DateTime EndDate,
    string PeriodType,
    string DisplayName,
    int TotalDays,
    bool IsCurrentPeriod
);