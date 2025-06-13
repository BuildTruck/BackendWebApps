using BuildTruckBack.Auth.Domain.Model.Queries;
using BuildTruckBack.Auth.Domain.Model.ValueObjects;

namespace BuildTruckBack.Auth.Domain.Services;

/// <summary>
/// Auth Query Service Interface
/// </summary>
/// <remarks>
/// Domain service interface for authentication queries
/// </remarks>
public interface IAuthQueryService
{
    /// <summary>
    /// Handle get current user query
    /// </summary>
    /// <param name="query">Get current user query with user ID and audit info</param>
    /// <returns>AuthenticatedUser if found and active, null otherwise</returns>
    Task<AuthenticatedUser?> HandleGetCurrentUserAsync(GetCurrentUserQuery query);
}