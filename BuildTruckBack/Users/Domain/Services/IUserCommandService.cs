using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.Commands;

namespace BuildTruckBack.Users.Domain.Services;

/**
 * <summary>
 *     The user command service interface
 * </summary>
 * <remarks>
 *     This service handles user commands for the BuildTruck platform
 * </remarks>
 */
public interface IUserCommandService
{
    /**
     * <summary>
     *     Handle create user command
     * </summary>
     * <param name="command">The create user command</param>
     * <returns>The created user</returns>
     */
    Task<User> Handle(CreateUserCommand command);

    /**
     * <summary>
     *     Handle change password command
     * </summary>
     * <param name="command">The change password command</param>
     * <returns>The updated user</returns>
     */
    Task<User> Handle(ChangePasswordCommand command);
}