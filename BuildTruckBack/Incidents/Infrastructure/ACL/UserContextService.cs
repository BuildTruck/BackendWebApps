using BuildTruckBack.Incidents.Application.ACL.Services;
using BuildTruckBack.Users.Application.Internal.OutboundServices;

namespace BuildTruckBack.Incidents.Infrastructure.ACL;

public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;

    public UserContextService(IUserFacade userFacade)
    {
        _userFacade = userFacade;
    }

    public async Task<bool> ExistsAsync(int userId)
    {
        return await _userFacade.FindByIdAsync(userId) != null;
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        if (int.TryParse(userId, out int id))
            return await ExistsAsync(id);
        return false;
    }
}
