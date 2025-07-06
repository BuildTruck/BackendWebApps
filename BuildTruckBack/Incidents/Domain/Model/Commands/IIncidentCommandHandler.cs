using System.Threading.Tasks;
namespace BuildTruckBack.Incidents.Domain.Model.Commands;

public interface IIncidentCommandHandler
{
    Task<int> HandleAsync(CreateIncidentCommand command);
    Task HandleAsync(UpdateIncidentCommand command);
    Task DeleteAsync(int id);
    
    
}