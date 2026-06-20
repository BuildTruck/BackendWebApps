using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Configurations.Application.Internal.OutboundServices;

namespace BuildTruckBack.Configurations.Infrastructure.Http;

public class HttpConfigurationFacade : IConfigurationFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpConfigurationFacade> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpConfigurationFacade(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpConfigurationFacade> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ConfigurationDto?> GetConfigurationSettingsByUserIdAsync(int userId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ConfigurationService");
            var response = await client.GetAsync($"/api/v1/configuration-settings/user/{userId}");
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ConfigurationDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error calling ConfigurationService for user {UserId}",
                userId);
            return null;
        }
    }
}
