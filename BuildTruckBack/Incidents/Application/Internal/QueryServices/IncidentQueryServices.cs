using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.Model.Queries;
using BuildTruckBack.Incidents.Domain.Repositories;

namespace BuildTruckBack.Incidents.Application.Internal.QueryServices;

public class IncidentQueryService : IIncidentQueryHandler
{
    private readonly IIncidentRepository _incidentRepository;

    public IncidentQueryService(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public async Task<Incident?> HandleAsync(GetIncidentByIdQuery query)
    {
        return await _incidentRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query)
    {
        return await _incidentRepository.FindByProjectIdAsync(query.ProjectId);
    }
}