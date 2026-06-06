using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Documentation.Application.Internal.OutboundServices;

namespace BuildTruckBack.Documentation.Infrastructure.Http;

public class HttpDocumentationFacade : IDocumentationFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpDocumentationFacade> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpDocumentationFacade(IHttpClientFactory httpClientFactory, ILogger<HttpDocumentationFacade> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("DocumentationService");

    public async Task<IEnumerable<DocumentationDto>> GetByProjectIdAsync(int projectId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/documentation?projectId={projectId}");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<DocumentationDto>();
            return await response.Content.ReadFromJsonAsync<IEnumerable<DocumentationDto>>(JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DocumentationService GetByProjectIdAsync for project {ProjectId}", projectId);
            return Enumerable.Empty<DocumentationDto>();
        }
    }

    public async Task<DocumentationDto?> GetByIdAsync(int documentationId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/documentation/{documentationId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<DocumentationDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DocumentationService GetByIdAsync for documentation {DocumentationId}", documentationId);
            return null;
        }
    }

    public async Task<DocumentationDto> CreateAsync(CreateDocumentationDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/v1/documentation", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DocumentationDto>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DocumentationService CreateAsync");
            throw;
        }
    }

    public async Task<DocumentationDto> UpdateAsync(int id, UpdateDocumentationDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"/api/v1/documentation/{id}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DocumentationDto>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to parse response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DocumentationService UpdateAsync for documentation {DocumentationId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int documentationId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"/api/v1/documentation/{documentationId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DocumentationService DeleteAsync for documentation {DocumentationId}", documentationId);
            return false;
        }
    }
}
