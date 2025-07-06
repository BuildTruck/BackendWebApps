using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Users.Application.Internal.OutboundServices;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;

    public UserContextService(IUserFacade userFacade)
    {
        _userFacade = userFacade;
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
        return user?.PersonalEmail ?? string.Empty;
    }

    public async Task<string> GetUserRoleAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.Role.Role ?? string.Empty;
    }

    public async Task<bool> IsUserActiveAsync(int userId)
    {
        var user = await _userFacade.FindByIdAsync(userId);
        return user?.IsActive ?? false;
    }

    public async Task<IEnumerable<int>> GetAdminUsersAsync()
    {
        return await GetUsersByRoleAsync("Admin");
    }

    public async Task<IEnumerable<int>> GetManagerUsersAsync()
    {
        return await GetUsersByRoleAsync("Manager");
    }

    public async Task<IEnumerable<int>> GetSupervisorUsersAsync()
    {
        return await GetUsersByRoleAsync("Supervisor");
    }

    private async Task<IEnumerable<int>> GetUsersByRoleAsync(string role)
    {
        var userIds = new List<int>();
        var maxUserId = 1000;

        for (int i = 1; i <= maxUserId; i++)
        {
            try
            {
                var userRole = await GetUserRoleAsync(i);
                if (userRole == role && await IsUserActiveAsync(i))
                {
                    userIds.Add(i);
                }
            }
            catch
            {
                // Continue if user doesn't exist
            }
        }

        return userIds;
    }
}