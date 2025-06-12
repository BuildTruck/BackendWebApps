namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;

/// <summary>
/// Email service interface for sending notifications
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send welcome email with temporal credentials
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="fullName">User full name</param>
    /// <param name="temporalPassword">Temporal password</param>
    /// <returns>Task</returns>
    Task SendWelcomeEmailAsync(string email, string fullName, string temporalPassword);

    /// <summary>
    /// Send password changed notification
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="fullName">User full name</param>
    /// <returns>Task</returns>
    Task SendPasswordChangedNotificationAsync(string email, string fullName);
}