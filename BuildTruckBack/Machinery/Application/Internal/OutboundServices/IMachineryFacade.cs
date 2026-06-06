namespace BuildTruckBack.Machinery.Application.Internal.OutboundServices;

public interface IMachineryFacade
{
    Task<IEnumerable<MachineryDto>> GetByProjectIdAsync(int projectId);
    Task<MachineryDto?> GetByIdAsync(int machineryId);
    Task<MachineryDto> CreateAsync(CreateMachineryDto dto);
    Task<MachineryDto> UpdateAsync(int id, UpdateMachineryDto dto);
    Task<bool> DeleteAsync(int machineryId);
}

public record MachineryDto(
    int Id,
    int ProjectId,
    string Name,
    string Description,
    string Type,
    string Condition,
    string? ImageUrl,
    DateTime AcquisitionDate,
    string? Notes,
    DateTime CreatedAt
);

public record CreateMachineryDto(
    int ProjectId,
    string Name,
    string Description,
    string Type,
    string Condition,
    string? ImageUrl,
    DateTime AcquisitionDate,
    string? Notes
);

public record UpdateMachineryDto(
    string Name,
    string Description,
    string Type,
    string Condition,
    string? ImageUrl,
    DateTime AcquisitionDate,
    string? Notes
);
