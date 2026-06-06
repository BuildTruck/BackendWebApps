namespace BuildTruckBack.Machinery.Application.Internal.OutboundServices;

public interface IMachineryFacade
{
    Task<IEnumerable<MachineryDto>> GetByProjectIdAsync(int projectId);
    Task<MachineryDto?> GetByIdAsync(int machineryId);
    Task<MachineryDto> CreateAsync(CreateMachineryDto dto);
    Task<MachineryDto> UpdateAsync(int id, UpdateMachineryDto dto);
    Task<bool> DeleteAsync(int machineryId);

    // Additional methods for ACL services
    Task<MachineryDto?> GetMachineryByIdAsync(int machineryId) => GetByIdAsync(machineryId);
    Task<bool> ValidateMachineryExistsInProjectAsync(int machineryId, int projectId);
    Task<IEnumerable<MachineryDto>> GetActiveMachineryByProjectAsync(int projectId);
    Task<IEnumerable<MachineryDto>> GetMachineryByProjectAsync(int projectId) => GetByProjectIdAsync(projectId);
}

public record MachineryDto(
    int Id,
    int ProjectId,
    string Name,
    string Description,
    string Type,
    string Condition,
    string Status,
    int? PersonnelId,
    string? ImageUrl,
    DateTime AcquisitionDate,
    string? Notes,
    DateTime CreatedAt
)
{
    public bool IsActive() => Status == "Activo" || Status == "Active";
};

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
