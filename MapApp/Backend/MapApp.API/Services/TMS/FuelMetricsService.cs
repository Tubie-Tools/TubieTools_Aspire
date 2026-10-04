using MapApp.API.Data;
using MapApp.API.Models.TMS;
using Microsoft.EntityFrameworkCore;

namespace MapApp.API.Services.TMS;

public class FuelMetricsService : IFuelMetricsService
{
    private readonly MapAppDbContext _context;
    private readonly ILogger<FuelMetricsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    // Base fuel price (would be pulled from API in production - AAA, EIA)
    private decimal _currentFuelPrice = 3.50m;

    public FuelMetricsService(
        MapAppDbContext context,
        ILogger<FuelMetricsService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<decimal> GetCurrentFuelPriceAsync(double latitude, double longitude)
    {
        _logger.LogInformation("Getting fuel price for location ({Lat}, {Lon})", latitude, longitude);

        // In production, would query API like:
        // - EIA (Energy Information Administration)
        // - AAA Fuel Gauge Report
        // - Local gas station prices

        // For demo, return base price with regional variance
        var price = _currentFuelPrice;

        // Simulated regional variance
        if (latitude > 40) price += 0.25m; // Northeast premium
        if (latitude < 32) price -= 0.15m; // South discount

        return price;
    }

    public async Task<FuelCostCalculation> CalculateFuelCostAsync(Shipment shipment)
    {
        var calculation = new FuelCostCalculation
        {
            ShipmentId = shipment.ShipmentId,
            Distance = shipment.PlannedDistanceMiles
        };

        // Get fuel price at origin
        var fuelPrice = await GetCurrentFuelPriceAsync(
            shipment.PickupScheduledTime.Millisecond, // Placeholder lat
            shipment.PickupScheduledTime.Millisecond); // Placeholder lon

        calculation.FuelPrice = fuelPrice;

        // Industry standard truck MPG
        const decimal truckMPG = 6.5m;
        calculation.GallonsRequired = calculation.Distance / truckMPG;
        calculation.TotalFuelCost = (decimal)calculation.GallonsRequired * fuelPrice;

        // Calculate cost per mile
        calculation.CostPerMile = calculation.Distance > 0 ? 
            calculation.TotalFuelCost / (decimal)calculation.Distance : 0;

        _logger.LogInformation("Fuel cost for shipment {ShipmentId}: {Distance} mi * {MPG} = {Gallons} gal @ ${Price}/gal = ${Cost}",
            shipment.ShipmentId, calculation.Distance, truckMPG, 
            calculation.GallonsRequired, fuelPrice, calculation.TotalFuelCost);

        return calculation;
    }

    public async Task<decimal> CalculateFuelSurchargeAsync(double distanceMiles, decimal fuelPrice)
    {
        // Industry standard: fuel surcharge based on national fuel index
        // Typically: 0.06% per $0.01 change from base price
        const decimal basePrice = 2.50m;
        const double baseMPG = 6.5;
        const decimal baseSurcharge = 0.15m; // $0.15/mile base

        var priceVariance = fuelPrice - basePrice;
        var surchargePercentage = priceVariance * 0.06m; // 6% per $0.01

        var surcharge = baseSurcharge * (1 + surchargePercentage);
        var totalSurcharge = (decimal)distanceMiles * surcharge;

        return totalSurcharge;
    }

    public async Task UpdateFuelPriceIndexAsync()
    {
        _logger.LogInformation("Updating national fuel price index");

        // In production, would call EIA API
        // Department of Energy publishes weekly data
        // AAA publishes daily data

        // Simulate realistic variation: ±10% around base
        var random = new Random();
        var variance = (decimal)(random.NextDouble() - 0.5) * 0.20m;
        _currentFuelPrice = 3.50m * (1 + variance);

        _logger.LogInformation("Updated fuel price to ${Price:F2}/gal", _currentFuelPrice);
    }

    public async Task<FuelEfficiencyMetrics> CalculateFuelEfficiencyAsync(string truckId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Calculating fuel efficiency for truck {TruckId} from {Start} to {End}",
            truckId, startDate, endDate);

        var metrics = new FuelEfficiencyMetrics
        {
            TruckId = truckId,
            StartDate = startDate,
            EndDate = endDate
        };

        // Get shipments for truck in date range
        var truck = await _context.Trucks.FirstOrDefaultAsync(t => t.TruckId == truckId);
        if (truck == null) return metrics;

        // Simulated calculation (would aggregate actual telematics data)
        metrics.ActualMPG = truck.AverageMPG;
        metrics.TotalMiles = truck.TotalCurrentMiles;
        metrics.TotalGallonsUsed = metrics.TotalMiles / metrics.ActualMPG;
        metrics.TotalFuelCost = metrics.TotalGallonsUsed * (double)_currentFuelPrice;
        metrics.CostPerMile = metrics.TotalMiles > 0 ? 
            metrics.TotalFuelCost / metrics.TotalMiles : 0;

        // Industry benchmark: 6.0-7.0 MPG
        const double benchmark = 6.5;
        metrics.EfficiencyVariance = ((metrics.ActualMPG - benchmark) / benchmark) * 100;
        metrics.IsBelowBenchmark = metrics.ActualMPG < benchmark;

        _logger.LogInformation("Truck {TruckId} fuel efficiency: {MPG} MPG ({Variance:+0.0;-0.0}%)",
            truckId, metrics.ActualMPG, metrics.EfficiencyVariance);

        return metrics;
    }
}
