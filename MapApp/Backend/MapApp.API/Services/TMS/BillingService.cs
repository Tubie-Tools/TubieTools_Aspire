using MapApp.API.Data;
using MapApp.API.Models.TMS;
using Microsoft.EntityFrameworkCore;

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

        calculation.FuelPricePerGallon = fuelPrice;

        // Industry standard truck MPG
        const double truckMPG = 6.5;
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

public class FuelCostCalculation
{
    public string ShipmentId { get; set; } = string.Empty;
    public double Distance { get; set; }
    public decimal FuelPricePerGallon { get; set; }
    public double GallonsRequired { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal CostPerMile { get; set; }
}

public class FuelEfficiencyMetrics
{
    public string TruckId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double ActualMPG { get; set; }
    public double TotalMiles { get; set; }
    public double TotalGallonsUsed { get; set; }
    public double TotalFuelCost { get; set; }
    public double CostPerMile { get; set; }
    public double EfficiencyVariance { get; set; } // % above/below benchmark
    public bool IsBelowBenchmark { get; set; }
}

/// <summary>
/// Code-to-Cash billing service
/// Manages the revenue recognition and billing process
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// Calculate total revenue for shipment
    /// </summary>
    Task<decimal> CalculateTotalRevenueAsync(Shipment shipment);

    /// <summary>
    /// Generate billing record from completed shipment
    /// </summary>
    Task<ShipmentBillingRecord> GenerateBillingRecordAsync(Shipment shipment);

    /// <summary>
    /// Validate billing record accuracy
    /// </summary>
    Task<BillingValidation> ValidateBillingRecordAsync(BillingRecord record);

    /// <summary>
    /// Calculate revenue recognition for accounting
    /// </summary>
    Task<RevenueRecognition> CalculateRevenueRecognitionAsync(ShipmentBillingRecord record);

    /// <summary>
    /// Process payment and mark billing as complete
    /// </summary>
    Task<PaymentProcessResult> ProcessPaymentAsync(string billingId, decimal amountReceived);
}

