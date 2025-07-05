namespace BuildTruckBack.Stats.Domain.Services;

using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Model.Queries;

/// <summary>
/// Domain service interface for stats query operations
/// </summary>
public interface IStatsQueryService
{
    /// <summary>
    /// Get manager statistics
    /// </summary>
    Task<ManagerStats?> Handle(GetManagerStatsQuery query);

    /// <summary>
    /// Get stats history
    /// </summary>
    Task<IEnumerable<StatsHistory>> Handle(GetStatsHistoryQuery query);

    /// <summary>
    /// Get current stats for a manager (most recent)
    /// </summary>
    Task<ManagerStats?> GetCurrentStats(int managerId);

    /// <summary>
    /// Get stats comparison between two periods
    /// </summary>
    Task<StatsComparison?> GetStatsComparison(int managerId, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End);

    /// <summary>
    /// Get performance trends for a manager
    /// </summary>
    Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrends(int managerId, int months = 12);

    /// <summary>
    /// Get safety trends for a manager
    /// </summary>
    Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrends(int managerId, int months = 12);

    /// <summary>
    /// Get top performing managers
    /// </summary>
    Task<IEnumerable<ManagerStats>> GetTopPerformers(int count = 10, string? periodType = null);

    /// <summary>
    /// Get managers with critical alerts
    /// </summary>
    Task<IEnumerable<ManagerStats>> GetManagersWithCriticalAlerts();

    /// <summary>
    /// Get system-wide statistics summary
    /// </summary>
    Task<Dictionary<string, object>> GetSystemSummary();

    /// <summary>
    /// Get manager ranking by performance score
    /// </summary>
    Task<IEnumerable<(int ManagerId, decimal Score, int Rank)>> GetManagerRankings();

    /// <summary>
    /// Search stats by criteria
    /// </summary>
    Task<IEnumerable<ManagerStats>> SearchStats(
        string? grade = null,
        decimal? minScore = null,
        decimal? maxScore = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get dashboard data for a manager
    /// </summary>
    Task<Dictionary<string, object>> GetManagerDashboard(int managerId);

    /// <summary>
    /// Get alerts summary for multiple managers
    /// </summary>
    Task<Dictionary<int, List<string>>> GetManagersAlerts(IEnumerable<int> managerIds);

    /// <summary>
    /// Export stats data for reporting
    /// </summary>
    Task<IEnumerable<Dictionary<string, object>>> ExportStatsData(
        IEnumerable<int> managerIds,
        DateTime? startDate = null,
        DateTime? endDate = null);
}