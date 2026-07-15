namespace TubieTools_PublicAPI.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using DataAccessLayer.Data.Entities;
    using DataAccessLayer.Data;

    /// <summary>
    /// Implementation of EF Core based care provider service
    /// </summary>
    public class EFCareProviderService : IEFCareProviderService
    {
        private readonly TubieDbContext _dbContext;
        private readonly ILogger<EFCareProviderService> _logger;

        public EFCareProviderService(TubieDbContext dbContext, ILogger<EFCareProviderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<CareProviderEntity>> GetAllProvidersAsync()
        {
            try
            {
                return await _dbContext.CareProviders
                    .Include(c => c.PrimaryContactEntity)
                    .Include(c => c.SecondaryContactEntity)
                    .Include(c => c.BillingAddressEntity)
                    .Include(c => c.ShippingAddressEntity)
                    .Include(c => c.PaymentConfigurationEntity)
                    .Include(c => c.DiscountPoliciesEntities)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all providers: {ex.Message}");
                throw;
            }
        }

        public async Task<CareProviderEntity> GetProviderByIdAsync(int id)
        {
            try
            {
                return await _dbContext.CareProviders
                    .Include(c => c.PrimaryContactEntity)
                    .Include(c => c.SecondaryContactEntity)
                    .Include(c => c.BillingAddressEntity)
                    .Include(c => c.ShippingAddressEntity)
                    .Include(c => c.PaymentConfigurationEntity)
                    .Include(c => c.DiscountPoliciesEntities)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting provider by ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<CareProviderEntity> GetProviderByProviderIdAsync(string providerId)
        {
            try
            {
                return await _dbContext.CareProviders
                    .Include(c => c.PrimaryContactEntity)
                    .Include(c => c.SecondaryContactEntity)
                    .Include(c => c.BillingAddressEntity)
                    .Include(c => c.ShippingAddressEntity)
                    .Include(c => c.PaymentConfigurationEntity)
                    .Include(c => c.DiscountPoliciesEntities)
                    .FirstOrDefaultAsync(c => c.ProviderId == providerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting provider by ProviderId {providerId}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CareProviderEntity>> GetProvidersByTierAsync(int tier)
        {
            try
            {
                return await _dbContext.CareProviders
                    .Include(c => c.PrimaryContactEntity)
                    .Include(c => c.DiscountPoliciesEntities)
                    .Where(c => c.Tier == tier)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting providers by tier {tier}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CareProviderEntity>> GetActiveProvidersAsync()
        {
            try
            {
                return await _dbContext.CareProviders
                    .Include(c => c.PrimaryContactEntity)
                    .Include(c => c.DiscountPoliciesEntities)
                    .Where(c => c.Status == "Active")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting active providers: {ex.Message}");
                throw;
            }
        }

        public async Task<CareProviderEntity> CreateProviderAsync(CareProviderEntity provider)
        {
            try
            {
                provider.CreatedDate = DateTime.UtcNow;
                provider.ModifiedDate = DateTime.UtcNow;

                _dbContext.CareProviders.Add(provider);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Provider created: {provider.ProviderId}");
                return provider;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating provider: {ex.Message}");
                throw;
            }
        }

        public async Task<CareProviderEntity> UpdateProviderAsync(CareProviderEntity provider)
        {
            try
            {
                provider.ModifiedDate = DateTime.UtcNow;

                _dbContext.CareProviders.Update(provider);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Provider updated: {provider.ProviderId}");
                return provider;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating provider: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteProviderAsync(int id)
        {
            try
            {
                var provider = await _dbContext.CareProviders.FindAsync(id);
                if (provider == null)
                    return false;

                // Soft delete
                provider.Status = "Inactive";
                provider.ModifiedDate = DateTime.UtcNow;

                _dbContext.CareProviders.Update(provider);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Provider deleted (soft): {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting provider {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<PurchaseHistoryEntity> AddPurchaseHistoryAsync(PurchaseHistoryEntity history)
        {
            try
            {
                history.CreatedDate = DateTime.UtcNow;
                history.OrderTimestamp = DateTime.UtcNow;

                _dbContext.PurchaseHistory.Add(history);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Purchase history added: {history.OrderId}");
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding purchase history: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PurchaseHistoryEntity>> GetPurchaseHistoryAsync(int careProviderId)
        {
            try
            {
                return await _dbContext.PurchaseHistory
                    .Where(p => p.CareProviderId == careProviderId)
                    .OrderByDescending(p => p.OrderTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting purchase history for provider {careProviderId}: {ex.Message}");
                throw;
            }
        }

        public async Task<DiscountPolicyEntity> AddDiscountPolicyAsync(DiscountPolicyEntity discount)
        {
            try
            {
                discount.CreatedDate = DateTime.UtcNow;

                _dbContext.DiscountPolicies.Add(discount);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Discount policy added for provider {discount.CareProviderId}");
                return discount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding discount policy: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DiscountPolicyEntity>> GetActiveDiscountsAsync(int careProviderId)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _dbContext.DiscountPolicies
                    .Where(d => d.CareProviderId == careProviderId &&
                               d.IsActive &&
                               d.StartDate <= now &&
                               d.EndDate >= now)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting active discounts for provider {careProviderId}: {ex.Message}");
                throw;
            }
        }

        public async Task<OrderEntity> CreateOrderAsync(OrderEntity order)
        {
            try
            {
                order.CreatedDate = DateTime.UtcNow;
                order.ModifiedDate = DateTime.UtcNow;

                _dbContext.Add(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Order created: {order.OrderId}");
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                throw;
            }
        }

        public async Task<OrderEntity> UpdateOrderAsync(OrderEntity order)
        {
            try
            {
                order.ModifiedDate = DateTime.UtcNow;

                _dbContext.Update(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Order updated: {order.OrderId}");
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order: {ex.Message}");
                throw;
            }
        }

        public async Task<OrderEntity> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Set<OrderEntity>()
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting order {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OrderEntity>> GetOrdersByProviderAsync(int careProviderId)
        {
            try
            {
                return await _dbContext.Set<OrderEntity>()
                    .Where(o => o.CareProviderId == careProviderId)
                    .OrderByDescending(o => o.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting orders for provider {careProviderId}: {ex.Message}");
                throw;
            }
        }
    }
}
