namespace BuildTruckBack.Machinery.Application.Internal.OutboundServices;

using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Machinery.Domain.Services;
using Microsoft.Extensions.Logging;
    /// <summary>
    /// Machinery Facade Implementation
    /// Provides machinery information and metrics for other bounded contexts
    /// </summary>
    public class MachineryFacade : IMachineryFacade
    {
        private readonly IMachineryRepository _machineryRepository;
        private readonly IMachineryQueryService _queryService;
        private readonly ILogger<MachineryFacade> _logger;

        public MachineryFacade(
            IMachineryRepository machineryRepository,
            IMachineryQueryService queryService,
            ILogger<MachineryFacade> logger)
        {
            _machineryRepository = machineryRepository ?? throw new ArgumentNullException(nameof(machineryRepository));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Machinery>> GetMachineryByProjectAsync(int projectId)
        {
            try
            {
                _logger.LogDebug("Getting machinery for project: {ProjectId}", projectId);
                
                var query = new GetMachineryByProjectQuery(projectId);
                var machinery = await _queryService.Handle(query);
                
                _logger.LogDebug("Found {Count} machinery items for project {ProjectId}", machinery.Count(), projectId);
                return machinery.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery for project: {ProjectId}", projectId);
                return new List<Machinery>();
            }
        }

        public async Task<Machinery?> GetMachineryByIdAsync(int machineryId)
        {
            try
            {
                _logger.LogDebug("Getting machinery by ID: {MachineryId}", machineryId);
                
                var query = new GetMachineryByIdQuery(machineryId);
                var machinery = await _queryService.Handle(query);
                
                if (machinery != null)
                {
                    _logger.LogDebug("Found machinery: {Name} (ID: {MachineryId})", machinery.Name, machineryId);
                }
                else
                {
                    _logger.LogDebug("Machinery not found: {MachineryId}", machineryId);
                }
                
                return machinery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery by ID: {MachineryId}", machineryId);
                return null;
            }
        }

        public async Task<int> GetTotalMachineryCountAsync()
        {
            try
            {
                _logger.LogDebug("Getting total machinery count");
                
                var allMachinery = await _machineryRepository.ListAsync();
                var count = allMachinery.Count();
                
                _logger.LogDebug("Total machinery count: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total machinery count");
                return 0;
            }
        }

        public async Task<int> GetActiveMachineryCountAsync()
        {
            try
            {
                _logger.LogDebug("Getting active machinery count");
                
                // Get all machinery and filter active ones
                var allMachinery = await _machineryRepository.ListAsync();
                var count = allMachinery.Count(m => m.IsActive());
                
                _logger.LogDebug("Active machinery count: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active machinery count");
                return 0;
            }
        }

        public async Task<int> GetMachineryCountByStatusAsync(string status)
        {
            try
            {
                _logger.LogDebug("Getting machinery count by status: {Status}", status);
                
                var allMachinery = await _machineryRepository.ListAsync();
                var count = allMachinery.Count(m => m.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                
                _logger.LogDebug("Machinery count for status {Status}: {Count}", status, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery count by status: {Status}", status);
                return 0;
            }
        }

        public async Task<Dictionary<string, int>> GetMachineryByStatusBreakdownAsync()
        {
            try
            {
                _logger.LogDebug("Getting machinery breakdown by status");
                
                var allMachinery = await _machineryRepository.ListAsync();
                var breakdown = allMachinery
                    .GroupBy(m => m.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                _logger.LogDebug("Machinery status breakdown: {Breakdown}", 
                    string.Join(", ", breakdown.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
                
                return breakdown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery breakdown by status");
                return new Dictionary<string, int>();
            }
        }

        public async Task<Dictionary<string, int>> GetMachineryByProjectBreakdownAsync()
        {
            try
            {
                _logger.LogDebug("Getting machinery breakdown by project");
                
                var allMachinery = await _machineryRepository.ListAsync();
                var breakdown = allMachinery
                    .GroupBy(m => m.ProjectId)
                    .ToDictionary(g => $"Project {g.Key}", g => g.Count());
                
                _logger.LogDebug("Machinery project breakdown: {Count} projects", breakdown.Count);
                return breakdown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery breakdown by project");
                return new Dictionary<string, int>();
            }
        }

        public async Task<List<Machinery>> GetActiveMachineryByProjectAsync(int projectId)
        {
            try
            {
                _logger.LogDebug("Getting active machinery for project: {ProjectId}", projectId);
                
                var query = new GetActiveMachineryQuery(projectId);
                var activeMachinery = await _queryService.Handle(query);
                
                _logger.LogDebug("Found {Count} active machinery items for project {ProjectId}", activeMachinery.Count(), projectId);
                return activeMachinery.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active machinery for project: {ProjectId}", projectId);
                return new List<Machinery>();
            }
        }

        public async Task<List<Machinery>> GetMachineryByStatusAsync(string status)
        {
            try
            {
                _logger.LogDebug("Getting machinery by status: {Status}", status);
                
                var allMachinery = await _machineryRepository.ListAsync();
                var filteredMachinery = allMachinery
                    .Where(m => m.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                _logger.LogDebug("Found {Count} machinery items with status {Status}", filteredMachinery.Count, status);
                return filteredMachinery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery by status: {Status}", status);
                return new List<Machinery>();
            }
        }

        public async Task<bool> ValidateMachineryExistsInProjectAsync(int machineryId, int projectId)
        {
            try
            {
                _logger.LogDebug("Validating machinery {MachineryId} exists in project {ProjectId}", machineryId, projectId);
                
                var machinery = await GetMachineryByIdAsync(machineryId);
                var exists = machinery != null && machinery.ProjectId == projectId;
                
                _logger.LogDebug("Machinery {MachineryId} exists in project {ProjectId}: {Exists}", machineryId, projectId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating machinery exists in project: Machinery {MachineryId}, Project {ProjectId}", machineryId, projectId);
                return false;
            }
        }

        public async Task<decimal> GetMachineryAvailabilityRateAsync(int projectId)
        {
            try
            {
                _logger.LogDebug("Calculating machinery availability rate for project: {ProjectId}", projectId);
                
                var projectMachinery = await GetMachineryByProjectAsync(projectId);
                
                if (!projectMachinery.Any())
                {
                    _logger.LogDebug("No machinery found for project {ProjectId}", projectId);
                    return 0m;
                }
                
                var activeMachinery = projectMachinery.Count(m => m.IsActive());
                var availabilityRate = (decimal)activeMachinery / projectMachinery.Count * 100;
                
                _logger.LogDebug("Machinery availability rate for project {ProjectId}: {Rate}% ({Active}/{Total})", 
                    projectId, availabilityRate, activeMachinery, projectMachinery.Count);
                
                return Math.Round(availabilityRate, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating machinery availability rate for project: {ProjectId}", projectId);
                return 0m;
            }
        }

        public async Task<List<Machinery>> GetMachineryByProjectAndStatusAsync(int projectId, string status)
        {
            try
            {
                _logger.LogDebug("Getting machinery for project {ProjectId} with status {Status}", projectId, status);
                
                var projectMachinery = await GetMachineryByProjectAsync(projectId);
                var filteredMachinery = projectMachinery
                    .Where(m => m.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                _logger.LogDebug("Found {Count} machinery items for project {ProjectId} with status {Status}", 
                    filteredMachinery.Count, projectId, status);
                
                return filteredMachinery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machinery for project {ProjectId} with status {Status}", projectId, status);
                return new List<Machinery>();
            }
        }
    }
