// BuildTruckBack.Stats.Infrastructure.ACL.PersonnelContextService
namespace BuildTruckBack.Stats.Infrastructure.ACL;

using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Personnel.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// ACL Service implementation for Personnel bounded context
/// </summary>
public class PersonnelContextService : IPersonnelContextService
{
    private readonly IPersonnelFacade _personnelFacade;
    private readonly ILogger<PersonnelContextService> _logger;

    public PersonnelContextService(
        IPersonnelFacade personnelFacade,
        ILogger<PersonnelContextService> logger)
    {
        _personnelFacade = personnelFacade ?? throw new ArgumentNullException(nameof(personnelFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PersonnelMetrics> GetPersonnelMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            _logger.LogDebug("Getting personnel metrics for {Count} projects", projectIds.Count);

            if (!projectIds.Any())
            {
                return PersonnelMetrics.FromCounts(0, 0);
            }

            var totalPersonnel = await _personnelFacade.GetTotalPersonnelCountAsync(projectIds);
            var activePersonnel = await _personnelFacade.GetActivePersonnelCountAsync(projectIds);
            var personnelByType = await _personnelFacade.GetPersonnelByTypeAsync(projectIds);
            var totalSalary = await _personnelFacade.GetTotalSalaryAmountAsync(projectIds);
            var averageAttendance = await _personnelFacade.GetAverageAttendanceRateAsync(projectIds);

            var metrics = new PersonnelMetrics(
                totalPersonnel,
                activePersonnel,
                totalPersonnel - activePersonnel,
                personnelByType,
                null, // personnelByProject not needed here
                totalSalary,
                averageAttendance
            );

            _logger.LogDebug("Personnel metrics calculated: {Total} total, {Active} active, {Attendance}% attendance", 
                totalPersonnel, activePersonnel, averageAttendance);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel metrics for projects");
            return PersonnelMetrics.FromCounts(0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds)
    {
        try
        {
            return await _personnelFacade.GetPersonnelByTypeAsync(projectIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel by type for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<int> GetActivePersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            return await _personnelFacade.GetActivePersonnelCountAsync(projectIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active personnel count for projects");
            return 0;
        }
    }

    public async Task<int> GetTotalPersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            return await _personnelFacade.GetTotalPersonnelCountAsync(projectIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total personnel count for projects");
            return 0;
        }
    }

    public async Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            return await _personnelFacade.GetAverageAttendanceRateAsync(projectIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average attendance rate for projects");
            return 0m;
        }
    }

    public async Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds)
    {
        try
        {
            return await _personnelFacade.GetTotalSalaryAmountAsync(projectIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total salary amount for projects");
            return 0m;
        }
    }
}