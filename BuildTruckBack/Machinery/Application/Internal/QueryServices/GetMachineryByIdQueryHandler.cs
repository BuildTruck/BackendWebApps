using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Domain.Repositories;

namespace BuildTruckBack.Machinery.Application.Internal.QueryServices;

public class GetMachineryByIdQueryHandler
{
    private readonly IMachineryRepository _machineryRepository;

    public GetMachineryByIdQueryHandler(IMachineryRepository machineryRepository)
    {
        _machineryRepository = machineryRepository;
    }

    public async Task<Domain.Model.Aggregates.Machinery?> Handle(GetMachineryByIdQuery query)
    {
        return await _machineryRepository.FindByIdAsync(query.Id);
    }
}