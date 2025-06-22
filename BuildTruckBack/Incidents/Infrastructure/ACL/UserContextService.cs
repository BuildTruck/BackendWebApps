using System.Threading.Tasks;
using BuildTruckBack.Incidents.Application.ACL.Services;
using BuildTruckBack.Users.Domain.Repositories;

namespace BuildTruckBack.Incidents.Infrastructure.ACL;

public class UserContextService : IUserContextService
{
    private readonly IUserRepository _userRepository;

    public UserContextService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> ExistsAsync(int userId)
    {
        return await _userRepository.FindByIdAsync(userId) != null;
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        if (int.TryParse(userId, out int id))
        {
            return await ExistsAsync(id);
        }
        return false;
    }
}