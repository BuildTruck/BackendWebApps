// BuildTruckBack.Personnel.Application.Internal.OutboundServices.IPersonnelFacade
namespace BuildTruckBack.Personnel.Application.Internal.OutboundServices;

/// <summary>
/// Facade for Personnel bounded context operations
/// </summary>
public interface IPersonnelFacade
{
    /// <summary>
    /// Get all personnel for a specific project
    /// </summary>
    Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetPersonnelByProjectAsync(int projectId);

    /// <summary>
    /// Get personnel for multiple projects
    /// </summary>
    Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetPersonnelByProjectsAsync(List<int> projectIds);

    /// <summary>
    /// Get active personnel count for projects
    /// </summary>
    Task<int> GetActivePersonnelCountAsync(List<int> projectIds);

    /// <summary>
    /// Get total personnel count for projects
    /// </summary>
    Task<int> GetTotalPersonnelCountAsync(List<int> projectIds);

    /// <summary>
    /// Get personnel grouped by type for projects
    /// </summary>
    Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds);

    /// <summary>
    /// Get total salary amount for projects
    /// </summary>
    Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds);

    /// <summary>
    /// Get average attendance rate for projects
    /// </summary>
    Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds);

    /// <summary>
    /// Get personnel statistics for a period
    /// </summary>
    Task<Dictionary<string, object>> GetPersonnelStatisticsAsync(List<int> projectIds, int year, int month);
}