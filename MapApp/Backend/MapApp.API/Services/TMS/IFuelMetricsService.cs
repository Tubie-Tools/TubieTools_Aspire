using MapApp.API.Models.TMS;

namespace MapApp.API.Services.TMS;

/// <summary>
/// Fuel metrics tracking for real-time fuel price and economy
/// Schneider tracks fuel as major cost driver
/// </summary>
public interface IFuelMetricsService
{
    /// <summary>
    /// Get current fuel price in region
    /// </summary>
    Task<decimal> GetCurrentFuelPriceAsync(double latitude, double longitude);

    /// <summary>
    /// Calculate fuel cost for shipment route
    /// </summary>
    Task<FuelCostCalculation> CalculateFuelCostAsync(Shipment shipment);

    /// <summary>
    /// Apply fuel surcharge to billing
    /// </summary>
    Task<decimal> CalculateFuelSurchargeAsync(double distanceMiles, decimal fuelPrice);

    /// <summary>
    /// Track real-time fuel price volatility
    /// </summary>
    Task UpdateFuelPriceIndexAsync();

    /// <summary>
    /// Calculate fuel efficiency metrics
    /// </summary>
    Task<FuelEfficiencyMetrics> CalculateFuelEfficiencyAsync(string truckId, DateTime startDate, DateTime endDate);
}
