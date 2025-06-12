using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.ValueObjects;
using BuildTruckBack.Users.Domain.Repositories;
using BuildTruckBack.Users.Application.ACL.Services;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Users.Application.Internal.OutboundServices;

/// <summary>
/// User Facade Implementation
/// Provides a simplified interface for other bounded contexts to interact with Users
/// </summary>
public class UserFacade : IUserFacade
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IImageService _imageService;
    private readonly ILogger<UserFacade> _logger;

    public UserFacade(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IImageService imageService,
        ILogger<UserFacade> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Verify user credentials for authentication
    /// </summary>
    public async Task<User?> VerifyCredentialsAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("🔐 Verifying credentials for email: {Email}", email);

            // ✅ Find user by email using Value Object
            var emailAddress = new EmailAddress(email);
            var user = await _userRepository.FindByEmailAsync(emailAddress);

            if (user == null)
            {
                _logger.LogWarning("❌ User not found for email: {Email}", email);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("❌ User is inactive: {Email}", email);
                return null;
            }

            // ✅ Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("❌ Invalid password for user: {Email}", email);
                return null;
            }

            _logger.LogInformation("✅ Credentials verified successfully for user: {Email}", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error verifying credentials for email: {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Find user by corporate email
    /// </summary>
    public async Task<User?> FindByEmailAsync(string email)
    {
        try
        {
            var emailAddress = new EmailAddress(email);
            return await _userRepository.FindByEmailAsync(emailAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding user by email: {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Find user by ID
    /// </summary>
    public async Task<User?> FindByIdAsync(int userId)
    {
        try
        {
            return await _userRepository.FindByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding user by ID: {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        try
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("❌ User not found for last login update: {UserId}", userId);
                return false;
            }

            user.UpdateLastLogin();
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("✅ Last login updated for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Check if user exists and is active
    /// </summary>
    public async Task<bool> IsActiveUserAsync(string email)
    {
        try
        {
            var user = await FindByEmailAsync(email);
            return user != null && user.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking if user is active: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Send password reset email (for Auth context)
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            var user = await FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("❌ User not found for password reset: {Email}", email);
                return false;
            }

            // ✅ Use ACL Email Service to send reset email
            await _emailService.SendPasswordResetEmailAsync(user, resetToken);
            
            _logger.LogInformation("✅ Password reset email sent to: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error sending password reset email: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Get user's profile image URL for display
    /// </summary>
    public async Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200)
    {
        try
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("❌ User not found for profile image: {UserId}", userId);
                return GenerateDefaultAvatar(userId, size);
            }

            // ✅ Use ACL Image Service to get optimized URL
            return _imageService.GetUserProfileImageUrl(user, size);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting profile image for user: {UserId}", userId);
            return GenerateDefaultAvatar(userId, size);
        }
    }

    /// <summary>
    /// Generate default avatar for unknown users
    /// </summary>
    private static string GenerateDefaultAvatar(int userId, int size)
    {
        return $"https://via.placeholder.com/{size}x{size}/f97316/ffffff?text=U{userId}";
    }
}