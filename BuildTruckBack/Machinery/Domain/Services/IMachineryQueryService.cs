using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Queries;

namespace BuildTruckBack.Machinery.Domain.Services;

public interface IMachineryQueryService
{
    Task<Model.Aggregates.Machinery?> Handle(GetMachineryByIdQuery query);
    Task<IEnumerable<Model.Aggregates.Machinery>> Handle(GetMachineryByProjectQuery query);
    Task<IEnumerable<Model.Aggregates.Machinery>> Handle(GetActiveMachineryQuery query);

}