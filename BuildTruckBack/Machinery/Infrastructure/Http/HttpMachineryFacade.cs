using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Machinery.Application.Internal.OutboundServices;

namespace BuildTruckBack.Machinery.Infrastructure.Http;

public class HttpMachineryFacade : IMachineryFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpMachineryFacade> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpMachineryFacade(IHttpClientFactory httpClientFactory, ILogger<HttpMachineryFacade> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("MachineryService");

    public async Task<IEnumerable<MachineryDto>> GetByProjectIdAsync(int projectId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/machinery?projectId={projectId}");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<MachineryDto>();
            return await response.Content.ReadFromJsonAsync<IEnumerable<MachineryDto>>(JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MachineryService GetByProjectIdAsync for project {ProjectId}", projectId);
            return Enumerable.Empty<MachineryDto>();
        }
    }

    public async Task<MachineryDto?> GetByIdAsync(int machineryId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/machinery/{machineryId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<MachineryDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MachineryService GetByIdAsync for machinery {MachineryId}", machineryId);
            return null;
        }
    }

    public async Task<MachineryDto> CreateAsync(CreateMachineryDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/v1/machinery", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MachineryDto>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MachineryService CreateAsync");
            throw;
        }
    }

    public async Task<MachineryDto> UpdateAsync(int id, UpdateMachineryDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"/api/v1/machinery/{id}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MachineryDto>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MachineryService UpdateAsync for machinery {MachineryId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int machineryId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"/api/v1/machinery/{machineryId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MachineryService DeleteAsync for machinery {MachineryId}", machineryId);
            return false;
        }
    }
}
