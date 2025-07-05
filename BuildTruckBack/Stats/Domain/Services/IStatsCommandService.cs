namespace BuildTruckBack.Stats.Domain.Services;

using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Model.Commands;

/// <summary>
/// Domain service interface for stats command operations
/// </summary>
public interface IStatsCommandService
{
    /// <summary>
    /// Calculate and save manager statistics
    /// </summary>
    Task<ManagerStats> Handle(CalculateManagerStatsCommand command);

    /// <summary>
    /// Save stats snapshot to history
    /// </summary>
    Task<StatsHistory> Handle(SaveStatsHistoryCommand command);

    /// <summary>
    /// Recalculate stats for a manager (force refresh)
    /// </summary>
    Task<ManagerStats> RecalculateManagerStats(int managerId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Update existing stats with new data
    /// </summary>
    Task<ManagerStats> UpdateManagerStats(int managerStatsId);

    /// <summary>
    /// Create manual snapshot of current stats
    /// </summary>
    Task<StatsHistory> CreateManualSnapshot(int managerStatsId, string notes);

    /// <summary>
    /// Delete outdated stats (cleanup operation)
    /// </summary>
    Task<int> DeleteOutdatedStats(int daysOld = 90);

    /// <summary>
    /// Archive old history snapshots
    /// </summary>
    Task<int> ArchiveOldHistory(int daysOld = 365);

    /// <summary>
    /// Bulk calculate stats for multiple managers
    /// </summary>
    Task<IEnumerable<ManagerStats>> BulkCalculateStats(IEnumerable<int> managerIds, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Schedule automatic stats calculation
    /// </summary>
    Task ScheduleAutomaticCalculation(int managerId, string cronExpression);
}