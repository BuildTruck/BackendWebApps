namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing machinery metrics in history snapshot
/// </summary>
public record MachineryHistoryMetricsResource(
    int TotalMachinery,
    int ActiveMachinery,
    decimal MachineryAvailabilityRate,
    decimal MachineryEfficiencyScore
);