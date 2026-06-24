namespace BuildTruckBack.Stats.Application.Internal.OutboundServices;

/// <summary>
/// Facade interface for Stats operations exposed to other bounded contexts in the monolith.
/// The implementation delegates all calls to BuildTruckStatsService via HTTP.
/// Stats does not currently expose outbound services consumed by other modules,
/// but the facade is defined here for future use and to complete the migration pattern.
/// </summary>
public interface IStatsFacade
{
    /// <summary>
    /// Triggers stats recalculation for a manager in the Stats microservice.
    /// </summary>
    Task TriggerCalculationAsync(int managerId);
}
