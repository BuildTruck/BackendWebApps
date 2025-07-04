using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.Commands;
using BuildTruckBack.Incidents.Domain.Model.Queries;


namespace BuildTruckBack.Incidents.Application.Internal;

public interface IIncidentFacade
{
    Task<int> CreateIncidentAsync(CreateIncidentCommand command);
    Task UpdateIncidentAsync(UpdateIncidentCommand command);
    Task<Incident?> GetIncidentByIdAsync(int id);
    
    Task DeleteIncidentAsync(int id);
    Task<IEnumerable<Incident>> GetIncidentsByProjectIdAsync(int projectId);
}

public class IncidentFacade : IIncidentFacade
{
    private readonly IIncidentCommandHandler _commandHandler;
    private readonly IIncidentQueryHandler _queryHandler;

    public IncidentFacade(IIncidentCommandHandler commandHandler, IIncidentQueryHandler queryHandler)
    {
        _commandHandler = commandHandler;
        _queryHandler = queryHandler;
    }

    public async Task<int> CreateIncidentAsync(CreateIncidentCommand command)
    {
        return await _commandHandler.HandleAsync(command);
    }

    public async Task UpdateIncidentAsync(UpdateIncidentCommand command)
    {
        await _commandHandler.HandleAsync(command);
    }

    public async Task<Incident?> GetIncidentByIdAsync(int id)
    {
        return await _queryHandler.HandleAsync(new GetIncidentByIdQuery(id));
    }

    public async Task<IEnumerable<Incident>> GetIncidentsByProjectIdAsync(int projectId)
    {
        return await _queryHandler.HandleAsync(new GetIncidentsByProjectIdQuery(projectId));
    }
    public async Task DeleteIncidentAsync(int id)
    {
        await _commandHandler.DeleteAsync(id);
    }
}