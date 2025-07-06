using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.Model.Queries;
using BuildTruckBack.Incidents.Domain.Repositories;

namespace BuildTruckBack.Incidents.Application.Internal
{
    public class IncidentQueryHandler : IIncidentQueryHandler
    {
        private readonly IIncidentRepository _incidentRepository;

        // ✅ Agregar constructor con dependency injection
        public IncidentQueryHandler(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<Incident?> HandleAsync(GetIncidentByIdQuery query)
        {
            // ✅ Usar el repository real, no devolver null
            return await _incidentRepository.FindByIdAsync(query.Id);
        }

        public async Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query)
        {
            // ✅ Usar el repository real, no devolver lista vacía
            return await _incidentRepository.FindByProjectIdAsync(query.ProjectId);
        }
    }
}