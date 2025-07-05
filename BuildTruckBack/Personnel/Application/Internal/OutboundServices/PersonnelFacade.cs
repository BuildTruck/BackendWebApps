// BuildTruckBack.Personnel.Application.Internal.OutboundServices.PersonnelFacade
namespace BuildTruckBack.Personnel.Application.Internal.OutboundServices;

using BuildTruckBack.Personnel.Domain.Repositories;
using Microsoft.Extensions.Logging;

/// <summary>
/// Personnel Facade implementation
/// </summary>
public class PersonnelFacade : IPersonnelFacade
{
    private readonly IPersonnelRepository _personnelRepository;
    private readonly ILogger<PersonnelFacade> _logger;

    public PersonnelFacade(
        IPersonnelRepository personnelRepository,
        ILogger<PersonnelFacade> logger)
    {
        _personnelRepository = personnelRepository ?? throw new ArgumentNullException(nameof(personnelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetPersonnelByProjectAsync(int projectId)
    {
        try
        {
            var personnel = await _personnelRepository.FindByProjectIdAsync(projectId);
            return personnel.Where(p => !p.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel for project {ProjectId}", projectId);
            return Enumerable.Empty<Domain.Model.Aggregates.Personnel>();
        }
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetPersonnelByProjectsAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return Enumerable.Empty<Domain.Model.Aggregates.Personnel>();

            var allPersonnel = new List<Domain.Model.Aggregates.Personnel>();

            foreach (var projectId in projectIds)
            {
                var projectPersonnel = await GetPersonnelByProjectAsync(projectId);
                allPersonnel.AddRange(projectPersonnel);
            }

            return allPersonnel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel for multiple projects");
            return Enumerable.Empty<Domain.Model.Aggregates.Personnel>();
        }
    }

    public async Task<int> GetActivePersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return personnel.Count(p => p.IsActive());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active personnel count");
            return 0;
        }
    }

    public async Task<int> GetTotalPersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return personnel.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total personnel count");
            return 0;
        }
    }

    public async Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            
            return personnel
                .GroupBy(p => p.PersonnelType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel by type");
            return new Dictionary<string, int>();
        }
    }

    public async Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            var activePersonnel = personnel.Where(p => p.IsActive());
            
            return activePersonnel.Sum(p => p.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total salary amount");
            return 0m;
        }
    }

    public async Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            var activePersonnel = personnel.Where(p => p.IsActive()).ToList();

            if (!activePersonnel.Any()) return 0m;

            // Calculate attendance rate based on worked days vs total days
            var attendanceRates = activePersonnel
                .Where(p => p.TotalDays > 0)
                .Select(p => (decimal)(p.WorkedDays + p.CompensatoryDays) / p.TotalDays * 100);

            return attendanceRates.Any() ? Math.Round(attendanceRates.Average(), 2) : 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average attendance rate");
            return 0m;
        }
    }

    public async Task<Dictionary<string, object>> GetPersonnelStatisticsAsync(List<int> projectIds, int year, int month)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            
            // Usar el método estático existente de ExternalPersonnelService
            return ExternalPersonnelService.GetMonthStatistics(personnel, year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personnel statistics");
            return new Dictionary<string, object>();
        }
    }
}