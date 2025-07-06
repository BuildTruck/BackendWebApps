// BuildTruckBack.Stats.Infrastructure.ACL.UserContextService
namespace BuildTruckBack.Stats.Infrastructure.ACL;

using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Users.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// ACL Service implementation for Users bounded context (only user-related operations)
/// </summary>
public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(
        IUserFacade userFacade,
        ILogger<UserContextService> logger)
    {
        _userFacade = userFacade ?? throw new ArgumentNullException(nameof(userFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsValidManagerAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user != null && user.Role.ToString().ToLower() == "manager" && user.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating manager {UserId}", userId);
            return false;
        }
    }

    public async Task<Dictionary<string, object>?> GetManagerInfoAsync(int managerId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(managerId);
            if (user == null) return null;

            return new Dictionary<string, object>
            {
                ["Id"] = user.Id,
                ["Name"] = user.Name.FirstName,
                ["LastName"] = user.Name.LastName,
                ["FullName"] = user.FullName,
                ["Email"] = user.CorporateEmail.Address,
                ["PersonalEmail"] = user.ContactInfo.PersonalEmailAddress ?? "N/A",
                ["Phone"] = user.ContactInfo.Phone ?? "N/A",
                ["Role"] = user.Role.ToString(),
                ["IsActive"] = user.IsActive,
                ["LastLogin"] = user.LastLogin
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting manager info for {ManagerId}", managerId);
            return null;
        }
    }
}