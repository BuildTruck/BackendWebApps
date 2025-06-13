using BuildTruckBack.Auth.Domain.Model.ValueObjects;

namespace BuildTruckBack.Auth.Application.ACL.Services;

/// <summary>
/// ACL Service interface for communication with Users Context
/// </summary>
/// <remarks>
/// Anti-Corruption Layer that isolates Auth Context from Users Context implementation details
/// </remarks>
public interface IUserContextService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <param name="password">Plain text password</param>
    /// <returns>AuthenticatedUser if credentials are valid, null otherwise</returns>
    Task<AuthenticatedUser?> AuthenticateUserAsync(string email, string password);

    /// <summary>
    /// Get user information by user ID
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>AuthenticatedUser if found, null otherwise</returns>
    Task<AuthenticatedUser?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>True if updated successfully, false otherwise</returns>
    Task<bool> UpdateUserLastLoginAsync(int userId);

    /// <summary>
    /// Check if user is active and can authenticate
    /// </summary>
    /// <param name="email">Corporate email address</param>
    /// <returns>True if user is active, false otherwise</returns>
    Task<bool> IsUserActiveAsync(string email);

    /// <summary>
    /// Get user profile image URL with specified size
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="size">Image size in pixels (default: 200)</param>
    /// <returns>Profile image URL or null if not available</returns>
    Task<string?> GetUserProfileImageUrlAsync(int userId, int size = 200);
}