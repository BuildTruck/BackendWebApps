using System.Threading.Tasks;

namespace BuildTruckBack.Incidents.Domain.Commands;

public interface IIncidentCommandHandler
{
    Task<int> HandleAsync(CreateIncidentCommand command);
    Task HandleAsync(UpdateIncidentCommand command);
}