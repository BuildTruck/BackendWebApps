namespace BuildTruckBack.Stats.Domain.Repositories;

using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// Repository interface for ManagerStats aggregate
/// </summary>
public interface IStatsRepository : IBaseRepository<ManagerStats>
{
    /// <summary>
    /// Find stats by manager ID
    /// </summary>
    Task<ManagerStats?> FindByManagerIdAsync(int managerId);

    /// <summary>
    /// Find stats by manager ID and period
    /// </summary>
    Task<ManagerStats?> FindByManagerIdAndPeriodAsync(int managerId, StatsPeriod period);

    /// <summary>
    /// Find the most recent stats for a manager
    /// </summary>
    Task<ManagerStats?> FindMostRecentByManagerIdAsync(int managerId);

    /// <summary>
    /// Find stats that are outdated (older than specified hours)
    /// </summary>
    Task<IEnumerable<ManagerStats>> FindOutdatedStatsAsync(int hoursOld = 24);

    /// <summary>
    /// Find stats by performance grade
    /// </summary>
    Task<IEnumerable<ManagerStats>> FindByPerformanceGradeAsync(string grade);

    /// <summary>
    /// Find stats with critical alerts
    /// </summary>
    Task<IEnumerable<ManagerStats>> FindWithCriticalAlertsAsync();

    /// <summary>
    /// Find stats within performance score range
    /// </summary>
    Task<IEnumerable<ManagerStats>> FindByPerformanceRangeAsync(decimal minScore, decimal maxScore);

    /// <summary>
    /// Find all stats for managers within a date range
    /// </summary>
    Task<IEnumerable<ManagerStats>> FindByCalculationDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Check if stats exist for manager and period
    /// </summary>
    Task<bool> ExistsForManagerAndPeriodAsync(int managerId, StatsPeriod period);

    /// <summary>
    /// Delete old stats (older than specified days)
    /// </summary>
    Task<int> DeleteOldStatsAsync(int daysOld = 90);

    /// <summary>
    /// Get statistics summary for all managers
    /// </summary>
    Task<Dictionary<string, object>> GetSystemWideSummaryAsync();

    /// <summary>
    /// Update stats without changing timestamps
    /// </summary>
    Task UpdateStatsAsync(ManagerStats stats);
}