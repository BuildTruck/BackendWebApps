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
    /// Application service for handling material entry commands
    /// </summary>
    public class MaterialEntryCommandService : IMaterialEntryCommandService
    {
        private readonly IMaterialEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MaterialEntryCommandService(IMaterialEntryRepository entryRepository, IUnitOfWork unitOfWork)
        {
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MaterialEntry?> Handle(CreateMaterialEntryCommand command)
        {
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
                throw new InvalidOperationException("Entry not found");

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
