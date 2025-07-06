using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationPreferenceRepository : BaseRepository<NotificationPreference>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NotificationPreference>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<NotificationPreference>()
            .Where(np => np.UserId == userId)
            .ToListAsync();
    }

    public async Task<NotificationPreference?> FindByUserIdAndContextAsync(int userId, NotificationContext context)
    {
        return await Context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == userId && np.Context == context);
    }

    public async Task<bool> ExistsByUserIdAndContextAsync(int userId, NotificationContext context)
    {
        return await Context.Set<NotificationPreference>()
            .AnyAsync(np => np.UserId == userId && np.Context == context);
    }

    public async Task CreateDefaultPreferencesAsync(int userId)
    {
        var contexts = NotificationContext.GetAllContexts();
        var existingPreferences = await FindByUserIdAsync(userId);
        var existingContexts = existingPreferences.Select(p => p.Context.Value).ToHashSet();

        foreach (var context in contexts)
        {
            if (!existingContexts.Contains(context.Value))
            {
                var defaultPreference = new NotificationPreference(userId, context);
                await AddAsync(defaultPreference);
            }
        }

        await Context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationPreference>> FindEnabledEmailPreferencesAsync()
    {
        return await Context.Set<NotificationPreference>()
            .Where(np => np.EmailEnabled)
            .ToListAsync();
    }

    public async Task<bool> ShouldReceiveNotificationAsync(int userId, NotificationContext context, NotificationPriority priority)
    {
        var preference = await FindByUserIdAndContextAsync(userId, context);
        
        if (preference == null)
        {
            return true;
        }

        return preference.ShouldReceiveInApp(priority);
    }

    public async Task<bool> ShouldReceiveEmailAsync(int userId, NotificationContext context, NotificationPriority priority)
    {
        var preference = await FindByUserIdAndContextAsync(userId, context);
        
        if (preference == null)
        {
            return false;
        }

        return preference.ShouldReceiveEmail(priority);
    }
}