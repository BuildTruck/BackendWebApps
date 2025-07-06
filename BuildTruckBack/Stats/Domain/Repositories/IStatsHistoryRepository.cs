namespace BuildTruckBack.Stats.Domain.Repositories;

using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Stats.Domain.Model.Aggregates;

/// <summary>
/// Repository interface for StatsHistory aggregate
/// </summary>
public interface IStatsHistoryRepository : IBaseRepository<StatsHistory>
{
    /// <summary>
    /// Find history by manager ID
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId);

    /// <summary>
    /// Find history by manager ID with pagination
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId, int skip, int take);

    /// <summary>
    /// Find history by manager ID within date range
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindByManagerIdAndDateRangeAsync(
        int managerId, 
        DateTime startDate, 
        DateTime endDate);

    /// <summary>
    /// Find history by manager ID and period type
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindByManagerIdAndPeriodTypeAsync(int managerId, string periodType);

    /// <summary>
    /// Find most recent history entries for a manager
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindRecentByManagerIdAsync(int managerId, int count = 10);

    /// <summary>
    /// Find history for comparison (same period type, chronological order)
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindForComparisonAsync(int managerId, string periodType, int count = 5);

    /// <summary>
    /// Find snapshots from a specific month
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindByMonthAsync(int managerId, int year, int month);

    /// <summary>
    /// Find snapshots from a specific year
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindByYearAsync(int managerId, int year);

    /// <summary>
    /// Find manual snapshots only
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindManualSnapshotsAsync(int managerId);

    /// <summary>
    /// Find automatic snapshots only
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindAutomaticSnapshotsAsync(int managerId);

    /// <summary>
    /// Check if snapshot exists for manager stats ID
    /// </summary>
    Task<bool> ExistsForManagerStatsAsync(int managerStatsId);

    /// <summary>
    /// Find existing snapshot for the same period (to avoid duplicates)
    /// </summary>
    Task<StatsHistory?> FindExistingSnapshotAsync(int managerId, string periodType, DateTime snapshotDate);

    /// <summary>
    /// Get performance trend data
    /// </summary>
    Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrendAsync(int managerId, int months = 12);

    /// <summary>
    /// Get safety score trend data
    /// </summary>
    Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrendAsync(int managerId, int months = 12);

    /// <summary>
    /// Delete snapshots older than specified days
    /// </summary>
    Task<int> DeleteOldSnapshotsAsync(int daysOld = 365);

    /// <summary>
    /// Archive old snapshots (mark for archival, don't delete)
    /// </summary>
    Task<int> ArchiveOldSnapshotsAsync(int daysOld = 365);

    /// <summary>
    /// Get statistics about the history repository
    /// </summary>
    Task<Dictionary<string, object>> GetRepositoryStatsAsync();

    /// <summary>
    /// Find snapshots that should be archived
    /// </summary>
    Task<IEnumerable<StatsHistory>> FindSnapshotsToArchiveAsync();

    /// <summary>
    /// Get monthly summary for a manager
    /// </summary>
    Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int managerId, int year);

    /// <summary>
    /// Get quarterly summary for a manager
    /// </summary>
    Task<Dictionary<string, decimal>> GetQuarterlySummaryAsync(int managerId, int year);
}