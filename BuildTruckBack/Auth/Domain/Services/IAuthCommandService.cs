using BuildTruckBack.Auth.Domain.Model.Commands;
using BuildTruckBack.Auth.Domain.Model.ValueObjects;

namespace BuildTruckBack.Auth.Domain.Services;

/// <summary>
/// Auth Command Service Interface
/// </summary>
/// <remarks>
/// Domain service interface for authentication commands
/// </remarks>
public interface IAuthCommandService
{
    /// <summary>
    /// Handle user sign-in command
    /// </summary>
    /// <param name="command">Sign-in command with credentials and audit info</param>
    /// <returns>AuthToken if successful, null if authentication fails</returns>
    Task<AuthToken?> HandleSignInAsync(SignInCommand command);
}