namespace BuildTruckBack.Stats.Application.Internal.CommandServices;

using BuildTruckBack.Stats.Domain.Model.Aggregates;
using BuildTruckBack.Stats.Domain.Model.Commands;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Stats.Domain.Repositories;
using BuildTruckBack.Stats.Domain.Services;
using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Shared.Domain.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Command service implementation for stats operations
/// </summary>
public class StatsCommandService : IStatsCommandService
{
    private readonly IStatsRepository _statsRepository;
    private readonly IStatsHistoryRepository _historyRepository;
    private readonly IProjectContextService _projectContextService;
    private readonly IUserContextService _userContextService;
    private readonly IPersonnelContextService _personnelContextService; // NUEVO
    private readonly IMaterialContextService _materialContextService;
    private readonly IIncidentContextService _incidentContextService;
    private readonly IMachineryContextService _machineryContextService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StatsCommandService> _logger;

    public StatsCommandService(
        IStatsRepository statsRepository,
        IStatsHistoryRepository historyRepository,
        IProjectContextService projectContextService,
        IUserContextService userContextService,
        IPersonnelContextService personnelContextService, // NUEVO
        IMaterialContextService materialContextService,
        IIncidentContextService incidentContextService,
        IMachineryContextService machineryContextService,
        IUnitOfWork unitOfWork,
        ILogger<StatsCommandService> logger)
    {
        _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _projectContextService = projectContextService ?? throw new ArgumentNullException(nameof(projectContextService));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _personnelContextService = personnelContextService ?? throw new ArgumentNullException(nameof(personnelContextService)); // NUEVO
        _materialContextService = materialContextService ?? throw new ArgumentNullException(nameof(materialContextService));
        _incidentContextService = incidentContextService ?? throw new ArgumentNullException(nameof(incidentContextService));
        _machineryContextService = machineryContextService ?? throw new ArgumentNullException(nameof(machineryContextService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ManagerStats> Handle(CalculateManagerStatsCommand command)
    {
        try
        {
            _logger.LogInformation("Calculating stats for manager {ManagerId} for period {Period}", 
                command.ManagerId, command.Period);

            // Validate command
            if (!command.IsValid())
            {
                var errors = string.Join(", ", command.GetValidationErrors());
                throw new ArgumentException($"Invalid command: {errors}");
            }

            // Verify manager exists
            if (!await _userContextService.IsValidManagerAsync(command.ManagerId))
            {
                throw new ArgumentException($"Manager {command.ManagerId} not found or invalid");
            }

            // Check if we need to force recalculation
            if (!command.ForceRecalculation)
            {
                var existingStats = await _statsRepository.FindByManagerIdAndPeriodAsync(command.ManagerId, command.Period);
                if (existingStats != null && existingStats.IsRecent())
                {
                    _logger.LogDebug("Returning existing recent stats for manager {ManagerId}", command.ManagerId);
                    return existingStats;
                }
            }

            // Get project IDs for this manager
            var projectIds = await _projectContextService.GetManagerProjectIdsAsync(command.ManagerId);
            if (!projectIds.Any())
            {
                _logger.LogWarning("No projects found for manager {ManagerId}", command.ManagerId);
                // Return empty stats
                return CreateEmptyStats(command.ManagerId, command.Period);
            }

            // Calculate all metrics
            var projectMetrics = await _projectContextService.GetProjectMetricsAsync(command.ManagerId, command.Period);
            var personnelMetrics = await _personnelContextService.GetPersonnelMetricsAsync(projectIds, command.Period); // CAMBIO
            var incidentMetrics = await _incidentContextService.GetIncidentMetricsAsync(projectIds, command.Period);
            var materialMetrics = await _materialContextService.GetMaterialMetricsAsync(projectIds, command.Period);
            var machineryMetrics = await _machineryContextService.GetMachineryMetricsAsync(projectIds, command.Period);

            // Create or update stats
            var stats = new ManagerStats(
                command.ManagerId,
                command.Period,
                projectMetrics,
                personnelMetrics,
                incidentMetrics,
                materialMetrics,
                machineryMetrics
            );

            // Save stats
            var existingStats2 = await _statsRepository.FindByManagerIdAndPeriodAsync(command.ManagerId, command.Period);
            if (existingStats2 != null)
            {
                existingStats2.RecalculateMetrics(projectMetrics, personnelMetrics, incidentMetrics, materialMetrics, machineryMetrics);
                _statsRepository.Update(existingStats2);
                stats = existingStats2;
            }
            else
            {
                await _statsRepository.AddAsync(stats);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully calculated stats for manager {ManagerId} with score {Score}", 
                command.ManagerId, stats.OverallPerformanceScore);

            // Save history if requested
            if (command.SaveHistory)
            {
                await Handle(SaveStatsHistoryCommand.Automatic(stats.Id, command.HistoryNotes));
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating stats for manager {ManagerId}", command.ManagerId);
            throw;
        }
    }

    public async Task<StatsHistory> Handle(SaveStatsHistoryCommand command)
    {
        try
        {
            _logger.LogInformation("Saving stats history for ManagerStats {ManagerStatsId}", command.ManagerStatsId);

            // Validate command
            if (!command.IsValid())
            {
                var errors = string.Join(", ", command.GetValidationErrors());
                throw new ArgumentException($"Invalid command: {errors}");
            }

            // Get the manager stats
            var managerStats = await _statsRepository.FindByIdAsync(command.ManagerStatsId);
            if (managerStats == null)
            {
                throw new ArgumentException($"ManagerStats {command.ManagerStatsId} not found");
            }

            // Check if we should overwrite existing
            if (!command.OverwriteExisting)
            {
                var existingSnapshot = await _historyRepository.FindExistingSnapshotAsync(
                    managerStats.ManagerId, 
                    managerStats.Period.PeriodType, 
                    DateTime.UtcNow.AddHours(-5));

                if (existingSnapshot != null)
                {
                    _logger.LogDebug("Snapshot already exists for manager {ManagerId} and period {Period}", 
                        managerStats.ManagerId, managerStats.Period.PeriodType);
                    return existingSnapshot;
                }
            }

            // Create history snapshot
            var history = new StatsHistory(managerStats, command.Notes ?? string.Empty, command.IsManualSnapshot);

            await _historyRepository.AddAsync(history);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully saved stats history {HistoryId} for manager {ManagerId}", 
                history.Id, managerStats.ManagerId);

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving stats history for ManagerStats {ManagerStatsId}", command.ManagerStatsId);
            throw;
        }
    }

    public async Task<ManagerStats> RecalculateManagerStats(int managerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var period = startDate.HasValue && endDate.HasValue 
            ? StatsPeriod.Custom(startDate.Value, endDate.Value)
            : StatsPeriod.CurrentMonth();

        var command = CalculateManagerStatsCommand.WithForceRecalculation(managerId, period, true);
        return await Handle(command);
    }

    public async Task<ManagerStats> UpdateManagerStats(int managerStatsId)
    {
        var existingStats = await _statsRepository.FindByIdAsync(managerStatsId);
        if (existingStats == null)
        {
            throw new ArgumentException($"ManagerStats {managerStatsId} not found");
        }

        var command = CalculateManagerStatsCommand.WithForceRecalculation(existingStats.ManagerId, existingStats.Period);
        return await Handle(command);
    }

    public async Task<StatsHistory> CreateManualSnapshot(int managerStatsId, string notes)
    {
        var command = SaveStatsHistoryCommand.Manual(managerStatsId, notes);
        return await Handle(command);
    }

    public async Task<int> DeleteOutdatedStats(int daysOld = 90)
    {
        _logger.LogInformation("Deleting stats older than {DaysOld} days", daysOld);
        
        var deletedCount = await _statsRepository.DeleteOldStatsAsync(daysOld);
        await _unitOfWork.CompleteAsync();
        
        _logger.LogInformation("Deleted {Count} outdated stats", deletedCount);
        return deletedCount;
    }

    public async Task<int> ArchiveOldHistory(int daysOld = 365)
    {
        _logger.LogInformation("Archiving history older than {DaysOld} days", daysOld);
        
        var archivedCount = await _historyRepository.ArchiveOldSnapshotsAsync(daysOld);
        await _unitOfWork.CompleteAsync();
        
        _logger.LogInformation("Archived {Count} old history snapshots", archivedCount);
        return archivedCount;
    }

    public async Task<IEnumerable<ManagerStats>> BulkCalculateStats(IEnumerable<int> managerIds, DateTime? startDate = null, DateTime? endDate = null)
    {
        var results = new List<ManagerStats>();
        var period = startDate.HasValue && endDate.HasValue 
            ? StatsPeriod.Custom(startDate.Value, endDate.Value)
            : StatsPeriod.CurrentMonth();

        foreach (var managerId in managerIds)
        {
            try
            {
                var command = new CalculateManagerStatsCommand(managerId, period, false, true);
                var stats = await Handle(command);
                results.Add(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating stats for manager {ManagerId} in bulk operation", managerId);
                // Continue with other managers
            }
        }

        return results;
    }

    public async Task ScheduleAutomaticCalculation(int managerId, string cronExpression)
    {
        // This would integrate with a background job scheduler like Hangfire or Quartz
        _logger.LogInformation("Scheduling automatic calculation for manager {ManagerId} with expression {CronExpression}", 
            managerId, cronExpression);
        
        // Implementation would depend on your background job system
        throw new NotImplementedException("Automatic scheduling not implemented yet");
    }

    private ManagerStats CreateEmptyStats(int managerId, StatsPeriod period)
    {
        return new ManagerStats(
            managerId,
            period,
            ProjectMetrics.FromCounts(0, 0, 0),
            PersonnelMetrics.FromCounts(0, 0),
            IncidentMetrics.FromCounts(0, 0, 0),
            MaterialMetrics.FromCounts(0, 0, 0, 0m),
            MachineryMetrics.FromCounts(0, 0, 0)
        );
    }
    public static CalculateManagerStatsCommand WithForceRecalculation(int managerId, StatsPeriod period, bool saveHistory = true)
    {
        return new CalculateManagerStatsCommand(managerId, period, true, saveHistory);
    }

}