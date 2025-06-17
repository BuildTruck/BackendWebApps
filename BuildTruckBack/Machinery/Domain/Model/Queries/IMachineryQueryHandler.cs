namespace BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Domain.Model.Aggregates;

public interface IMachineryQueryHandler

{
    Task<Machinery?> Handle(GetMachineryByIdQuery query);
    Task<IEnumerable<Machinery>> Handle(GetMachineryByProjectIdQuery query);
}