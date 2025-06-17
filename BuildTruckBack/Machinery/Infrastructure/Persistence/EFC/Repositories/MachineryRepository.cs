using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using BuildTruckBack.Machinery.Domain.Model.Aggregates;
namespace BuildTruckBack.Machinery.Infrastructure.Persistence.EFC.Repositories;

public class MachineryRepository(AppDbContext context) : BaseRepository<Domain.Model.Aggregates.Machinery>(context), IMachineryRepository
{
    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> FindByProjectIdAsync(string projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Machinery>()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<Domain.Model.Aggregates.Machinery?> FindByLicensePlateAsync(string licensePlate)
    {
        return await Context.Set<Domain.Model.Aggregates.Machinery>()
            .FirstOrDefaultAsync(m => m.LicensePlate == licensePlate);
    }
}