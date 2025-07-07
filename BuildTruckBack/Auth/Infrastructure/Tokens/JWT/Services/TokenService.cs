using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuildTruckBack.Auth.Domain.Model.ValueObjects;
using BuildTruckBack.Auth.Infrastructure.Tokens.JWT.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildTruckBack.Auth.Infrastructure.Tokens.JWT.Services;

/// <summary>
/// JWT Token Service for generating and validating tokens
/// </summary>
/// <remarks>
/// Handles JWT token creation and validation using configured settings
/// </remarks>
public class TokenService
{
    private readonly TokenSettings _tokenSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SigningCredentials _signingCredentials;

    public TokenService(IOptions<TokenSettings> tokenSettings, ILogger<TokenService> logger)
    {
        _tokenSettings = tokenSettings?.Value ?? throw new ArgumentNullException(nameof(tokenSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Validate token settings on startup
        if (!_tokenSettings.IsValid())
        {
            var errors = _tokenSettings.GetValidationErrors();
            throw new InvalidOperationException($"Invalid TokenSettings: {string.Join(", ", errors)}");
        }

        _tokenHandler = new JwtSecurityTokenHandler();
        
        // Configure signing credentials
        var key = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);
        _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        
        _logger.LogInformation("TokenService initialized with {ExpirationHours}h expiration", _tokenSettings.ExpirationHours);
    }

    /// <summary>
    /// Generate JWT token for authenticated user
    /// </summary>
    /// <param name="user">Authenticated user</param>
    /// <returns>AuthToken with JWT and expiration info</returns>
    public AuthToken GenerateToken(AuthenticatedUser user)
    {
        try
        {
            _logger.LogDebug("Generating token for user: {UserId} - {Email}", user.Id, user.Email);

            var now = DateTime.UtcNow;
            var expires = now.AddHours(_tokenSettings.ExpirationHours);

            // Create claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                
                // Custom claims for easy access
                new("user_id", user.Id.ToString()),
                new("full_name", user.FullName),
                new("email", user.Email),
                new("role", user.Role)
            };

            // Add optional claims
            if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                claims.Add(new Claim("profile_image", user.ProfileImageUrl));
            }

            if (user.LastLogin.HasValue)
            {
                claims.Add(new Claim("last_login", user.LastLogin.Value.ToString("O")));
            }

            // Create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                NotBefore = now,
                IssuedAt = now,
                Issuer = _tokenSettings.Issuer,
                Audience = _tokenSettings.Audience,
                SigningCredentials = _signingCredentials
            };

            // Generate token
            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            _logger.LogInformation("Token generated successfully for user: {UserId}, expires at: {Expires:yyyy-MM-dd HH:mm:ss} UTC", 
                user.Id, expires);

            // Create AuthToken using factory method
            return AuthToken.CreateFromUser(user, tokenString, expires, now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            _logger.LogDebug("Validating JWT token");

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation failed: token is null or empty");
                return null;
            }

            var key = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _tokenSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _tokenSettings.ValidateIssuer,
                ValidIssuer = _tokenSettings.Issuer,
                ValidateAudience = _tokenSettings.ValidateAudience,
                ValidAudience = _tokenSettings.Audience,
                ValidateLifetime = _tokenSettings.ValidateLifetime,
                RequireExpirationTime = _tokenSettings.RequireExpirationTime,
                RequireSignedTokens = _tokenSettings.RequireSignedTokens,
                ClockSkew = TimeSpan.FromMinutes(_tokenSettings.ClockSkewMinutes)
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            
            _logger.LogDebug("Token validation successful");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token validation failed: token expired - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("Token validation failed: invalid signature - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }

    /// <summary>
    /// Extract user ID from JWT token without full validation
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>User ID if found, 0 otherwise</returns>
    public int ExtractUserIdFromToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return 0;

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
            
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting user ID from token");
            return 0;
        }
    }

    /// <summary>
    /// Check if token is expired without full validation
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>True if expired, false if valid or cannot determine</returns>
    public bool IsTokenExpired(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking token expiration");
            return true;
        }
    }
    
    // Agregar estos métodos a tu TokenService.cs existente

