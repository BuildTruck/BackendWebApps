// BuildTruckBack.Stats.Application.ACL.Services.IUserContextService
namespace BuildTruckBack.Stats.Application.ACL.Services;

/// <summary>
/// ACL Service interface for Users bounded context
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Verify if user is a valid manager
    /// </summary>
    Task<bool> IsValidManagerAsync(int userId);

    /// <summary>
    /// Get manager information
    /// </summary>
    Task<Dictionary<string, object>?> GetManagerInfoAsync(int managerId);
}