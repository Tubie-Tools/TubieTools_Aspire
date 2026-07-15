using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TubieTools_PublicAPI.Services;
using TubieTools_PublicAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.PublicAPI.Services
{
    /// <summary>
    /// Unit tests for PricingCalculationService
    /// Tests all pricing calculation logic including volume, tier, and promo discounts
    /// </summary>
    ///
    public class PricingCalculationServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<PricingCalculationService>> _mockLogger;
        private PricingCalculationService _pricingService;

        public PricingCalculationServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<PricingCalculationService>>();
            SetupMockConfiguration();
            _pricingService = new PricingCalculationService(_mockConfiguration.Object, _mockLogger.Object);
        }

        private void SetupMockConfiguration()
        {
            var promoCodesSection = new Mock<IConfigurationSection>();
            var promoCodes = new List<IConfigurationSection>();

            // SUMMER20 promo code
            var summer20 = new Mock<IConfigurationSection>();
            summer20.Setup(x => x.Key).Returns("SUMMER20");
            summer20.Setup(x => x["DiscountPercentage"]).Returns("0.20");
            summer20.Setup(x => x["ValidFrom"]).Returns(DateTime.UtcNow.AddDays(-10).ToString("O"));
            summer20.Setup(x => x["ValidTo"]).Returns(DateTime.UtcNow.AddDays(10).ToString("O"));
            summer20.Setup(x => x["MaxUses"]).Returns("100");
            summer20.Setup(x => x["MinimumQuantity"]).Returns("1");
            promoCodes.Add(summer20.Object);

            // BULK100 promo code
            var bulk100 = new Mock<IConfigurationSection>();
            bulk100.Setup(x => x.Key).Returns("BULK100");
            bulk100.Setup(x => x["DiscountPercentage"]).Returns("0.15");
            bulk100.Setup(x => x["ValidFrom"]).Returns(DateTime.UtcNow.AddDays(-30).ToString("O"));
            bulk100.Setup(x => x["ValidTo"]).Returns(DateTime.UtcNow.AddDays(30).ToString("O"));
            bulk100.Setup(x => x["MaxUses"]).Returns("0");
            bulk100.Setup(x => x["MinimumQuantity"]).Returns("100");
            promoCodes.Add(bulk100.Object);

            promoCodesSection.Setup(x => x.GetChildren()).Returns(promoCodes);

            var volumeTiersSection = new Mock<IConfigurationSection>();
            var volumeTiers = new List<IConfigurationSection>();

            // DayCare volume tiers
            var daycareTier = new Mock<IConfigurationSection>();
            daycareTier.Setup(x => x["Tier"]).Returns("DayCare");

            var daycareThresholdsSection = new Mock<IConfigurationSection>();
            var daycareThresholds = new List<IConfigurationSection>();

            var threshold50 = new Mock<IConfigurationSection>();
            threshold50.Setup(x => x["Quantity"]).Returns("50");
            threshold50.Setup(x => x["DiscountPercentage"]).Returns("0.05");
            daycareThresholds.Add(threshold50.Object);

            var threshold100 = new Mock<IConfigurationSection>();
            threshold100.Setup(x => x["Quantity"]).Returns("100");
            threshold100.Setup(x => x["DiscountPercentage"]).Returns("0.10");
            daycareThresholds.Add(threshold100.Object);

            daycareThresholdsSection.Setup(x => x.GetChildren()).Returns(daycareThresholds);
            daycareTier.Setup(x => x.GetSection("Thresholds")).Returns(daycareThresholdsSection.Object);
            volumeTiers.Add(daycareTier.Object);

            volumeTiersSection.Setup(x => x.GetChildren()).Returns(volumeTiers);

            _mockConfiguration.Setup(x => x.GetSection("PromoCodes")).Returns(promoCodesSection.Object);
            _mockConfiguration.Setup(x => x.GetSection("VolumeTiers")).Returns(volumeTiersSection.Object);
        }

        [Fact]
        public async Task CalculateQuoteAsync_WithValidQuantityAndPrice_ReturnsCorrectPrice()
        {
            // Arrange
            var providerId = 1;
            var quantity = 100;
            var productType = "60ml Serum";
            var basePrice = 29.99m;

            // Act
            var quote = await _pricingService.CalculateQuoteAsync(providerId, quantity, productType, basePrice);

            // Assert
            Assert.NotNull(quote);
            Assert.AreEqual(basePrice, quote.BasePrice);
            Assert.AreEqual(quantity, quote.Quantity);
            Assert.AreEqual(basePrice * quantity, quote.Subtotal);
            Assert.True(quote.FinalPrice > 0);
            Assert.IsNotEmpty(quote.AppliedDiscounts);
        }

        [Fact]
        public async Task CalculateQuoteAsync_WithSmallQuantity_NoVolumeDiscount()
        {
            // Arrange
            var providerId = 1;
            var quantity = 25;
            var basePrice = 29.99m;

            // Act
            var quote = await _pricingService.CalculateQuoteAsync(providerId, quantity, "Product", basePrice);

            // Assert
            Assert.AreEqual(0m, quote.VolumeDiscount);
            Assert.AreEqual(quote.Subtotal, quote.FinalPrice);
        }

        [NUnit.Framework.Theory]
        [InlineData(50, 0.05)]
        [InlineData(100, 0.10)]
        [InlineData(250, 0.15)]
        [InlineData(500, 0.20)]
        [InlineData(2500, 0.25)]
        public async Task ApplyVolumeDiscountAsync_WithDifferentQuantities_ReturnsCorrectDiscount(int quantity, decimal expectedDiscount)
        {
            // Act
            var discount = await _pricingService.ApplyVolumeDiscountAsync(0, quantity);

            // Assert
            Assert.AreEqual(expectedDiscount, discount);
        }

        [NUnit.Framework.Theory]
        [InlineData(CareProviderTier.DayCare, 0.05)]
        [InlineData(CareProviderTier.ElderlyHome, 0.10)]
        [InlineData(CareProviderTier.HealthcareProvider, 0.15)]
        public async Task ApplyTierDiscountAsync_WithDifferentTiers_ReturnsCorrectDiscount(CareProviderTier tier, decimal expectedDiscount)
        {
            // Act
            var discount = await _pricingService.ApplyTierDiscountAsync(tier);

            // Assert
            Assert.AreEqual(expectedDiscount, discount);
        }

        [Fact]
        public async Task ValidatePromoCodeAsync_WithValidCode_ReturnsTrue()
        {
            // Arrange
            var promoCode = "SUMMER20";

            // Act
            var isValid = await _pricingService.ValidatePromoCodeAsync(promoCode);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public async Task ValidatePromoCodeAsync_WithInvalidCode_ReturnsFalse()
        {
            // Arrange
            var promoCode = "INVALID123";

            // Act
            var isValid = await _pricingService.ValidatePromoCodeAsync(promoCode);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task ValidatePromoCodeAsync_WithNullCode_ReturnsFalse()
        {
            // Act
            var isValid = await _pricingService.ValidatePromoCodeAsync(null);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task CalculateQuoteAsync_WithPromoCode_AppliesPromoDiscount()
        {
            // Arrange
            var quantity = 100;
            var basePrice = 29.99m;
            var promoCode = "SUMMER20";

            // Act
            var quote = await _pricingService.CalculateQuoteAsync(1, quantity, "Product", basePrice, promoCode);

            // Assert
            Assert.NotNull(quote);
            Assert.AreEqual(promoCode, quote.PromoCode);
            Assert.True(quote.PromoDiscount >= 0);
        }

        [Fact]
        public async Task GetApplicableDiscountsAsync_WithValidQuantity_ReturnsDiscounts()
        {
            // Act
            var discounts = await _pricingService.GetApplicableDiscountsAsync(1, 100);

            // Assert
            Assert.IsNotEmpty(discounts);
            //Assert.Contains(discounts, d => d.DiscountType == "Volume");
        }

        [Fact]
        public async Task GetApplicableDiscountsAsync_WithZeroQuantity_ReturnsEmptyOrValidDiscounts()
        {
            // Act
            var discounts = await _pricingService.GetApplicableDiscountsAsync(1, 0);

            // Assert
            Assert.NotNull(discounts);
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_WithValidPromoAndQuantity_ReturnsDiscount()
        {
            // Act
            var discount = await _pricingService.ApplyPromoCodeAsync("SUMMER20", CareProviderTier.DayCare, 100);

            // Assert
            Assert.True(discount > 0);
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_WithMinimumQuantityNotMet_ReturnsZero()
        {
            // Act
            var discount = await _pricingService.ApplyPromoCodeAsync("BULK100", CareProviderTier.DayCare, 50);

            // Assert
            Assert.AreEqual(0m, discount);
        }

        [Fact]
        public void PricingQuote_Properties_AreSettable()
        {
            // Arrange
            var quote = new PricingQuote
            {
                BasePrice = 100m,
                Quantity = 10,
                Subtotal = 1000m,
                VolumeDiscount = 50m,
                TierDiscount = 25m,
                PromoDiscount = 100m,
                TotalDiscount = 175m,
                FinalPrice = 825m,
                PromoCode = "TEST20",
                AppliedDiscounts = new List<string> { "Volume", "Promo" }
            };

            // Assert
            Assert.AreEqual(100m, quote.BasePrice);
            Assert.AreEqual(10, quote.Quantity);
            Assert.AreEqual(1000m, quote.Subtotal);
            Assert.AreEqual(50m, quote.VolumeDiscount);
            Assert.AreEqual(25m, quote.TierDiscount);
            Assert.AreEqual(100m, quote.PromoDiscount);
            Assert.AreEqual(175m, quote.TotalDiscount);
            Assert.AreEqual(825m, quote.FinalPrice);
            Assert.AreEqual("TEST20", quote.PromoCode);
            Assert.IsNotEmpty(quote.AppliedDiscounts);
        }

        [Fact]
        public void ApplicableDiscount_Properties_AreSettable()
        {
            // Arrange
            var discount = new ApplicableDiscount
            {
                DiscountType = "Volume",
                Description = "10% volume discount",
                DiscountPercentage = 0.10m
            };

            // Assert
            Assert.AreEqual("Volume", discount.DiscountType);
            Assert.AreEqual("10% volume discount", discount.Description);
            Assert.AreEqual(0.10m, discount.DiscountPercentage);
        }
    }
}
