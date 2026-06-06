namespace BuildTruckBack.Documentation.Application.Internal.OutboundServices;

public interface IDocumentationFacade
{
    Task<IEnumerable<DocumentationDto>> GetByProjectIdAsync(int projectId);
    Task<DocumentationDto?> GetByIdAsync(int documentationId);
    Task<DocumentationDto> CreateAsync(CreateDocumentationDto dto);
    Task<DocumentationDto> UpdateAsync(int id, UpdateDocumentationDto dto);
    Task<bool> DeleteAsync(int documentationId);
}

public record DocumentationDto(
    int Id,
    int ProjectId,
    string Title,
    string Description,
    string? ImageUrl,
    DateTime Date,
    int CreatedBy,
    DateTime CreatedAt
);

public record CreateDocumentationDto(
    int ProjectId,
    string Title,
    string Description,
    string? ImageUrl,
    DateTime Date
);

public record UpdateDocumentationDto(
    string Title,
    string Description,
    string? ImageUrl,
    DateTime Date
);
