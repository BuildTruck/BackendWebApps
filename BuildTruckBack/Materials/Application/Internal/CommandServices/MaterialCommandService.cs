using System;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Materials.Domain.Model.Commands;
using BuildTruckBack.Materials.Domain.Model.ValueObjects;
using BuildTruckBack.Materials.Domain.Repositories;
using BuildTruckBack.Materials.Domain.Services;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Materials.Application.Internal.CommandServices
{
    /// <summary>
    /// Application service for handling material commands
    /// </summary>
    public class MaterialCommandService : IMaterialCommandService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MaterialCommandService(IMaterialRepository materialRepository, IUnitOfWork unitOfWork)
        {
            _materialRepository = materialRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Material?> Handle(CreateMaterialCommand command)
        {
            var material = new Material(
                command.ProjectId,
                new MaterialName(command.Name),
                new MaterialType(command.Type),
                new MaterialUnit(command.Unit),
                new MaterialQuantity(command.MinimumStock),
                command.Provider
            );

            await _materialRepository.AddAsync(material);
            await _unitOfWork.CompleteAsync();
            return material;
        }

        public async Task<Material?> Handle(UpdateMaterialCommand command)
        {
            var material = await _materialRepository.GetByIdAsync(command.MaterialId);
            if (material == null)
                throw new InvalidOperationException("Material not found");

            material.UpdateBasicInfo(
                new MaterialName(command.Name),
                new MaterialType(command.Type),
                new MaterialUnit(command.Unit),
                new MaterialQuantity(command.MinimumStock),
                command.Provider
            );

            await _unitOfWork.CompleteAsync();
            return material;
        }

        public async Task<bool> Handle(DeleteMaterialCommand command)
        {
            var material = await _materialRepository.GetByIdAsync(command.MaterialId);
            if (material == null)
                return false;

            _materialRepository.Remove(material);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}