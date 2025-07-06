namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing manager statistics for API responses
/// </summary>
public record ManagerStatsResource(
    int Id,
    int ManagerId,
    PeriodResource Period,
    ProjectMetricsResource ProjectMetrics,
    PersonnelMetricsResource PersonnelMetrics,
    IncidentMetricsResource IncidentMetrics,
    MaterialMetricsResource MaterialMetrics,
    MachineryMetricsResource MachineryMetrics,
    decimal OverallPerformanceScore,
    string PerformanceGrade,
    List<string> Alerts,
    List<string> Recommendations,
    DateTime CalculatedAt,
    bool IsCurrentPeriod,
    string CalculationSource,
    bool HasCriticalAlerts,
    string OverallStatus,
    Dictionary<string, decimal> ScoreBreakdown
);