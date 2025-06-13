using BuildTruckBack.Auth.Application.ACL.Services;
using BuildTruckBack.Auth.Domain.Model.Commands;
using BuildTruckBack.Auth.Domain.Model.ValueObjects;
using BuildTruckBack.Auth.Domain.Services;
using BuildTruckBack.Auth.Infrastructure.Tokens.JWT.Services;
using Microsoft.Extensions.Logging;

namespace BuildTruckBack.Auth.Application.Internal.CommandServices;

/// <summary>
/// Auth Command Service Implementation
/// </summary>
/// <remarks>
/// Application service that orchestrates authentication commands
/// </remarks>
public class AuthCommandService : IAuthCommandService
{
    private readonly IUserContextService _userContextService;
    private readonly TokenService _tokenService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthCommandService> _logger;

    public AuthCommandService(
        IUserContextService userContextService,
        TokenService tokenService,
        IServiceProvider serviceProvider,
        ILogger<AuthCommandService> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthToken?> HandleSignInAsync(SignInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing sign-in command: {AuditInfo}", command.GetAuditString());

            // Validate command
            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid sign-in command for email: {Email}", command.Email);
                return null;
            }

            // Authenticate user via ACL
            var authenticatedUser = await _userContextService.AuthenticateUserAsync(
                command.Email, 
                command.Password);

            if (authenticatedUser == null)
            {
                _logger.LogWarning("Authentication failed for email: {Email}", command.Email);
                return null;
            }

            _logger.LogInformation("User authenticated successfully: {UserId} - {Email}", 
                authenticatedUser.Id, authenticatedUser.Email);

            // Generate JWT token
            var authToken = _tokenService.GenerateToken(authenticatedUser);

            // Update last login timestamp in background with proper scope
            _ = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
                try
                {
                    await scopedUserService.UpdateUserLastLoginAsync(authenticatedUser.Id);
                    _logger.LogDebug("Last login updated for user: {UserId}", authenticatedUser.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update last login for user: {UserId}", authenticatedUser.Id);
                }
            });

            _logger.LogInformation("Sign-in completed successfully for user: {UserId}", authenticatedUser.Id);
            return authToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sign-in command for email: {Email}", command.Email);
            return null;
        }
    }
}