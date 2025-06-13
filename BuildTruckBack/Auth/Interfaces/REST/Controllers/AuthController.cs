using System.Security.Claims;
using BuildTruckBack.Auth.Domain.Model.Queries;
using BuildTruckBack.Auth.Domain.Services;
using BuildTruckBack.Auth.Interfaces.REST.Resources;
using BuildTruckBack.Auth.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildTruckBack.Auth.Interfaces.REST.Controllers;

/// <summary>
/// Authentication Controller
/// </summary>
/// <remarks>
/// Handles authentication endpoints for the Auth bounded context
/// </remarks>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthCommandService _authCommandService;
    private readonly IAuthQueryService _authQueryService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthCommandService authCommandService,
        IAuthQueryService authQueryService,
        ILogger<AuthController> logger)
    {
        _authCommandService = authCommandService ?? throw new ArgumentNullException(nameof(authCommandService));
        _authQueryService = authQueryService ?? throw new ArgumentNullException(nameof(authQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// User sign-in
    /// </summary>
    /// <param name="resource">Sign-in credentials</param>
    /// <returns>Authentication token and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        try
        {
            _logger.LogInformation("Sign-in attempt for email: {Email}", resource.Email);

            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid sign-in request for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Transform to domain command
            var command = AuthResourceAssembler.ToCommand(resource, ipAddress, userAgent);

            // Execute sign-in
            var authToken = await _authCommandService.HandleSignInAsync(command);

            if (authToken == null)
            {
                _logger.LogWarning("Sign-in failed for email: {Email}", resource.Email);
                return Unauthorized(AuthResourceAssembler.ToErrorResponse("Invalid credentials"));
            }

            _logger.LogInformation("Sign-in successful for user: {UserId}", authToken.GetUserId());

            // Transform to response
            var response = AuthResourceAssembler.ToSignInResponse(authToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-in for email: {Email}", resource.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                AuthResourceAssembler.ToErrorResponse("An error occurred during authentication"));
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">Not authenticated or token expired</response>
    /// <response code="404">User not found or inactive</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatedUserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Get user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized(AuthResourceAssembler.ToUnauthorizedResponse("Invalid token"));
            }

            _logger.LogDebug("Getting current user information for user: {UserId}", userId);

            // Get client information
            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Create query
            var query = new GetCurrentUserQuery(userId, ipAddress, userAgent);

            // Execute query
            var user = await _authQueryService.HandleGetCurrentUserAsync(query);

            if (user == null)
            {
                _logger.LogWarning("User not found or inactive for ID: {UserId}", userId);
                return NotFound(AuthResourceAssembler.ToErrorResponse("User not found or inactive"));
            }

            _logger.LogDebug("Current user information retrieved for user: {UserId}", userId);

            // Transform to response
            var response = AuthResourceAssembler.ToResource(user);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred while retrieving user information"));
        }
    }

    #region Private Helper Methods

    private string? GetClientIpAddress()
    {
        try
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetClientUserAgent()
    {
        try
        {
            return HttpContext.Request.Headers.UserAgent.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private int GetUserIdFromClaims()
    {
        try
        {
            var userIdClaim = HttpContext.User.FindFirst("user_id")?.Value ??
                             HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
        catch
        {
            return 0;
        }
    }

    #endregion
}