using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Auth.Interfaces.REST.Resources;

/// <summary>
/// Sign-in request resource
/// </summary>
/// <remarks>
/// Resource for user authentication requests
/// </remarks>
public record SignInResource
{
    /// <summary>
    /// Corporate email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 100 characters")]
    public string Password { get; init; } = string.Empty;
}