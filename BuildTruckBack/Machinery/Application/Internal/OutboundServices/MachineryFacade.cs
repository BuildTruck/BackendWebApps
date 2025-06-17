using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Model.Queries;

namespace BuildTruckBack.Machinery.Application.Internal.OutboundServices;

public interface IMachineryFacade
{
    Task<IEnumerable<Domain.Model.Aggregates.Machinery>> GetAllMachineryAsync(string projectId);
    Task<Domain.Model.Aggregates.Machinery?> GetMachineryByIdAsync(int id);
    Task<Domain.Model.Aggregates.Machinery?> CreateMachineryAsync(CreateMachineryCommand command);
    Task<Domain.Model.Aggregates.Machinery?> UpdateMachineryAsync(UpdateMachineryCommand command);
}

public class MachineryFacade(IMachineryQueryHandler queryHandler, IMachineryCommandHandler commandHandler) : IMachineryFacade
{
    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> GetAllMachineryAsync(string projectId)
    {
        return await queryHandler.Handle(new GetMachineryByProjectIdQuery { ProjectId = projectId });
    }

    public async Task<Domain.Model.Aggregates.Machinery?> GetMachineryByIdAsync(int id)
    {
        return await queryHandler.Handle(new GetMachineryByIdQuery { Id = id });
    }

    public async Task<Domain.Model.Aggregates.Machinery?> CreateMachineryAsync(CreateMachineryCommand command)
    {
        return await commandHandler.Handle(command);
    }

    public async Task<Domain.Model.Aggregates.Machinery?> UpdateMachineryAsync(UpdateMachineryCommand command)
    {
        return await commandHandler.Handle(command);
    }
}