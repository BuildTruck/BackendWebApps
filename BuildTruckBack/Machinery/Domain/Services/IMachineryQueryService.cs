namespace BuildTruckBack.Machinery.Domain.Services;

using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Queries;

public interface IMachineryQueryService
{
    Task<Machinery?> Handle(GetMachineryByIdQuery query);
    Task<IEnumerable<Machinery>> Handle(GetMachineryByProjectQuery query);
    Task<IEnumerable<Machinery>> Handle(GetActiveMachineryQuery query);
}