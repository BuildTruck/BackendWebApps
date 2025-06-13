using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.ValueObjects;
using BuildTruckBack.Users.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Users.Infrastructure.Persistence.EFC.Repositories;

/**
 * <summary>
 *     The user repository implementation using Entity Framework
 * </summary>
 * <remarks>
 *     This repository manages users with Value Objects conversion
 * </remarks>
 */
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    /**
     * <summary>
     *     Find user by email using Value Object
     * </summary>
     * <param name="email">The email value object to search</param>
     * <returns>The user if found, null otherwise</returns>
     */
    public async Task<User?> FindByEmailAsync(EmailAddress email)
    {
        // ✅ Convertir Value Object a string para query EF
        return await Context.Set<User>()
            .FirstOrDefaultAsync(user => user.CorporateEmail.Address == email.Address);
    }

    /**
     * <summary>
     *     Check if email exists using Value Object
     * </summary>
     * <param name="email">The email value object to check</param>
     * <returns>True if email exists, false otherwise</returns>
     */
    public async Task<bool> ExistsByEmailAsync(EmailAddress email)
    {
        // ✅ Convertir Value Object a string para query EF
        return await Context.Set<User>()
            .AnyAsync(user => user.CorporateEmail.Address == email.Address);
    }

    /**
     * <summary>
     *     Find users by role using Value Object
     * </summary>
     * <param name="role">The role value object to search</param>
     * <returns>List of users with the specified role</returns>
     */
    public async Task<IEnumerable<User>> FindByRoleAsync(UserRole role)
    {
        // ✅ Convertir Value Object a string para query EF
        return await Context.Set<User>()
            .Where(user => user.Role.Role == role.Role && user.IsActive)
            .ToListAsync();
    }
    
    /**
     * <summary>
     *     Find user by person name (first name + last name) - NUEVO
     * </summary>
     * <param name="personName">The person name to search</param>
     * <returns>The user if found, null otherwise</returns>
     */
    public async Task<User?> FindByPersonNameAsync(PersonName personName)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.Name.FirstName == personName.FirstName 
                                   && u.Name.LastName == personName.LastName);
    }
}