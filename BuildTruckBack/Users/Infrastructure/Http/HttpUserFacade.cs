using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Users.Application.Internal.OutboundServices;

namespace BuildTruckBack.Users.Infrastructure.Http;

public class HttpUserFacade : IUserFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpUserFacade> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpUserFacade(IHttpClientFactory httpClientFactory, ILogger<HttpUserFacade> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("UserService");

    public async Task<UserDto?> FindByIdAsync(int userId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/users/{userId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService FindByIdAsync for user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> FindByEmailAsync(string email)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/users?email={Uri.EscapeDataString(email)}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService FindByEmailAsync for {Email}", email);
            return null;
        }
    }

    public async Task<UserDto?> VerifyCredentialsAsync(string email, string password)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/v1/auth/internal/verify-credentials",
                new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService VerifyCredentialsAsync for {Email}", email);
            return null;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsync($"/api/v1/users/{userId}/last-login", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService UpdateLastLoginAsync for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsActiveUserAsync(string email)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/users/is-active?email={Uri.EscapeDataString(email)}");
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            return result.GetProperty("isActive").GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService IsActiveUserAsync for {Email}", email);
            return false;
        }
    }

    public async Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/users/{userId}/profile-image-url?size={size}");
            if (!response.IsSuccessStatusCode) return string.Empty;
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            return result.GetProperty("url").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService GetUserProfileImageUrlAsync for user {UserId}", userId);
            return string.Empty;
        }
    }

    public async Task SendPasswordResetEmailAsync(int userId, string email, string fullName, string resetToken)
    {
        try
        {
            var client = CreateClient();
            await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService SendPasswordResetEmailAsync for {Email}", email);
            throw;
        }
    }

    public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password",
                new { UserId = userId, NewPassword = newPassword });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UserService ResetUserPasswordAsync for user {UserId}", userId);
            return false;
        }
    }
}
