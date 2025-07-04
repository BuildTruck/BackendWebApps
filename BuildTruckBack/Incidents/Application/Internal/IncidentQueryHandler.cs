using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.Model.Queries;

namespace BuildTruckBack.Incidents.Application.Internal
{
    public class IncidentQueryHandler : IIncidentQueryHandler
    {
        public Task<Incident?> HandleAsync(GetIncidentByIdQuery query)
        {
            // Implementa la lógica para obtener un incidente por ID aquí
            return Task.FromResult<Incident?>(null);
        }

        public Task<IEnumerable<Incident>> HandleAsync(GetIncidentsByProjectIdQuery query)
        {
            // Implementa la lógica para obtener incidentes por ID de proyecto aquí
            return Task.FromResult<IEnumerable<Incident>>(new List<Incident>());
        }
    }
}