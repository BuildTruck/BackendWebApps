using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Configurations.Infrastructure.Persistence.EFC.Repositories;

public class ConfigurationRepository(AppDbContext context) : BaseRepository<Configuration>(context), IConfigurationRepository
{
    public async Task<Configuration?> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Configuration>()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}