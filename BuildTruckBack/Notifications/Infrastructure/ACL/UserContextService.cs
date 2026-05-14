using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Users.Application.Internal.OutboundServices;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;
    private readonly IHttpClientFactory _httpClientFactory;

    public UserContextService(IUserFacade userFacade, IHttpClientFactory httpClientFactory)
    {
        _userFacade = userFacade;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user != null;
    }

    public async Task<string> GetUserNameAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.FullName ?? string.Empty;
    }

    public async Task<string> GetUserEmailAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.PersonalEmail ?? user?.Email ?? string.Empty;
    }

    public async Task<string> GetUserRoleAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.Role ?? string.Empty;
    }

    public async Task<bool> IsUserActiveAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.IsActive ?? false;
    }

    public async Task<IEnumerable<int>> GetAdminUsersAsync() => await GetUsersByRoleAsync("Admin");
    public async Task<IEnumerable<int>> GetManagerUsersAsync() => await GetUsersByRoleAsync("Manager");
    public async Task<IEnumerable<int>> GetSupervisorUsersAsync() => await GetUsersByRoleAsync("Supervisor");

    private async Task<IEnumerable<int>> GetUsersByRoleAsync(string role)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync(
                $"/api/v1/users?role={Uri.EscapeDataString(role)}&activeOnly=true");

            if (!response.IsSuccessStatusCode) return [];

            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return users?.Select(u => u.Id) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
