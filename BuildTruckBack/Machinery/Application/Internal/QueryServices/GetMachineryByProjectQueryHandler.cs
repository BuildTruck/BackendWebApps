using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Domain.Repositories;

namespace BuildTruckBack.Machinery.Application.Internal.QueryServices;

public class GetMachineryByProjectQueryHandler
{
    private readonly IMachineryRepository _machineryRepository;

    public GetMachineryByProjectQueryHandler(IMachineryRepository machineryRepository)
    {
        _machineryRepository = machineryRepository;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> Handle(GetMachineryByProjectQuery query)
    {
        return await _machineryRepository.FindByProjectIdAsync(query.ProjectId);
    }
}