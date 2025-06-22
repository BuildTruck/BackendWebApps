using System.Threading.Tasks;

namespace BuildTruckBack.Incidents.Application.ACL.Services;

public interface IUserContextService
{
    Task<bool> ExistsAsync(string userId);
}