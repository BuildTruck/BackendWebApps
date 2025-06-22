using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Incidents.Domain.Repositories;

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<IEnumerable<Incident>> FindByProjectIdAsync(int projectId);
}