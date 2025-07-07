using System;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Materials.Domain.Model.Commands;
using BuildTruckBack.Materials.Domain.Model.ValueObjects;
using BuildTruckBack.Materials.Domain.Repositories;
using BuildTruckBack.Materials.Domain.Services;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Interfaces.ACL;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Materials.Application.Internal.CommandServices
{
    /// <summary>
    /// Application service for handling material usage commands
    /// </summary>
    public class MaterialUsageCommandService : IMaterialUsageCommandService
    {
        private readonly IMaterialUsageRepository _usageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationContextFacade _notificationFacade;
        public MaterialUsageCommandService(IMaterialUsageRepository usageRepository, IUnitOfWork unitOfWork)
        {
            _usageRepository = usageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MaterialUsage?> Handle(CreateMaterialUsageCommand command)
        {
            var usage = new MaterialUsage(
                command.ProjectId,
                command.MaterialId,
                command.Date,
                new MaterialQuantity(command.Quantity),
                new UsageType(command.UsageType),
                command.Area,
                command.Worker,
                command.Observations
            );
            
            await _usageRepository.AddAsync(usage);
            await _unitOfWork.CompleteAsync();
            await _notificationFacade.CreateNotificationForProjectAsync(
                usage.ProjectId,
                NotificationType.MaterialUsed,
                NotificationContext.Materials,
                "Material Utilizado",
                $"Se utilizó {usage.Quantity} unidades en {usage.UsageType} para {usage.Observations}",
                NotificationPriority.Low
            );
            
            return usage;
        }

        public async Task<MaterialUsage?> Handle(UpdateMaterialUsageCommand command)
        {
            var usage = await _usageRepository.GetByIdAsync(command.UsageId);
            if (usage == null)
                throw new InvalidOperationException("Usage not found");

            usage.UpdateDetails(
                command.Date,
                new MaterialQuantity(command.Quantity),
                new UsageType(command.UsageType),
                command.Area,
                command.Worker,
                command.Observations
            );
           
            await _unitOfWork.CompleteAsync();
            return usage;
        }

        public async Task<bool> Handle(DeleteMaterialUsageCommand command)
        {
            var usage = await _usageRepository.GetByIdAsync(command.UsageId);
            if (usage == null)
                return false;

            _usageRepository.Remove(usage);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}