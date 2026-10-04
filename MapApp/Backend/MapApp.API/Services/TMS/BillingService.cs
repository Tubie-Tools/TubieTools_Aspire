using MapApp.API.Data;
using MapApp.API.Models.TMS;

namespace MapApp.API.Services.TMS;

/// <summary>
/// 
/// </summary>
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
