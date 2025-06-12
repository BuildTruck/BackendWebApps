namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;

/// <summary>
/// Generic email service interface for sending notifications across all bounded contexts
/// </summary>
public interface IGenericEmailService
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

    /// <summary>
    /// Send email with custom subject and body (for ACL usage)
    /// </summary>
    /// <param name="email">Destination email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">Email HTML body</param>
    /// <returns>Task</returns>
    Task SendEmailAsync(string email, string subject, string htmlBody);
}