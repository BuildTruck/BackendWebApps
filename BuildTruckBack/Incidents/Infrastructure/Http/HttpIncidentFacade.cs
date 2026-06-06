using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Incidents.Application.Internal.OutboundServices;

namespace BuildTruckBack.Incidents.Infrastructure.Http;

public class HttpIncidentFacade : IIncidentFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpIncidentFacade> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpIncidentFacade(IHttpClientFactory httpClientFactory, ILogger<HttpIncidentFacade> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("IncidentService");

    public async Task<IEnumerable<IncidentDto>> GetByProjectIdAsync(int projectId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/incidents?projectId={projectId}");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<IncidentDto>();
            return await response.Content.ReadFromJsonAsync<IEnumerable<IncidentDto>>(JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IncidentService GetByProjectIdAsync for project {ProjectId}", projectId);
            return Enumerable.Empty<IncidentDto>();
        }
    }

    public async Task<IncidentDto?> GetByIdAsync(int incidentId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/incidents/{incidentId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IncidentService GetByIdAsync for incident {IncidentId}", incidentId);
            return null;
        }
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/v1/incidents", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IncidentService CreateAsync");
            throw;
        }
    }

    public async Task<IncidentDto> UpdateAsync(int id, UpdateIncidentDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"/api/v1/incidents/{id}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IncidentService UpdateAsync for incident {IncidentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int incidentId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"/api/v1/incidents/{incidentId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling IncidentService DeleteAsync for incident {IncidentId}", incidentId);
            return false;
        }
    }
}
