using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Services;

namespace BuildTruckBack.Machinery.Application.Internal.CommandServices;

public class MachineryCommandService : IMachineryCommandService
{
    private readonly CreateMachineryCommandHandler _createMachineryCommandHandler;
    private readonly UpdateMachineryCommandHandler _updateMachineryCommandHandler;
    private readonly DeleteMachineryCommandHandler _deleteMachineryCommandHandler;
   

    public MachineryCommandService(
        CreateMachineryCommandHandler createMachineryCommandHandler,
        UpdateMachineryCommandHandler updateMachineryCommandHandler,
        DeleteMachineryCommandHandler deleteMachineryCommandHandler)
    {
        _createMachineryCommandHandler = createMachineryCommandHandler;
        _updateMachineryCommandHandler = updateMachineryCommandHandler;
        _deleteMachineryCommandHandler = deleteMachineryCommandHandler;
       
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(CreateMachineryCommand command, IFormFile? imageFile = null)
    {
        return await _createMachineryCommandHandler.Handle(command, imageFile);
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(UpdateMachineryCommand command, IFormFile? imageFile = null)
    {
        return await _updateMachineryCommandHandler.Handle(command, imageFile);
    }

    public async Task Handle(DeleteMachineryCommand command)
    {
        await _deleteMachineryCommandHandler.Handle(command);
    }

   
}