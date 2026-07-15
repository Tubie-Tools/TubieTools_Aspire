namespace TubieTools_PublicAPI.Services
{
    using TubieTools_PublicAPI.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of pricing calculation service
    /// </summary>
    public class PricingCalculationService : IPricingCalculationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PricingCalculationService> _logger;
        private Dictionary<string, PromoCodeConfig> _promoCodes;
        private Dictionary<string, VolumeTierConfig> _volumeTiers;

        public PricingCalculationService(IConfiguration configuration, ILogger<PricingCalculationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            try
            {
                _promoCodes = new Dictionary<string, PromoCodeConfig>();
                _volumeTiers = new Dictionary<string, VolumeTierConfig>();

                var promoCodesSection = _configuration.GetSection("PromoCodes");
                foreach (var child in promoCodesSection.GetChildren())
                {
                    var promoCode = new PromoCodeConfig
                    {
                        Code = child.Key,
                        DiscountPercentage = decimal.Parse(child["DiscountPercentage"] ?? "0"),
                        ValidFrom = DateTime.Parse(child["ValidFrom"] ?? DateTime.UtcNow.ToString()),
                        ValidTo = DateTime.Parse(child["ValidTo"] ?? DateTime.UtcNow.AddYears(1).ToString()),
                        MaxUses = int.Parse(child["MaxUses"] ?? "0"),
                        MinimumQuantity = int.Parse(child["MinimumQuantity"] ?? "1")
                    };
                    _promoCodes[child.Key] = promoCode;
                }

                var volumeTiersSection = _configuration.GetSection("VolumeTiers");
                foreach (var tier in volumeTiersSection.GetChildren())
                {
                    var tierName = tier["Tier"];
                    var thresholds = new List<VolumeThreshold>();

                    var thresholdsSection = tier.GetSection("Thresholds");
                    foreach (var threshold in thresholdsSection.GetChildren())
                    {
                        thresholds.Add(new VolumeThreshold
                        {
                            Quantity = int.Parse(threshold["Quantity"] ?? "0"),
                            DiscountPercentage = decimal.Parse(threshold["DiscountPercentage"] ?? "0")
                        });
                    }

                    _volumeTiers[tierName] = new VolumeTierConfig { Thresholds = thresholds };
                }

                _logger.LogInformation("Pricing configuration loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading pricing configuration: {ex.Message}");
            }
        }

        public async Task<PricingQuote> CalculateQuoteAsync(int careProviderId, int quantity, string productType, decimal basePrice, string promoCode = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var quote = new PricingQuote
                    {
                        BasePrice = basePrice,
                        Quantity = quantity,
                        Subtotal = basePrice * quantity,
                        AppliedDiscounts = new List<string>()
                    };

                    // Calculate volume discount
                    var volumeDiscount = 0m;
                    if (quantity >= 50)
                    {
                        volumeDiscount = quantity switch
                        {
                            >= 500 => 0.20m,
                            >= 250 => 0.15m,
                            >= 100 => 0.10m,
                            >= 50 => 0.05m,
                            _ => 0m
                        };
                    }

                    quote.VolumeDiscount = quote.Subtotal * volumeDiscount;
                    quote.AppliedDiscounts.Add($"Volume ({volumeDiscount * 100}%)");

                    // Apply promo code if provided
                    var promoDiscount = 0m;
                    if (!string.IsNullOrEmpty(promoCode) && _promoCodes.TryGetValue(promoCode, out var promo))
                    {
                        if (DateTime.UtcNow >= promo.ValidFrom && DateTime.UtcNow <= promo.ValidTo && quantity >= promo.MinimumQuantity)
                        {
                            promoDiscount = (quote.Subtotal - quote.VolumeDiscount) * promo.DiscountPercentage;
                            quote.PromoCode = promoCode;
                            quote.AppliedDiscounts.Add($"Promo: {promoCode} ({promo.DiscountPercentage * 100}%)");
                        }
                    }

                    quote.PromoDiscount = promoDiscount;
                    quote.TotalDiscount = quote.VolumeDiscount + quote.PromoDiscount;
                    quote.FinalPrice = quote.Subtotal - quote.TotalDiscount;

                    _logger.LogInformation($"Quote calculated for provider {careProviderId}: {quantity} units, Final Price: {quote.FinalPrice}");
                    return quote;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error calculating quote: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task<decimal> ApplyVolumeDiscountAsync(int careProviderId, int quantity)
        {
            return await Task.Run(() =>
            {
                return quantity switch
                {
                    >= 2500 => 0.25m,
                    >= 1000 => 0.20m,
                    >= 500 => 0.15m,
                    >= 250 => 0.10m,
                    >= 100 => 0.05m,
                    >= 50 => 0.02m,
                    _ => 0m
                };
            });
        }

        public async Task<decimal> ApplyTierDiscountAsync(CareProviderTier tier)
        {
            return await Task.Run(() =>
            {
                return tier switch
                {
                    CareProviderTier.DayCare => 0.05m,
                    CareProviderTier.ElderlyHome => 0.10m,
                    CareProviderTier.HealthcareProvider => 0.15m,
                    _ => 0m
                };
            });
        }

        public async Task<decimal> ApplyPromoCodeAsync(string promoCode, CareProviderTier tier, int quantity)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(promoCode) || !_promoCodes.TryGetValue(promoCode, out var promo))
                    return 0m;

                var now = DateTime.UtcNow;
                if (now < promo.ValidFrom || now > promo.ValidTo)
                    return 0m;

                if (quantity < promo.MinimumQuantity)
                    return 0m;

                return promo.DiscountPercentage;
            });
        }

        public async Task<bool> ValidatePromoCodeAsync(string promoCode)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(promoCode))
                    return false;

                if (!_promoCodes.TryGetValue(promoCode, out var promo))
                    return false;

                var now = DateTime.UtcNow;
                return now >= promo.ValidFrom && now <= promo.ValidTo;
            });
        }

        public async Task<List<ApplicableDiscount>> GetApplicableDiscountsAsync(int careProviderId, int quantity)
        {
            return await Task.Run(() =>
            {
                var discounts = new List<ApplicableDiscount>();

                // Volume discount
                if (quantity >= 50)
                {
                    var volumeDiscount = quantity switch
                    {
                        >= 500 => 0.20m,
                        >= 250 => 0.15m,
                        >= 100 => 0.10m,
                        >= 50 => 0.05m,
                        _ => 0m
                    };

                    if (volumeDiscount > 0)
                    {
                        discounts.Add(new ApplicableDiscount
                        {
                            DiscountType = "Volume",
                            Description = $"Volume discount for {quantity} units",
                            DiscountPercentage = volumeDiscount
                        });
                    }
                }

                // Active promo codes
                foreach (var promo in _promoCodes.Values)
                {
                    var now = DateTime.UtcNow;
                    if (now >= promo.ValidFrom && now <= promo.ValidTo && quantity >= promo.MinimumQuantity)
                    {
                        discounts.Add(new ApplicableDiscount
                        {
                            DiscountType = "Promo",
                            Description = $"Promo code: {promo.Code}",
                            DiscountPercentage = promo.DiscountPercentage
                        });
                    }
                }

                return discounts;
            });
        }

        private class PromoCodeConfig
        {
            public string Code { get; set; }
            public decimal DiscountPercentage { get; set; }
            public DateTime ValidFrom { get; set; }
            public DateTime ValidTo { get; set; }
            public int MaxUses { get; set; }
            public int MinimumQuantity { get; set; }
        }

        private class VolumeTierConfig
        {
            public List<VolumeThreshold> Thresholds { get; set; }
        }

        private class VolumeThreshold
        {
            public int Quantity { get; set; }
            public decimal DiscountPercentage { get; set; }
        }
    }
}
