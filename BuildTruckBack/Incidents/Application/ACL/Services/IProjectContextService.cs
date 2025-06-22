using System.Threading.Tasks;

namespace BuildTruckBack.Incidents.Application.ACL.Services;

public interface IProjectContextService
{
    Task<bool> ExistsAsync(int projectId);
}