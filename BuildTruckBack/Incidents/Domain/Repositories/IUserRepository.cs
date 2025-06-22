namespace BuildTruckBack.Incidents.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> ExistsAsync(string userId);
}