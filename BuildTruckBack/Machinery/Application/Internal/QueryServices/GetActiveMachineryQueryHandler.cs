using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Domain.Model.ValueObjects;
using BuildTruckBack.Machinery.Domain.Repositories;

namespace BuildTruckBack.Machinery.Application.Internal.QueryServices;

public class GetActiveMachineryQueryHandler
{
    private readonly IMachineryRepository _machineryRepository;

    public GetActiveMachineryQueryHandler(IMachineryRepository machineryRepository)
    {
        _machineryRepository = machineryRepository;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Machinery>> Handle(GetActiveMachineryQuery query)
    {
        var machinery = await _machineryRepository.FindByProjectIdAsync(query.ProjectId);
        return machinery.Where(m => m.Status == MachineryStatus.Active.ToString());
    }
}