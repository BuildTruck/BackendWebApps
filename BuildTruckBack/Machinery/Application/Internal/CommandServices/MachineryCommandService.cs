using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Services;
using BuildTruckBack.Notifications.Interfaces.ACL;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
namespace BuildTruckBack.Machinery.Application.Internal.CommandServices;

public class MachineryCommandService : IMachineryCommandService
{
    private readonly CreateMachineryCommandHandler _createMachineryCommandHandler;
    private readonly UpdateMachineryCommandHandler _updateMachineryCommandHandler;
    private readonly DeleteMachineryCommandHandler _deleteMachineryCommandHandler;
    private readonly INotificationContextFacade _notificationFacade;

    public MachineryCommandService(
        CreateMachineryCommandHandler createMachineryCommandHandler,
        UpdateMachineryCommandHandler updateMachineryCommandHandler,
        DeleteMachineryCommandHandler deleteMachineryCommandHandler,
        INotificationContextFacade notificationFacade)  
    {
        _createMachineryCommandHandler = createMachineryCommandHandler;
        _updateMachineryCommandHandler = updateMachineryCommandHandler;
        _deleteMachineryCommandHandler = deleteMachineryCommandHandler;
        _notificationFacade = notificationFacade;     
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(CreateMachineryCommand command, IFormFile? imageFile = null)
    {
        var machinery = await _createMachineryCommandHandler.Handle(command, imageFile);
    
        // 🔔 NOTIFICAR MAQUINARIA AGREGADA
        if (machinery.ProjectId > 0)
        {
            await _notificationFacade.CreateNotificationForProjectAsync(
                projectId: machinery.ProjectId,
                type: NotificationType.MachineryAssigned,
                context: NotificationContext.Machinery,
                title: "🚜 Maquinaria Agregada",
                message: $"Se agregó la maquinaria '{machinery.Name}' al proyecto.",
                priority: NotificationPriority.Normal,
                actionUrl: $"/machinery/{machinery.Id}",
                relatedEntityId: machinery.Id,
                relatedEntityType: "Machinery"
            );
        }
    
        return machinery;
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(UpdateMachineryCommand command, IFormFile? imageFile = null)
    {
        var machinery = await _updateMachineryCommandHandler.Handle(command, imageFile);;
        
        await _notificationFacade.CreateNotificationForProjectAsync(
            machinery.ProjectId,
            NotificationType.MachineryStatusChanged,
            NotificationContext.Machinery,
            "Estado de Maquinaria Actualizado",
            $"El estado de {machinery.Name} cambió a {machinery.Status}",
            NotificationPriority.Normal
        );

        return machinery;
    }

    public async Task Handle(DeleteMachineryCommand command)
    {
        await _deleteMachineryCommandHandler.Handle(command);
    }

   
}