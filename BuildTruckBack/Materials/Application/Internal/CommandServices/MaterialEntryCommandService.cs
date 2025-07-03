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
    public class MaterialEntryCommandService : IMaterialEntryCommandService
    {
        private readonly IMaterialEntryRepository _entryRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MaterialEntryCommandService(
            IMaterialEntryRepository entryRepository, 
            IMaterialRepository materialRepository,
            IUnitOfWork unitOfWork)
        {
            _entryRepository = entryRepository;
            _materialRepository = materialRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MaterialEntry?> Handle(CreateMaterialEntryCommand command)
        {
            // Validar que el material existe y pertenece al proyecto
            var material = await _materialRepository.GetByIdAsync(command.MaterialId);
            if (material == null)
                throw new InvalidOperationException("El material especificado no existe");
                
            if (material.ProjectId != command.ProjectId)
                throw new InvalidOperationException("El material no pertenece al proyecto especificado");

            // CORRECCIÓN: No hay duplicación aquí porque MaterialEntry permite múltiples entradas
            // para el mismo material (diferentes fechas, proveedores, etc.)
            // Si quisieras evitar duplicados en el mismo día/proveedor, agregarías:
            
            // var existingEntry = await _entryRepository.GetByMaterialAndDateAsync(
            //     command.MaterialId, command.Date, command.Provider);
            // if (existingEntry != null)
            //     throw new InvalidOperationException("Ya existe una entrada para este material en esta fecha con este proveedor");

            var entry = new MaterialEntry(
                command.ProjectId,
                command.MaterialId,
                command.Date,
                new MaterialQuantity(command.Quantity),
                new MaterialUnit(command.Unit),
                new MaterialCost(command.UnitCost, "PEN"),
                new PaymentMethod(command.Payment),
                new DocumentType(command.DocumentType),
                command.DocumentNumber,
                command.Provider,
                command.Ruc,
                command.Observations
            );

            await _entryRepository.AddAsync(entry);
            await _unitOfWork.CompleteAsync();
            return entry;
        }

        public async Task<MaterialEntry?> Handle(UpdateMaterialEntryCommand command)
        {
            var entry = await _entryRepository.GetByIdAsync(command.EntryId);
            if (entry == null)
                throw new InvalidOperationException("Entrada no encontrada");

            entry.UpdateDetails(
                command.Date,
                new MaterialQuantity(command.Quantity),
                new MaterialUnit(command.Unit),
                new MaterialCost(command.UnitCost, "PEN"),
                new PaymentMethod(command.Payment),
                new DocumentType(command.DocumentType),
                command.DocumentNumber,
                command.Provider,
                command.Ruc,
                command.Observations
            );

            await _unitOfWork.CompleteAsync();
            return entry;
        }

        public async Task<bool> Handle(DeleteMaterialEntryCommand command)
        {
            var entry = await _entryRepository.GetByIdAsync(command.EntryId);
            if (entry == null)
                return false;

            _entryRepository.Remove(entry);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}