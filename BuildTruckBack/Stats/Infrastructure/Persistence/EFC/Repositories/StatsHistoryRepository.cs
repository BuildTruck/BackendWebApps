namespace BuildTruckBack.Stats.Infrastructure.Persistence.EFC.Repositories;

using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Repository implementation for StatsHistory aggregate
/// </summary>
public class StatsHistoryRepository : BaseRepository<StatsHistory>, IStatsHistoryRepository
{
    private readonly ILogger<StatsHistoryRepository> _logger;

    public StatsHistoryRepository(AppDbContext context, ILogger<StatsHistoryRepository> logger) 
        : base(context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding history by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindByManagerIdAsync(int managerId, int skip, int take)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .OrderByDescending(h => h.SnapshotDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding paginated history by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindByManagerIdAndDateRangeAsync(int managerId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate && h.SnapshotDate <= endDate)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding history by manager ID {ManagerId} and date range {StartDate}-{EndDate}", 
                managerId, startDate, endDate);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindByManagerIdAndPeriodTypeAsync(int managerId, string periodType)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.PeriodType == periodType)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding history by manager ID {ManagerId} and period type {PeriodType}", 
                managerId, periodType);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindRecentByManagerIdAsync(int managerId, int count = 10)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .OrderByDescending(h => h.SnapshotDate)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding recent history by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindForComparisonAsync(int managerId, string periodType, int count = 5)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.PeriodType == periodType)
                .Where(h => !h.IsManualSnapshot)
                .OrderByDescending(h => h.SnapshotDate)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding history for comparison by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindByMonthAsync(int managerId, int year, int month)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate && h.SnapshotDate <= endDate)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding history by manager ID {ManagerId} for month {Year}-{Month}", 
                managerId, year, month);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindByYearAsync(int managerId, int year)
    {
        try
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate && h.SnapshotDate <= endDate)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding history by manager ID {ManagerId} for year {Year}", managerId, year);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindManualSnapshotsAsync(int managerId)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.IsManualSnapshot)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding manual snapshots by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindAutomaticSnapshotsAsync(int managerId)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => !h.IsManualSnapshot)
                .OrderByDescending(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding automatic snapshots by manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<bool> ExistsForManagerStatsAsync(int managerStatsId)
    {
        try
        {
            return await Context.Set<StatsHistory>()
                .AnyAsync(h => h.ManagerStatsId == managerStatsId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if history exists for manager stats ID {ManagerStatsId}", managerStatsId);
            throw;
        }
    }

    public async Task<StatsHistory?> FindExistingSnapshotAsync(int managerId, string periodType, DateTime snapshotDate)
    {
        try
        {
            // Check if a snapshot exists for the same day and period type
            var dayStart = snapshotDate.Date;
            var dayEnd = dayStart.AddDays(1);

            return await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.PeriodType == periodType)
                .Where(h => h.SnapshotDate >= dayStart && h.SnapshotDate < dayEnd)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding existing snapshot for manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<(DateTime Date, decimal Score)>> GetPerformanceTrendAsync(int managerId, int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddHours(-5).AddMonths(-months);

            var trends = await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate)
                .Where(h => !h.IsManualSnapshot)
                .OrderBy(h => h.SnapshotDate)
                .Select(h => new { h.SnapshotDate, h.OverallPerformanceScore })
                .ToListAsync();

            return trends.Select(t => (t.SnapshotDate, t.OverallPerformanceScore)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance trend for manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<IEnumerable<(DateTime Date, decimal SafetyScore)>> GetSafetyTrendAsync(int managerId, int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddHours(-5).AddMonths(-months);

            var trends = await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate)
                .Where(h => !h.IsManualSnapshot)
                .OrderBy(h => h.SnapshotDate)
                .Select(h => new { h.SnapshotDate, h.SafetyScore })
                .ToListAsync();

            return trends.Select(t => (t.SnapshotDate, t.SafetyScore)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting safety trend for manager ID {ManagerId}", managerId);
            throw;
        }
    }

    public async Task<int> DeleteOldSnapshotsAsync(int daysOld = 365)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddHours(-5).AddDays(-daysOld);

            var oldSnapshots = await Context.Set<StatsHistory>()
                .Where(h => h.SnapshotDate < cutoffDate)
                .ToListAsync();

            if (oldSnapshots.Any())
            {
                Context.Set<StatsHistory>().RemoveRange(oldSnapshots);
                await Context.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} old snapshots older than {DaysOld} days", 
                    oldSnapshots.Count, daysOld);
            }

            return oldSnapshots.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old snapshots older than {DaysOld} days", daysOld);
            throw;
        }
    }

    public async Task<int> ArchiveOldSnapshotsAsync(int daysOld = 365)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddHours(-5).AddDays(-daysOld);

            var oldSnapshots = await Context.Set<StatsHistory>()
                .Where(h => h.SnapshotDate < cutoffDate)
                .Where(h => !h.Notes.Contains("[ARCHIVED]"))
                .ToListAsync();

            foreach (var snapshot in oldSnapshots)
            {
                snapshot.UpdateNotes($"[ARCHIVED] {snapshot.Notes}");
            }

            if (oldSnapshots.Any())
            {
                await Context.SaveChangesAsync();
                _logger.LogInformation("Archived {Count} old snapshots older than {DaysOld} days", 
                    oldSnapshots.Count, daysOld);
            }

            return oldSnapshots.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old snapshots older than {DaysOld} days", daysOld);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetRepositoryStatsAsync()
    {
        try
        {
            var allHistory = Context.Set<StatsHistory>();

            var stats = new Dictionary<string, object>
            {
                ["TotalSnapshots"] = await allHistory.CountAsync(),
                ["ManualSnapshots"] = await allHistory.CountAsync(h => h.IsManualSnapshot),
                ["AutomaticSnapshots"] = await allHistory.CountAsync(h => !h.IsManualSnapshot),
                ["UniqueManagers"] = await allHistory.Select(h => h.ManagerId).Distinct().CountAsync(),
                ["OldestSnapshot"] = await allHistory.MinAsync(h => (DateTime?)h.SnapshotDate) ?? DateTime.MinValue,
                ["NewestSnapshot"] = await allHistory.MaxAsync(h => (DateTime?)h.SnapshotDate) ?? DateTime.MinValue,
                ["AveragePerformance"] = await allHistory.AverageAsync(h => h.OverallPerformanceScore),
                ["PeriodTypeDistribution"] = await allHistory
                    .GroupBy(h => h.PeriodType)
                    .ToDictionaryAsync(g => g.Key, g => g.Count())
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository stats");
            throw;
        }
    }

    public async Task<IEnumerable<StatsHistory>> FindSnapshotsToArchiveAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddHours(-5).AddDays(-365);

            return await Context.Set<StatsHistory>()
                .Where(h => h.SnapshotDate < cutoffDate)
                .Where(h => !h.Notes.Contains("[ARCHIVED]"))
                .OrderBy(h => h.SnapshotDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding snapshots to archive");
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int managerId, int year)
    {
        try
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var monthlyData = await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate && h.SnapshotDate <= endDate)
                .GroupBy(h => h.SnapshotDate.Month)
                .ToDictionaryAsync(
                    g => $"Month {g.Key}",
                    g => g.Average(h => h.OverallPerformanceScore)
                );

            return monthlyData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly summary for manager ID {ManagerId} and year {Year}", managerId, year);
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetQuarterlySummaryAsync(int managerId, int year)
    {
        try
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var quarterlyData = await Context.Set<StatsHistory>()
                .Where(h => h.ManagerId == managerId)
                .Where(h => h.SnapshotDate >= startDate && h.SnapshotDate <= endDate)
                .GroupBy(h => (h.SnapshotDate.Month - 1) / 3 + 1)
                .ToDictionaryAsync(
                    g => $"Q{g.Key}",
                    g => g.Average(h => h.OverallPerformanceScore)
                );

            return quarterlyData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quarterly summary for manager ID {ManagerId} and year {Year}", managerId, year);
            throw;
        }
    }

    // REMOVER estos métodos override que causan problemas:
    // NO agregues override AddAsync - ya está en BaseRepository
}