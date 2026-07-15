namespace TubieTools_PublicAPI.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TubieTools_PublicAPI.Models;

    /// <summary>
    /// Interface for pricing calculations and quote generation
    /// </summary>
    public interface IPricingCalculationService
    {
        /// <summary>
        /// Calculate total price with discounts applied
        /// </summary>
        Task<PricingQuote> CalculateQuoteAsync(int careProviderId, int quantity, string productType, decimal basePrice, string promoCode = null);

        /// <summary>
        /// Apply volume discount based on quantity
        /// </summary>
        Task<decimal> ApplyVolumeDiscountAsync(int careProviderId, int quantity);

        /// <summary>
        /// Apply tier-based discount
        /// </summary>
        Task<decimal> ApplyTierDiscountAsync(CareProviderTier tier);

        /// <summary>
        /// Apply promo code discount
        /// </summary>
        Task<decimal> ApplyPromoCodeAsync(string promoCode, CareProviderTier tier, int quantity);

        /// <summary>
        /// Validate promo code
        /// </summary>
        Task<bool> ValidatePromoCodeAsync(string promoCode);

        /// <summary>
        /// Get applicable discounts for a provider
        /// </summary>
        Task<List<ApplicableDiscount>> GetApplicableDiscountsAsync(int careProviderId, int quantity);
    }

    /// <summary>
    /// Pricing quote response
    /// </summary>
    public class PricingQuote
    {
        public decimal BasePrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VolumeDiscount { get; set; }
        public decimal TierDiscount { get; set; }
        public decimal PromoDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal FinalPrice { get; set; }
        public string PromoCode { get; set; }
        public List<string> AppliedDiscounts { get; set; }
    }

    /// <summary>
    /// Applicable discount information
    /// </summary>
    public class ApplicableDiscount
    {
        public string DiscountType { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}
