namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing personnel metrics in history snapshot
/// </summary>
public record PersonnelHistoryMetricsResource(
    int TotalPersonnel,
    int ActivePersonnel,
    decimal PersonnelActiveRate,
    decimal PersonnelEfficiencyScore
);