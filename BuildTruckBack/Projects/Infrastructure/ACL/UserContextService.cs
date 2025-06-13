using BuildTruckBack.Projects.Application.ACL.Services;
using BuildTruckBack.Users.Application.Internal.OutboundServices;

namespace BuildTruckBack.Projects.Infrastructure.ACL;

/// <summary>
/// User Context Service Implementation for Projects Context
/// </summary>
/// <remarks>
/// ACL adapter that wraps the Users Context UserFacade
/// for Projects Context specific needs
/// </remarks>
public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(
        IUserFacade userFacade,
        ILogger<UserContextService> logger)
    {
        _userFacade = userFacade;
        _logger = logger;
    }

    public async Task<UserDto?> FindByIdAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                ProfileImageUrl = await _userFacade.GetUserProfileImageUrlAsync(userId),
                CurrentProjectId = null, // TODO: Implement when User entity has CurrentProjectId
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to find user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> IsActiveUserAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user?.IsActive ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to check if user is active: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsManagerAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user?.Role.ToString() == "Manager";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to check if user is Manager: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsSupervisorAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user?.Role.ToString() == "Supervisor";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to check if user is Supervisor: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(userId);
            return user?.Role.ToString() == "Admin";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to check if user is Admin: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsSupervisorAvailableAsync(int supervisorId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(supervisorId);
            if (user == null || user.Role.ToString() != "Supervisor" || !user.IsActive)
                return false;

            // TODO: Check if supervisor has any active projects assigned
            // This would require access to ProjectRepository to check:
            // SELECT COUNT(*) FROM Projects WHERE SupervisorId = supervisorId AND State = 'Activo'
            
            _logger.LogInformation("✅ Supervisor {SupervisorId} availability check - assuming available for now", supervisorId);
            return true; // Temporary - assume all supervisors are available
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to check if supervisor is available: {SupervisorId}", supervisorId);
            return false;
        }
    }

    public async Task<int?> FindAvailableSupervisorAsync()
    {
        try
        {
            // This would need to be implemented in UserFacade if not available
            // For now, we'll implement a basic version here
            
            // Note: This is a simplified implementation
            // In a real scenario, you might want to add this method to UserFacade
            
            _logger.LogInformation("🔍 Searching for available supervisor...");
            
            // Since we don't have a direct method in UserFacade for this,
            // we would need to extend the UserFacade interface
            // For now, return null to indicate no available supervisor
            
            _logger.LogWarning("⚠️ FindAvailableSupervisorAsync not fully implemented - needs UserFacade extension");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to find available supervisor");
            return null;
        }
    }

    public async Task<bool> AssignSupervisorToProjectAsync(int supervisorId, int projectId)
    {
        try
        {
            // Just validate supervisor exists and is active
            var user = await _userFacade.FindByIdAsync(supervisorId);
            if (user == null || user.Role.ToString() != "Supervisor" || !user.IsActive)
            {
                _logger.LogWarning("⚠️ Supervisor {SupervisorId} is not valid for assignment", supervisorId);
                return false;
            }

            // The actual assignment is handled by Project aggregate
            // Project.SupervisorId = supervisorId is set by the domain
            // No need to update User entity
            
            _logger.LogInformation("✅ Supervisor {SupervisorId} validated for project {ProjectId} assignment", supervisorId, projectId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to validate supervisor {SupervisorId} for project {ProjectId}", supervisorId, projectId);
            return false;
        }
    }

    public async Task<bool> ReleaseSupervisorFromProjectAsync(int supervisorId)
    {
        try
        {
            var user = await _userFacade.FindByIdAsync(supervisorId);
            if (user == null || user.Role.ToString() != "Supervisor")
            {
                _logger.LogWarning("⚠️ User {SupervisorId} is not a supervisor", supervisorId);
                return false;
            }

            // The actual release is handled by Project aggregate
            // Project.SupervisorId = null is set by the domain
            // No need to update User entity
            
            _logger.LogInformation("✅ Supervisor {SupervisorId} validated for release from project", supervisorId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to validate supervisor {SupervisorId} for release", supervisorId);
            return false;
        }
    }

    public async Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200)
    {
        try
        {
            return await _userFacade.GetUserProfileImageUrlAsync(userId, size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get profile image URL for user: {UserId}", userId);
            return string.Empty;
        }
    }
}