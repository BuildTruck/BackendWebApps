namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IUserContextService
{
    Task<bool> UserExistsAsync(int userId);
    Task<string> GetUserNameAsync(int userId);
    Task<string> GetUserEmailAsync(int userId);
    Task<string> GetUserRoleAsync(int userId);
    Task<bool> IsUserActiveAsync(int userId);
    Task<IEnumerable<int>> GetAdminUsersAsync();
    Task<IEnumerable<int>> GetManagerUsersAsync();
    Task<IEnumerable<int>> GetSupervisorUsersAsync();
}