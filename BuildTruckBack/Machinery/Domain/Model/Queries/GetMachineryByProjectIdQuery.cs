namespace BuildTruckBack.Machinery.Domain.Model.Queries;

public record GetMachineryByProjectIdQuery(){
    public string ProjectId { get; init; } = string.Empty;
}