namespace BuildTruckBack.Stats.Infrastructure.Http;

using BuildTruckBack.Stats.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// HTTP facade implementation for Stats operations.
/// All Stats logic has been migrated to BuildTruckStatsService (port 5002).
/// This facade delegates to that microservice when needed.
/// </summary>
public class HttpStatsFacade : IStatsFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpStatsFacade> _logger;
    private System.Net.Http.HttpClient Client => _httpClientFactory.CreateClient("StatsService");

    public HttpStatsFacade(IHttpClientFactory httpClientFactory, ILogger<HttpStatsFacade> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task TriggerCalculationAsync(int managerId)
    {
        try
        {
            _logger.LogInformation("Triggering stats calculation for manager {ManagerId}", managerId);
            var response = await Client.PostAsync($"/api/v1/stats/manager/{managerId}/calculate", null);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Stats calculation returned {StatusCode} for manager {ManagerId}",
                    response.StatusCode, managerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering stats calculation for manager {ManagerId}", managerId);
        }
    }
}
