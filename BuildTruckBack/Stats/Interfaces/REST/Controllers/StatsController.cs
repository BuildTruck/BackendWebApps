namespace BuildTruckBack.Stats.Interfaces.REST.Controllers;

using BuildTruckBack.Stats.Domain.Model.Commands;
using BuildTruckBack.Stats.Domain.Model.Queries;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Stats.Domain.Services;
using BuildTruckBack.Stats.Interfaces.REST.Resources;
using BuildTruckBack.Stats.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

/// <summary>
/// Stats REST API Controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Stats")]
public class StatsController : ControllerBase
{
    private readonly IStatsCommandService _commandService;
    private readonly IStatsQueryService _queryService;
    private readonly ILogger<StatsController> _logger;

    public StatsController(
        IStatsCommandService commandService,
        IStatsQueryService queryService,
        ILogger<StatsController> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get current stats for a manager
    /// </summary>
    [HttpGet("manager/{managerId:int}/current")]
    [SwaggerOperation(
        Summary = "Get current manager stats",
        Description = "Retrieve the most recent statistics for a specific manager",
        OperationId = "GetCurrentManagerStats")]
    [SwaggerResponse(200, "Stats retrieved successfully", typeof(ManagerStatsResource))]
    [SwaggerResponse(404, "Manager not found or no stats available")]
    [SwaggerResponse(400, "Invalid manager ID")]
    public async Task<ActionResult<ManagerStatsResource>> GetCurrentManagerStats(int managerId)
    {
        try
        {
            _logger.LogInformation("Getting current stats for manager {ManagerId}", managerId);

            var stats = await _queryService.GetCurrentStats(managerId);
            if (stats == null)
            {
                return NotFound($"No current stats found for manager {managerId}");
            }

            var resource = StatsResourceAssembler.ToResourceFromEntity(stats);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current stats for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    /// <summary>
    /// Get stats for a manager within a specific period
    /// </summary>
    [HttpGet("manager/{managerId:int}")]
    [SwaggerOperation(
        Summary = "Get manager stats for period",
        Description = "Retrieve statistics for a manager within a specific time period",
        OperationId = "GetManagerStats")]
    [SwaggerResponse(200, "Stats retrieved successfully", typeof(ManagerStatsResource))]
    [SwaggerResponse(404, "Manager not found or no stats available")]
    [SwaggerResponse(400, "Invalid parameters")]
    public async Task<ActionResult<ManagerStatsResource>> GetManagerStats(
        int managerId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? periodType = null)
    {
        try
        {
            _logger.LogInformation("Getting stats for manager {ManagerId} with period parameters", managerId);

            StatsPeriod? period = null;
            if (startDate.HasValue && endDate.HasValue)
            {
                period = StatsPeriod.Custom(startDate.Value, endDate.Value);
            }
            else if (!string.IsNullOrEmpty(periodType))
            {
                period = periodType.ToUpper() switch
                {
                    "CURRENT_MONTH" => StatsPeriod.CurrentMonth(),
                    "CURRENT_QUARTER" => StatsPeriod.CurrentQuarter(),
                    "CURRENT_YEAR" => StatsPeriod.CurrentYear(),
                    "LAST_30_DAYS" => StatsPeriod.LastNDays(30),
                    "LAST_90_DAYS" => StatsPeriod.LastNDays(90),
                    _ => StatsPeriod.CurrentMonth()
                };
            }

            var query = new GetManagerStatsQuery(managerId, period);
            var stats = await _queryService.Handle(query);

            if (stats == null)
            {
                return NotFound($"No stats found for manager {managerId} in the specified period");
            }

            var resource = StatsResourceAssembler.ToResourceFromEntity(stats);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    /// <summary>
    /// Calculate/recalculate stats for a manager
    /// </summary>
    [HttpPost("manager/{managerId:int}/calculate")]
    [SwaggerOperation(
        Summary = "Calculate manager stats",
        Description = "Calculate or recalculate statistics for a manager",
        OperationId = "CalculateManagerStats")]
    [SwaggerResponse(200, "Stats calculated successfully", typeof(ManagerStatsResource))]
    [SwaggerResponse(400, "Invalid request parameters")]
    [SwaggerResponse(404, "Manager not found")]
    public async Task<ActionResult<ManagerStatsResource>> CalculateManagerStats(
        int managerId,
        [FromBody] CalculateStatsResource? request = null)
    {
        try
        {
            _logger.LogInformation("Calculating stats for manager {ManagerId}", managerId);

            StatsPeriod period;
            if (request?.StartDate.HasValue == true && request?.EndDate.HasValue == true)
            {
                period = StatsPeriod.Custom(request.StartDate.Value, request.EndDate.Value);
            }
            else
            {
                period = StatsPeriod.CurrentMonth();
            }

            var command = new CalculateManagerStatsCommand(
                managerId,
                period,
                request?.ForceRecalculation ?? false,
                request?.SaveHistory ?? true,
                request?.Notes
            );

            var stats = await _commandService.Handle(command);
            var resource = StatsResourceAssembler.ToResourceFromEntity(stats);

            return Ok(resource);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for calculating stats for manager {ManagerId}", managerId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating stats for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    /// <summary>
    /// Get stats history for a manager
    /// </summary>
    [HttpGet("manager/{managerId:int}/history")]
    [SwaggerOperation(
        Summary = "Get manager stats history",
        Description = "Retrieve historical statistics for a manager",
        OperationId = "GetManagerStatsHistory")]
    [SwaggerResponse(200, "History retrieved successfully", typeof(IEnumerable<StatsHistoryResource>))]
    [SwaggerResponse(404, "Manager not found")]
    [SwaggerResponse(400, "Invalid parameters")]
    public async Task<ActionResult<IEnumerable<StatsHistoryResource>>> GetManagerStatsHistory(
        int managerId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? periodType = null,
        [FromQuery] int? limit = null,
        [FromQuery] bool includeManual = true)
    {
        try
        {
            _logger.LogInformation("Getting stats history for manager {ManagerId}", managerId);

            var query = new GetStatsHistoryQuery(
                managerId,
                startDate,
                endDate,
                periodType,
                limit,
                true, // orderByNewest
                includeManual
            );

            var history = await _queryService.Handle(query);
            var resources = history.Select(StatsResourceAssembler.ToResourceFromEntity);

            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats history for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }

    // /// <summary>
    // /// Create manual snapshot of current stats
    // /// </summary>
    // [HttpPost("manager/{managerId:int}/snapshot")]
    // [SwaggerOperation(
    //     Summary = "Create manual stats snapshot",
    //     Description = "Create a manual snapshot of current manager statistics",
    //     OperationId = "CreateManualSnapshot")]
    // [SwaggerResponse(200, "Snapshot created successfully", typeof(StatsHistoryResource))]
    // [SwaggerResponse(400, "Invalid request")]
    // [SwaggerResponse(404, "Manager stats not found")]
    // public async Task<ActionResult<StatsHistoryResource>> CreateManualSnapshot(
    //     int managerId,
    //     [FromBody] CreateSnapshotResource request)
    // {
    //     try
    //     {
    //         _logger.LogInformation("Creating manual snapshot for manager {ManagerId}", managerId);
    //
    //         // First, get current stats
    //         var currentStats = await _queryService.GetCurrentStats(managerId);
    //         if (currentStats == null)
    //         {
    //             return NotFound($"No current stats found for manager {managerId}");
    //         }
    //
    //         var snapshot = await _commandService.CreateManualSnapshot(currentStats.Id, request.Notes ?? "");
    //         var resource = StatsResourceAssembler.ToResourceFromEntity(snapshot);
    //
    //         return Ok(resource);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error creating manual snapshot for manager {ManagerId}", managerId);
    //         return StatusCode(500, "Internal server error occurred");
    //     }
    // }

    /// <summary>
    /// Get manager dashboard data
    /// </summary>
    [HttpGet("manager/{managerId:int}/dashboard")]
    [SwaggerOperation(
        Summary = "Get manager dashboard",
        Description = "Retrieve comprehensive dashboard data for a manager",
        OperationId = "GetManagerDashboard")]
    [SwaggerResponse(200, "Dashboard data retrieved successfully")]
    [SwaggerResponse(404, "Manager not found")]
    public async Task<ActionResult<object>> GetManagerDashboard(int managerId)
    {
        try
        {
            _logger.LogInformation("Getting dashboard data for manager {ManagerId}", managerId);

            var dashboard = await _queryService.GetManagerDashboard(managerId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for manager {ManagerId}", managerId);
            return StatusCode(500, "Internal server error occurred");
        }
    }
}

// /// <summary>
    // /// Get performance trends for a manager
    // /// </summary>
    // [HttpGet("manager/{managerId:int}/trends")]
    // [SwaggerOperation(
    //     Summary = "Get performance trends",
    //     Description = "Retrieve performance and safety trends for a manager",
    //     OperationId = "GetManagerTrends")]
    // [SwaggerResponse(200, "Trends retrieved successfully")]
    // [SwaggerResponse(404, "Manager not found")]
    // public async Task<ActionResult<object>> GetManagerTrends(
    //     int managerId,
    //     [FromQuery] int months = 12)
    // {
    //     try
    //     {
    //         _logger.LogInformation("Getting trends for manager {ManagerId} over {Months} months", managerId, months);
    //
    //         var performanceTrends = await _queryService.GetPerformanceTrends(managerId, months);
    //         var safetyTrends = await _queryService.GetSafetyTrends(managerId, months);
    //
    //         var result = new
    //         {
    //             ManagerId = managerId,
    //             Months = months,
    //             PerformanceTrends = performanceTrends.Select(t => new { Date = t.Date, Score = t.Score }),
    //             SafetyTrends = safetyTrends.Select(t => new { Date = t.Date, Score = t.SafetyScore })
    //         };
    //
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting trends for manager {ManagerId}", managerId);
    //         return StatusCode(500, "Internal server error occurred");
    //     }
    // }

    // /// <summary>
    // /// Get top performing managers
    // /// </summary>
    // [HttpGet("top-performers")]
    // [SwaggerOperation(
    //     Summary = "Get top performing managers",
    //     Description = "Retrieve list of top performing managers",
    //     OperationId = "GetTopPerformers")]
    // [SwaggerResponse(200, "Top performers retrieved successfully", typeof(IEnumerable<ManagerStatsResource>))]
    // public async Task<ActionResult<IEnumerable<ManagerStatsResource>>> GetTopPerformers(
    //     [FromQuery] int count = 10,
    //     [FromQuery] string? periodType = null)
    // {
    //     try
    //     {
    //         _logger.LogInformation("Getting top {Count} performers", count);
    //
    //         var topPerformers = await _queryService.GetTopPerformers(count, periodType);
    //         var resources = topPerformers.Select(StatsResourceAssembler.ToResourceFromEntity);
    //
    //         return Ok(resources);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting top performers");
    //         return StatusCode(500, "Internal server error occurred");
    //     }
    // }

    // /// <summary>
    // /// Get system-wide summary
    // /// </summary>
    // [HttpGet("system/summary")]
    // [SwaggerOperation(
    //     Summary = "Get system summary",
    //     Description = "Retrieve system-wide statistics summary",
    //     OperationId = "GetSystemSummary")]
    // [SwaggerResponse(200, "System summary retrieved successfully")]
    // public async Task<ActionResult<object>> GetSystemSummary()
    // {
    //     try
    //     {
    //         _logger.LogInformation("Getting system-wide summary");
    //
    //         var summary = await _queryService.GetSystemSummary();
    //         return Ok(summary);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error getting system summary");
    //         return StatusCode(500, "Internal server error occurred");
    //     }
    // }

//     /// <summary>
//     /// Search stats by criteria
//     /// </summary>
//     [HttpGet("search")]
//     [SwaggerOperation(
//         Summary = "Search stats",
//         Description = "Search statistics by various criteria",
//         OperationId = "SearchStats")]
//     [SwaggerResponse(200, "Search results retrieved successfully", typeof(IEnumerable<ManagerStatsResource>))]
//     public async Task<ActionResult<IEnumerable<ManagerStatsResource>>> SearchStats(
//         [FromQuery] string? grade = null,
//         [FromQuery] decimal? minScore = null,
//         [FromQuery] decimal? maxScore = null,
//         [FromQuery] DateTime? fromDate = null,
//         [FromQuery] DateTime? toDate = null)
//     {
//         try
//         {
//             _logger.LogInformation("Searching stats with criteria");
//
//             var results = await _queryService.SearchStats(grade, minScore, maxScore, fromDate, toDate);
//             var resources = results.Select(StatsResourceAssembler.ToResourceFromEntity);
//
//             return Ok(resources);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error searching stats");
//             return StatusCode(500, "Internal server error occurred");
//         }
//     }
// }