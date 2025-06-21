
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Machinery.Infrastructure.Persistence.EFC.Repositories;

public class MachineryRepository : BaseRepository<Domain.Model.Aggregates.Machinery>, IMachineryRepository
{
    private readonly AppDbContext _context;

    public MachineryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> FindByProjectIdAsync(int projectId)
    {
        return await _context.Set<Domain.Model.Aggregates.Machinery>()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<Domain.Model.Aggregates.Machinery?> FindByLicensePlateAsync(string licensePlate, int projectId)
    {
        return await _context.Set<Domain.Model.Aggregates.Machinery>()
            .FirstOrDefaultAsync(m => 
                m.ProjectId == projectId && 
                m.LicensePlate == licensePlate);
    }
    
}