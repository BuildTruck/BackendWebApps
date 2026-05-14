using System.Net.Http.Json;
using BuildTruckBack.Projects.Application.Internal.OutboundServices;

namespace BuildTruckBack.Projects.Infrastructure.Http;

public class HttpProjectFacade(IHttpClientFactory httpClientFactory) : IProjectFacade
{
    private HttpClient Client => httpClientFactory.CreateClient("ProjectService");

    public async Task<bool> ExistsByIdAsync(int projectId)
    {
        try
        {
            var response = await Client.GetFromJsonAsync<ExistsResponse>($"/api/v1/projects/exists/{projectId}");
            return response?.Exists ?? false;
        }
        catch { return false; }
    }

    public async Task<ProjectInfo?> GetProjectByIdAsync(int projectId)
    {
        try
        {
            return await Client.GetFromJsonAsync<ProjectInfo>($"/api/v1/projects/{projectId}");
        }
        catch { return null; }
    }

    public async Task<bool> UserHasAccessToProjectAsync(int userId, int projectId)
    {
        try
        {
            var response = await Client.GetFromJsonAsync<AccessResponse>(
                $"/api/v1/projects/user-access?userId={userId}&projectId={projectId}");
            return response?.HasAccess ?? false;
        }
        catch { return false; }
    }

    public async Task<List<ProjectInfo>> GetProjectsByUserAsync(int userId)
    {
        try
        {
            return await Client.GetFromJsonAsync<List<ProjectInfo>>($"/api/v1/projects/by-user/{userId}")
                   ?? new List<ProjectInfo>();
        }
        catch { return new List<ProjectInfo>(); }
    }

    public async Task<List<ProjectInfo>> GetProjectsByManagerAsync(int managerId)
    {
        try
        {
            return await Client.GetFromJsonAsync<List<ProjectInfo>>($"/api/v1/projects/internal/by-manager/{managerId}")
                   ?? new List<ProjectInfo>();
        }
        catch { return new List<ProjectInfo>(); }
    }

    public async Task<List<ProjectInfo>> GetProjectsBySupervisorAsync(int supervisorId)
    {
        try
        {
            return await Client.GetFromJsonAsync<List<ProjectInfo>>($"/api/v1/projects/internal/by-supervisor/{supervisorId}")
                   ?? new List<ProjectInfo>();
        }
        catch { return new List<ProjectInfo>(); }
    }

    public async Task<int> GetProjectCountByStateAsync(string state)
    {
        try
        {
            var response = await Client.GetFromJsonAsync<CountResponse>(
                $"/api/v1/projects/count?state={Uri.EscapeDataString(state)}");
            return response?.Count ?? 0;
        }
        catch { return 0; }
    }

    public async Task<int> GetActiveProjectsCountAsync()
    {
        try
        {
            var response = await Client.GetFromJsonAsync<CountResponse>("/api/v1/projects/active-count");
            return response?.Count ?? 0;
        }
        catch { return 0; }
    }

    public async Task<List<ProjectInfo>> GetProjectsByStateAsync(string state)
    {
        try
        {
            return await Client.GetFromJsonAsync<List<ProjectInfo>>(
                $"/api/v1/projects/by-state?state={Uri.EscapeDataString(state)}")
                   ?? new List<ProjectInfo>();
        }
        catch { return new List<ProjectInfo>(); }
    }

    public async Task<List<ProjectInfo>> GetAllProjectsAsync()
    {
        try
        {
            return await Client.GetFromJsonAsync<List<ProjectInfo>>("/api/v1/projects/all")
                   ?? new List<ProjectInfo>();
        }
        catch { return new List<ProjectInfo>(); }
    }

    private record ExistsResponse(bool Exists);
    private record AccessResponse(bool HasAccess);
    private record CountResponse(int Count);
}
