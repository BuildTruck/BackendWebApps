// BuildTruckBack.Stats.Application.ACL.Services.IPersonnelContextService
namespace BuildTruckBack.Stats.Application.ACL.Services;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// ACL Service interface for Personnel bounded context
/// </summary>
public interface IPersonnelContextService
{
    /// <summary>
    /// Get personnel metrics for manager's projects within a period
    /// </summary>
    Task<PersonnelMetrics> GetPersonnelMetricsAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get personnel counts by type for projects
    /// </summary>
    Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds);

    /// <summary>
    /// Get active personnel count for projects
    /// </summary>
    Task<int> GetActivePersonnelCountAsync(List<int> projectIds);

    /// <summary>
    /// Get total personnel count for projects
    /// </summary>
    Task<int> GetTotalPersonnelCountAsync(List<int> projectIds);

    /// <summary>
    /// Get average attendance rate for projects within period
    /// </summary>
    Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get total salary amount for projects
    /// </summary>
    Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds);
}