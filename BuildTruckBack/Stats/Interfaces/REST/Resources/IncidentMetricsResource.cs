namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing incident metrics
/// </summary>
public record IncidentMetricsResource(
    int TotalIncidents,
    int CriticalIncidents,
    int OpenIncidents,
    int ResolvedIncidents,
    Dictionary<string, int> IncidentsBySeverity,
    Dictionary<string, int> IncidentsByType,
    Dictionary<string, int> IncidentsByStatus,
    decimal AverageResolutionTimeHours,
    decimal CriticalRate,
    decimal ResolutionRate,
    decimal OpenRate,
    string SafetyStatus,
    bool HasCriticalIncidents,
    bool NeedsAttention,
    string MostCommonSeverity,
    string MostCommonType,
    decimal SafetyScore,
    string IncidentSummary
);