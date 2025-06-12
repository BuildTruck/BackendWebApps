using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.ValueObjects;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Users.Domain.Repositories;

/**
 * <summary>
 *     The user repository interface
 * </summary>
 * <remarks>
 *     This repository manages users in the BuildTruck platform using Value Objects
 * </remarks>
 */
public interface IUserRepository : IBaseRepository<User>
{
    /**
     * <summary>
     *     Find user by email
     * </summary>
     * <param name="email">The email to search</param>
     * <returns>The user if found, null otherwise</returns>
     */
    Task<User?> FindByEmailAsync(EmailAddress email);

    /**
     * <summary>
     *     Check if email exists
     * </summary>
     * <param name="email">The email to check</param>
     * <returns>True if email exists, false otherwise</returns>
     */
    Task<bool> ExistsByEmailAsync(EmailAddress email);

    /**
     * <summary>
     *     Find users by role
     * </summary>
     * <param name="role">The role to search</param>
     * <returns>List of users with the specified role</returns>
     */
    Task<IEnumerable<User>> FindByRoleAsync(UserRole role);

    /**
     * <summary>
     *     Find available supervisors (not assigned to any project)
     * </summary>
     * <returns>List of available supervisors</returns>
     */
    Task<IEnumerable<User>> FindAvailableSupervisorsAsync();
}