/// <summary>
/// Generate JWT token specifically for password reset
/// </summary>
/// <param name="userId">User ID</param>
/// <param name="email">User email</param>
/// <returns>Password reset token (expires in 1 hour)</returns>
public string GeneratePasswordResetToken(int userId, string email)
{
    try
    {
        _logger.LogDebug("Generating password reset token for user: {UserId} - {Email}", userId, email);

        var now = DateTime.UtcNow;
        var expires = now.AddHours(1); // ⚠️ Solo 1 hora para seguridad

        // Claims específicos para password reset
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Email, email),
            
            // Claims importantes para reset
            new("user_id", userId.ToString()),
            new("email", email),
            new("purpose", "password_reset"), // 🔥 Claim crucial para identificar el propósito
            new("reset_timestamp", now.ToString("O")) // Para auditoría
        };

        // Create token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            NotBefore = now,
            IssuedAt = now,
            Issuer = _tokenSettings.Issuer,
            Audience = _tokenSettings.Audience,
            SigningCredentials = _signingCredentials
        };

        // Generate token
        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);

        _logger.LogInformation("Password reset token generated for user: {UserId}, expires at: {Expires:yyyy-MM-dd HH:mm:ss} UTC", 
            userId, expires);

        return tokenString;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating password reset token for user: {UserId}", userId);
        throw;
    }
}

/// <summary>
/// Validate password reset token and extract user information
/// </summary>
/// <param name="token">Password reset token</param>
/// <returns>Tuple with (isValid, userId, email) or (false, 0, null) if invalid</returns>
public (bool IsValid, int UserId, string? Email) ValidatePasswordResetToken(string token)
{
    try
    {
        _logger.LogDebug("Validating password reset token");

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Password reset token validation failed: token is null or empty");
            return (false, 0, null);
        }

        var key = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = _tokenSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = _tokenSettings.ValidateIssuer,
            ValidIssuer = _tokenSettings.Issuer,
            ValidateAudience = _tokenSettings.ValidateAudience,
            ValidAudience = _tokenSettings.Audience,
            ValidateLifetime = true, // ⚠️ Siempre validar expiración para reset tokens
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero // ⚠️ Sin tolerancia para reset tokens
        };

        var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
        
        // Verificar que es específicamente un token de password reset
        var purposeClaim = principal.FindFirst("purpose")?.Value;
        if (purposeClaim != "password_reset")
        {
            _logger.LogWarning("Token validation failed: not a password reset token");
            return (false, 0, null);
        }

        // Extraer información del usuario
        var userIdClaim = principal.FindFirst("user_id")?.Value;
        var emailClaim = principal.FindFirst("email")?.Value;

        if (!int.TryParse(userIdClaim, out var userId) || string.IsNullOrWhiteSpace(emailClaim))
        {
            _logger.LogWarning("Token validation failed: missing or invalid user claims");
            return (false, 0, null);
        }

        _logger.LogInformation("Password reset token validation successful for user: {UserId}", userId);
        return (true, userId, emailClaim);
    }
    catch (SecurityTokenExpiredException ex)
    {
        _logger.LogWarning("Password reset token validation failed: token expired - {Message}", ex.Message);
        return (false, 0, null);
    }
    catch (SecurityTokenInvalidSignatureException ex)
    {
        _logger.LogWarning("Password reset token validation failed: invalid signature - {Message}", ex.Message);
        return (false, 0, null);
    }
    catch (SecurityTokenException ex)
    {
        _logger.LogWarning("Password reset token validation failed: {Message}", ex.Message);
        return (false, 0, null);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during password reset token validation");
        return (false, 0, null);
    }
}
}