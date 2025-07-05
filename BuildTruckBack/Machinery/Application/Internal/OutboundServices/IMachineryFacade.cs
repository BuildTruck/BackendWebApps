namespace BuildTruckBack.Machinery.Application.Internal.OutboundServices;

using BuildTruckBack.Machinery.Domain.Model.Aggregates;
    /// <summary>
    /// Machinery Facade Interface for external access to Machinery bounded context
    /// Provides a clean contract for other bounded contexts to interact with Machinery
    /// </summary>
    public interface IMachineryFacade
    {
        /// <summary>
        /// Get all machinery for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of machinery items for the project</returns>
        Task<List<Machinery>> GetMachineryByProjectAsync(int projectId);

        /// <summary>
        /// Get specific machinery by ID
        /// </summary>
        /// <param name="machineryId">Machinery ID</param>
        /// <returns>Machinery if found, null otherwise</returns>
        Task<Machinery?> GetMachineryByIdAsync(int machineryId);

        /// <summary>
        /// Get total count of all machinery in the system
        /// </summary>
        /// <returns>Total machinery count</returns>
        Task<int> GetTotalMachineryCountAsync();

        /// <summary>
        /// Get count of active machinery in the system
        /// </summary>
        /// <returns>Active machinery count</returns>
        Task<int> GetActiveMachineryCountAsync();

        /// <summary>
        /// Get active machinery for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of active machinery for the project</returns>
        Task<List<Machinery>> GetActiveMachineryByProjectAsync(int projectId);

        /// <summary>
        /// Get count of machinery by specific status
        /// </summary>
        /// <param name="status">Machinery status (active, maintenance, inactive, etc.)</param>
        /// <returns>Count of machinery with the specified status</returns>
        Task<int> GetMachineryCountByStatusAsync(string status);

        /// <summary>
        /// Get breakdown of machinery counts grouped by status
        /// </summary>
        /// <returns>Dictionary with status as key and count as value</returns>
        Task<Dictionary<string, int>> GetMachineryByStatusBreakdownAsync();

        /// <summary>
        /// Get breakdown of machinery counts grouped by project
        /// </summary>
        /// <returns>Dictionary with project name as key and count as value</returns>
        Task<Dictionary<string, int>> GetMachineryByProjectBreakdownAsync();

        /// <summary>
        /// Get all machinery with specific status
        /// </summary>
        /// <param name="status">Machinery status</param>
        /// <returns>List of machinery with the specified status</returns>
        Task<List<Machinery>> GetMachineryByStatusAsync(string status);

        /// <summary>
        /// Validate if machinery exists and belongs to the specified project
        /// </summary>
        /// <param name="machineryId">Machinery ID</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>True if machinery exists in the project, false otherwise</returns>
        Task<bool> ValidateMachineryExistsInProjectAsync(int machineryId, int projectId);

        /// <summary>
        /// Calculate machinery availability rate for a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>Availability rate as percentage (0-100)</returns>
        Task<decimal> GetMachineryAvailabilityRateAsync(int projectId);

        /// <summary>
        /// Get machinery for specific project filtered by status
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="status">Machinery status</param>
        /// <returns>List of machinery matching project and status criteria</returns>
        Task<List<Machinery>> GetMachineryByProjectAndStatusAsync(int projectId, string status);
    }
