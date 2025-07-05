namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing project metrics in history snapshot
/// </summary>
public record ProjectHistoryMetricsResource(
    int TotalProjects,
    int ActiveProjects,
    int CompletedProjects,
    decimal ProjectCompletionRate
);