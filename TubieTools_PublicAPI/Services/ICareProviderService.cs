namespace TubieTools_PublicAPI.Services
{
    using TubieTools_PublicAPI.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for care provider service operations
    /// </summary>
    public interface ICareProviderService
    {
        /// <summary>
        /// Get all care providers with optional filtering
        /// </summary>
        /// <param name="tier">Filter by care provider tier (optional)</param>
        /// <param name="status">Filter by provider status (optional)</param>
        /// <returns>List of care providers</returns>
        Task<List<CareProvider>> GetAllProvidersAsync(CareProviderTier? tier = null, string status = null);

        /// <summary>
        /// Get a specific care provider by ID
        /// </summary>
        /// <param name="providerId">The provider ID</param>
        /// <returns>Care provider or null if not found</returns>
        Task<CareProvider> GetProviderByIdAsync(string providerId);

        /// <summary>
        /// Get care providers by tier
        /// </summary>
        /// <param name="tier">The care provider tier</param>
        /// <returns>List of providers in the specified tier</returns>
        Task<List<CareProvider>> GetProvidersByTierAsync(CareProviderTier tier);

        /// <summary>
        /// Create a new care provider
        /// </summary>
        /// <param name="request">Create provider request</param>
        /// <returns>Created care provider</returns>
        Task<CareProvider> CreateProviderAsync(Models.Requests.CreateCareProviderRequest request);

        /// <summary>
        /// Update an existing care provider
        /// </summary>
        /// <param name="providerId">The provider ID to update</param>
        /// <param name="request">Update provider request</param>
        /// <returns>Updated care provider</returns>
        Task<CareProvider> UpdateProviderAsync(string providerId, Models.Requests.UpdateCareProviderRequest request);

        /// <summary>
        /// Delete/deactivate a care provider
        /// </summary>
        /// <param name="providerId">The provider ID to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteProviderAsync(string providerId);

        /// <summary>
        /// Get all providers with active status
        /// </summary>
        /// <returns>List of active providers</returns>
        Task<List<CareProvider>> GetActiveProvidersAsync();

        /// <summary>
        /// Search providers by name
        /// </summary>
        /// <param name="searchTerm">Search term for provider name</param>
        /// <returns>List of matching providers</returns>
        Task<List<CareProvider>> SearchProvidersByNameAsync(string searchTerm);
    }
}
