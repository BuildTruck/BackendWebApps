namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing incident metrics in history snapshot
/// </summary>
public record IncidentHistoryMetricsResource(
    int TotalIncidents,
    int CriticalIncidents,
    decimal SafetyScore
);