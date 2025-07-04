using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Configurations.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Repository for ConfigurationSettings
/// </summary>
/// <remarks>Author: Your Name Here</remarks>
public class ConfigurationSettingsRepository : IConfigurationSettingsRepository
{
    private readonly AppDbContext _context;

    public ConfigurationSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationSettings?> FindByIdAsync(int id)
    {
        return await _context.Set<ConfigurationSettings>().FindAsync(id);
    }

    public async Task<ConfigurationSettings?> FindByUserIdAsync(int userId)
    {
        return await _context.Set<ConfigurationSettings>().FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task AddAsync(ConfigurationSettings configurationSettings)
    {
        await _context.Set<ConfigurationSettings>().AddAsync(configurationSettings);
    }

    public async Task UpdateAsync(ConfigurationSettings configurationSettings)
    {
        _context.Set<ConfigurationSettings>().Update(configurationSettings);
        await Task.CompletedTask;
    }
}