public class BillingService : IBillingService
{
    private readonly MapAppDbContext _context;
    private readonly IFuelMetricsService _fuelService;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        MapAppDbContext context,
        IFuelMetricsService fuelService,
        ILogger<BillingService> logger)
    {
        _context = context;
        _fuelService = fuelService;
        _logger = logger;
    }

    public async Task<decimal> CalculateTotalRevenueAsync(Shipment shipment)
    {
        _logger.LogInformation("Calculating total revenue for shipment {ShipmentId}", shipment.ShipmentId);

        var total = shipment.BaseRate;

        // Add fuel surcharge
        total += shipment.FuelSurcharge;

        // Add accessorials
        total += shipment.AdditionalCharges;

        return total;
    }

    public async Task<ShipmentBillingRecord> GenerateBillingRecordAsync(Shipment shipment)
    {
        _logger.LogInformation("Generating billing record for shipment {ShipmentId}", shipment.ShipmentId);

        var record = new ShipmentBillingRecord
        {
            ShipmentId = shipment.ShipmentId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
            CustomerName = "Customer", // Would get from shipment detail
            CustomerCode = "CUST001", // Would get from shipment detail
            ShipmentDate = shipment.ActualPickupTime ?? DateTime.UtcNow,
            BillingDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30), // Net 30 terms
            Status = BillingRecordStatus.Draft
        };

        // Linehaul (distance-based rate)
        var distance = shipment.ActualDistanceMiles ?? shipment.PlannedDistanceMiles;
        const decimal ratePerMile = 2.50m;
        record.BaseLineHaul = (decimal)distance * ratePerMile;

        // Fuel surcharge
        record.FuelSurcharge = shipment.FuelSurcharge;

        // Accessorials
        record.AccessorialCharges = shipment.AdditionalCharges;

        // Calculate tax
        record.TaxableAmount = record.BaseLineHaul + record.FuelSurcharge + record.AccessorialCharges;
        record.TaxAmount = record.TaxableAmount * 0.08m; // 8% (varies by state)

        record.TotalInvoiceAmount = record.TaxableAmount + record.TaxAmount;

        _logger.LogInformation("Generated billing record {InvoiceNumber}: ${Amount}",
            record.InvoiceNumber, record.TotalInvoiceAmount);

        return record;
    }

    public async Task<BillingValidation> ValidateBillingRecordAsync(ShipmentBillingRecord record)
    {
        _logger.LogInformation("Validating billing record {InvoiceNumber}", record.InvoiceNumber);

        var validation = new BillingValidation
        {
            InvoiceNumber = record.InvoiceNumber,
            IsValid = true,
            Validations = new()
        };

        // Check invoice number format
        if (string.IsNullOrEmpty(record.InvoiceNumber) || !record.InvoiceNumber.StartsWith("INV-"))
        {
            validation.IsValid = false;
            validation.Validations.Add("Invalid invoice number format");
        }
        else
        {
            validation.Validations.Add("✓ Invoice number valid");
        }

        // Check customer information
        if (string.IsNullOrEmpty(record.CustomerName) || string.IsNullOrEmpty(record.CustomerCode))
        {
            validation.IsValid = false;
            validation.Validations.Add("Missing customer information");
        }
        else
        {
            validation.Validations.Add("✓ Customer information complete");
        }

        // Check amounts
        if (record.BaseLineHaul <= 0)
        {
            validation.IsValid = false;
            validation.Validations.Add("Invalid linehaul amount");
        }
        else
        {
            validation.Validations.Add($"✓ Linehaul: ${record.BaseLineHaul}");
        }

        // Verify tax calculation
        var expectedTax = record.TaxableAmount * 0.08m;
        if (Math.Abs(record.TaxAmount - expectedTax) > 0.01m)
        {
            validation.IsValid = false;
            validation.Validations.Add($"Tax calculation error: ${record.TaxAmount} vs expected ${expectedTax}");
        }
        else
        {
            validation.Validations.Add("✓ Tax calculation correct");
        }

        // Verify total
        var expectedTotal = record.TaxableAmount + record.TaxAmount;
        if (Math.Abs(record.TotalInvoiceAmount - expectedTotal) > 0.01m)
        {
            validation.IsValid = false;
            validation.Validations.Add("Total amount mismatch");
        }
        else
        {
            validation.Validations.Add("✓ Total amount correct");
        }

        return validation;
    }

    public async Task<RevenueRecognition> CalculateRevenueRecognitionAsync(ShipmentBillingRecord record)
    {
        _logger.LogInformation("Calculating revenue recognition for {InvoiceNumber}", record.InvoiceNumber);

        var recognition = new RevenueRecognition
        {
            InvoiceNumber = record.InvoiceNumber,
            RecognitionDate = DateTime.UtcNow
        };

        // ASC 606 Revenue Recognition - transportation revenue recognized upon delivery
        recognition.RevenueAmount = record.TotalInvoiceAmount;
        recognition.RevenueRecognitionMethod = "Upon service completion (delivery)";

        // Break down by category
        recognition.LineHaulRevenue = record.BaseLineHaul;
        recognition.FuelSurchargeRevenue = record.FuelSurcharge;
        recognition.AccessorialRevenue = record.AccessorialCharges;

        return recognition;
    }

    public async Task<PaymentProcessResult> ProcessPaymentAsync(string billingId, decimal amountReceived)
    {
        _logger.LogInformation("Processing payment for billing {BillingId}: ${Amount}", billingId, amountReceived);

        var result = new PaymentProcessResult
        {
            BillingId = billingId,
            AmountReceived = amountReceived,
            ProcessDate = DateTime.UtcNow
        };

        // In production, would integrate with accounting system
        result.IsSuccessful = true;
        result.ConfirmationNumber = Guid.NewGuid().ToString();

        return result;
    }

    public Task<BillingValidation> ValidateBillingRecordAsync(BillingRecord record)
    {
        throw new NotImplementedException();
    }
}

public class BillingValidation
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Validations { get; set; } = new();
    public object BillingId { get; internal set; }
    public List<string> ValidationMessages { get; internal set; }
    public List<string> Issues { get; internal set; }
    public string Severity { get; internal set; }
    public string Recommendation { get; internal set; }
}

public class RevenueRecognition
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime RecognitionDate { get; set; }
    public decimal RevenueAmount { get; set; }
    public string RevenueRecognitionMethod { get; set; } = string.Empty;
    public decimal LineHaulRevenue { get; set; }
    public decimal FuelSurchargeRevenue { get; set; }
    public decimal AccessorialRevenue { get; set; }
}

public class PaymentProcessResult
{
    public string BillingId { get; set; } = string.Empty;
    public decimal AmountReceived { get; set; }
    public DateTime ProcessDate { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? ErrorMessage { get; set; }
}
