using BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Domain.Repositories;

namespace BuildTruckBack.Machinery.Application.Internal.QueryServices;

public class MachineryQueryService(IMachineryRepository machineryRepository) : IMachineryQueryHandler
{
    public async Task<Domain.Model.Aggregates.Machinery?> Handle(GetMachineryByIdQuery query)
    {
        return await machineryRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> Handle(GetMachineryByProjectIdQuery query)
    {
        return await machineryRepository.FindByProjectIdAsync(query.ProjectId);
    }
}