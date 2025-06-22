using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Incidents.Infrastructure.Persistence.EFC.Repositories;

public class IncidentRepository : BaseRepository<Incident>, IIncidentRepository
{
    public IncidentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Incident>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<Incident>()
            .Where(i => i.ProjectId == projectId)
            .ToListAsync();
    }
}