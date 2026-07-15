namespace TubieTools_PublicAPI.Services
{
    using TubieTools_PublicAPI.Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for Entity Framework based care provider operations
    /// </summary>
    public interface IEFCareProviderService
    {
        /// <summary>
        /// Get all care providers from database
        /// </summary>
        Task<List<CareProviderEntity>> GetAllProvidersAsync();

        /// <summary>
        /// Get a specific provider by ID
        /// </summary>
        Task<CareProviderEntity> GetProviderByIdAsync(int id);

        /// <summary>
        /// Get providers by ProviderId (string)
        /// </summary>
        Task<CareProviderEntity> GetProviderByProviderIdAsync(string providerId);

        /// <summary>
        /// Get providers by tier
        /// </summary>
        Task<List<CareProviderEntity>> GetProvidersByTierAsync(int tier);

        /// <summary>
        /// Get active providers
        /// </summary>
        Task<List<CareProviderEntity>> GetActiveProvidersAsync();

        /// <summary>
        /// Create a new provider
        /// </summary>
        Task<CareProviderEntity> CreateProviderAsync(CareProviderEntity provider);

        /// <summary>
        /// Update an existing provider
        /// </summary>
        Task<CareProviderEntity> UpdateProviderAsync(CareProviderEntity provider);

        /// <summary>
        /// Delete a provider
        /// </summary>
        Task<bool> DeleteProviderAsync(int id);

        /// <summary>
        /// Add purchase history entry
        /// </summary>
        Task<PurchaseHistoryEntity> AddPurchaseHistoryAsync(PurchaseHistoryEntity history);

        /// <summary>
        /// Get purchase history for a provider
        /// </summary>
        Task<List<PurchaseHistoryEntity>> GetPurchaseHistoryAsync(int careProviderId);

        /// <summary>
        /// Add discount policy to provider
        /// </summary>
        Task<DiscountPolicyEntity> AddDiscountPolicyAsync(DiscountPolicyEntity discount);

        /// <summary>
        /// Get active discounts for provider
        /// </summary>
        Task<List<DiscountPolicyEntity>> GetActiveDiscountsAsync(int careProviderId);

        /// <summary>
        /// Create an order
        /// </summary>
        Task<OrderEntity> CreateOrderAsync(OrderEntity order);

        /// <summary>
        /// Update order status
        /// </summary>
        Task<OrderEntity> UpdateOrderAsync(OrderEntity order);

        /// <summary>
        /// Get order by ID
        /// </summary>
        Task<OrderEntity> GetOrderByIdAsync(int id);

        /// <summary>
        /// Get orders for a provider
        /// </summary>
        Task<List<OrderEntity>> GetOrdersByProviderAsync(int careProviderId);
    }
}
