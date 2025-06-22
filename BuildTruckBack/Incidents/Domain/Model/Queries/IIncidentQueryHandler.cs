using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;

namespace BuildTruckBack.Incidents.Domain.Model.Queries;

public interface IIncidentQueryHandler
{
    Task<Incident?> HandleAsync(GetIncidentByIdQuery query);
    Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query);
}