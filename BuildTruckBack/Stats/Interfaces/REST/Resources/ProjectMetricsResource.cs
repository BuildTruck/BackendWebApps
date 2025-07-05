namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing project metrics
/// </summary>
public record ProjectMetricsResource(
    int TotalProjects,
    int ActiveProjects,
    int CompletedProjects,
    int PlannedProjects,
    int OverdueProjects,
    Dictionary<string, int> ProjectsByStatus,
    decimal CompletionRate,
    decimal ActiveRate,
    bool HasOverdueProjects,
    string StatusSummary,
    string DominantStatus
);