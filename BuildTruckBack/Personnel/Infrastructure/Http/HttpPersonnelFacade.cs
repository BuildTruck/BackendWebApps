using System.Net.Http.Json;
using BuildTruckBack.Personnel.Application.Internal.OutboundServices;

namespace BuildTruckBack.Personnel.Infrastructure.Http;

public class HttpPersonnelFacade(IHttpClientFactory httpClientFactory) : IPersonnelFacade
{
    private HttpClient Client => httpClientFactory.CreateClient("PersonnelService");

    public async Task<IEnumerable<PersonnelInfo>> GetPersonnelByProjectAsync(int projectId)
    {
        try
        {
            return await Client.GetFromJsonAsync<List<PersonnelInfo>>(
                $"/api/v1/personnel?projectId={projectId}") ?? [];
        }
        catch { return []; }
    }

    public async Task<IEnumerable<PersonnelInfo>> GetPersonnelByProjectsAsync(List<int> projectIds)
    {
        if (!projectIds.Any()) return [];

        var allPersonnel = new List<PersonnelInfo>();
        foreach (var projectId in projectIds)
        {
            var projectPersonnel = await GetPersonnelByProjectAsync(projectId);
            allPersonnel.AddRange(projectPersonnel);
        }
        return allPersonnel;
    }

    public async Task<int> GetActivePersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return personnel.Count(p => p.IsActive());
        }
        catch { return 0; }
    }

    public async Task<int> GetTotalPersonnelCountAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return personnel.Count();
        }
        catch { return 0; }
    }

    public async Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return personnel
                .GroupBy(p => p.PersonnelType)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch { return new Dictionary<string, int>(); }
    }

    public async Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return personnel.Where(p => p.IsActive()).Sum(p => p.TotalAmount);
        }
        catch { return 0m; }
    }

    public async Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            var active = personnel.Where(p => p.IsActive()).ToList();
            if (!active.Any()) return 0m;

            var rates = active
                .Where(p => p.TotalDays > 0)
                .Select(p => (decimal)(p.WorkedDays + p.CompensatoryDays) / p.TotalDays * 100);

            return rates.Any() ? Math.Round(rates.Average(), 2) : 0m;
        }
        catch { return 0m; }
    }

    public async Task<Dictionary<string, object>> GetPersonnelStatisticsAsync(List<int> projectIds, int year, int month)
    {
        try
        {
            var personnel = await GetPersonnelByProjectsAsync(projectIds);
            return new Dictionary<string, object>
            {
                ["totalPersonnel"] = personnel.Count(),
                ["activePersonnel"] = personnel.Count(p => p.IsActive()),
                ["totalWorkedDays"] = personnel.Sum(p => p.WorkedDays),
                ["totalAbsences"] = personnel.Sum(p => p.Absences),
                ["totalAmount"] = personnel.Sum(p => p.TotalAmount),
                ["averageAttendance"] = personnel.Any()
                    ? personnel.Average(p => p.WorkedDays + p.CompensatoryDays)
                    : 0
            };
        }
        catch { return new Dictionary<string, object>(); }
    }
}
