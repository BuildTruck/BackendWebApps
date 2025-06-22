using System;

namespace BuildTruckBack.Materials.Domain.Model.Commands
{
    public record UpdateMaterialEntryCommand(
        int EntryId,     // INT
        DateTime Date,
        decimal Quantity,
        string Unit,
        string Provider,
        string Ruc,
        string Payment,
        string DocumentType,
        string DocumentNumber,
        decimal UnitCost,
        decimal TotalCost,
        string Status,
        string Observations
    );
}