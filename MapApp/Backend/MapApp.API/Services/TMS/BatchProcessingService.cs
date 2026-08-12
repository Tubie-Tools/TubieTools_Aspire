using MapApp.API.Models.TMS;
using MapApp.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MapApp.API.Services.TMS;

/// <summary>
/// Batch processing for end-of-day reconciliation
/// Handles billing, compliance audits, analytics
/// </summary>
public interface IBatchProcessingService
{
    /// <summary>
    /// End-of-day reconciliation
    /// </summary>
    Task<BatchProcessResult> ProcessEndOfDayAsync();

    /// <summary>
    /// Validate all shipments for code-to-cash accuracy
    /// </summary>
    Task<List<BillingValidationResult>> ValidateCodeToCashAsync();

    /// <summary>
    /// Generate billing records from completed shipments
    /// </summary>
    Task<List<ShipmentBillingRecord>> GenerateBillingRecordsAsync(DateTime billingDate);

    /// <summary>
    /// Audit trail for compliance
    /// </summary>
    Task<List<ComplianceAuditEntry>> GenerateComplianceAuditAsync(DateTime auditDate);

    /// <summary>
    /// Calculate performance metrics
    /// </summary>
    Task<PerformanceMetrics> CalculatePerformanceMetricsAsync(DateTime startDate, DateTime endDate);
}

public class BatchProcessingService : IBatchProcessingService
{
    private readonly MapAppDbContext _context;
    private readonly IBillingService _billingService;
    private readonly ILogger<BatchProcessingService> _logger;

    public BatchProcessingService(
        MapAppDbContext context,
        IBillingService billingService,
        ILogger<BatchProcessingService> logger)
    {
        _context = context;
        _billingService = billingService;
        _logger = logger;
    }

    public async Task<BatchProcessResult> ProcessEndOfDayAsync()
    {
        var result = new BatchProcessResult
        {
            ProcessDate = DateTime.UtcNow,
            StartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting end-of-day batch processing");

            // 1. Complete any in-transit shipments that arrived
            result.CompletedShipments = await CompleteInTransitShipmentsAsync();

            // 2. Validate all shipments for accuracy
            result.ValidationResults = await ValidateCodeToCashAsync();
            result.ValidationsPassed = result.ValidationResults.Count(v => v.IsValid);
            result.ValidationsFailed = result.ValidationResults.Count(v => !v.IsValid);

            // 3. Generate billing records
            result.BillingRecords = await GenerateBillingRecordsAsync(DateTime.UtcNow);

            // 4. Calculate daily metrics
            result.DailyMetrics = await CalculatePerformanceMetricsAsync(
                DateTime.UtcNow.Date,
                DateTime.UtcNow.Date.AddDays(1));

            // 5. Generate compliance audit
            result.ComplianceIssues = await GenerateComplianceAuditAsync(DateTime.UtcNow).ContinueWith(
                t => t.Result.Where(a => !a.IsCompliant).ToList());

            result.EndTime = DateTime.UtcNow;
            result.TotalExecutionSeconds = (result.EndTime.Value - result.StartTime).TotalSeconds;
            result.Status = BatchProcessStatus.Success;

            _logger.LogInformation("End-of-day batch processing completed in {Seconds}s",
                result.TotalExecutionSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "End-of-day batch processing failed");
            result.Status = BatchProcessStatus.Failed;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<List<BillingValidationResult>> ValidateCodeToCashAsync()
    {
        _logger.LogInformation("Starting code-to-cash validation");
        var validationResults = new List<BillingValidationResult>();

        // Get all delivered shipments from past 24 hours
        var deliveredShipments = await _context.Shipments
            .Where(s => s.Status == ShipmentStatus.Delivered &&
                       s.CompletedAt > DateTime.UtcNow.AddDays(-1))
            .ToListAsync();

        foreach (var shipment in deliveredShipments)
        {
            var validation = new BillingValidationResult
            {
                ShipmentId = shipment.ShipmentId,
                TrackingNumber = shipment.TrackingNumber,
                IsValid = true,
                Validations = new List<string>()
            };

            // 1. Verify distance calculation
            if (shipment.ActualDistanceMiles.HasValue && shipment.PlannedDistanceMiles > 0)
            {
                var variance = Math.Abs(shipment.ActualDistanceMiles.Value - shipment.PlannedDistanceMiles) /
                              shipment.PlannedDistanceMiles;

                if (variance > 0.2) // 20% variance threshold
                {
                    validation.IsValid = false;
                    validation.Validations.Add($"Distance variance excessive: {variance:P}");
                    validation.Issues.Add("DistanceVariance");
                }
                else
                {
                    validation.Validations.Add($"✓ Distance variance acceptable: {variance:P}");
                }
            }

            // 2. Verify ELD recording (Hours of Service)
            if (!shipment.ELDRecorded)
            {
                validation.IsValid = false;
                validation.Validations.Add("ELD not recorded - HOS compliance issue");
                validation.Issues.Add("MissingELD");
            }
            else
            {
                validation.Validations.Add("✓ ELD properly recorded");
            }

            // 3. Verify billing amounts
            if (shipment.TotalRevenue <= 0)
            {
                validation.IsValid = false;
                validation.Validations.Add("Invalid bill amount");
                validation.Issues.Add("InvalidBillAmount");
            }
            else
            {
                // Verify fuel surcharge is consistent with fuel metrics
                var expectedFuelCost = CalculateExpectedFuelCost(shipment);
                var fuelVariance = Math.Abs(shipment.FuelSurcharge - expectedFuelCost) / expectedFuelCost;

                if (fuelVariance > 0.1M) // 10% variance
                {
                    validation.IsValid = false;
                    validation.Validations.Add($"Fuel surcharge variance: {fuelVariance:P}");
                    validation.Issues.Add("FuelSurchargeVariance");
                }
                else
                {
                    validation.Validations.Add("✓ Fuel surcharge accurate");
                }
            }

            // 4. Verify time windows met
            if (shipment.ActualDeliveryTime.HasValue && 
                shipment.ActualDeliveryTime.Value > shipment.DeliveryScheduledTime.AddHours(2))
            {
                validation.Warnings.Add("Delivery late by >2 hours");
            }
            else if (shipment.ActualDeliveryTime.HasValue)
            {
                validation.Validations.Add("✓ Delivery time window met");
            }

            // 5. Verify customer and billing data
            if (string.IsNullOrEmpty(shipment.ShipmentId))
            {
                validation.IsValid = false;
                validation.Validations.Add("Missing shipment identification");
                validation.Issues.Add("MissingShipmentID");
            }

            validationResults.Add(validation);
        }

        var failedCount = validationResults.Count(v => !v.IsValid);
        _logger.LogInformation("Code-to-cash validation complete: {Total} shipments, {Failed} issues",
            validationResults.Count, failedCount);

        return validationResults;
    }

    public async Task<List<ShipmentBillingRecord>> GenerateBillingRecordsAsync(DateTime billingDate)
    {
        _logger.LogInformation("Generating billing records for {Date}", billingDate);
        var billingRecords = new List<ShipmentBillingRecord>();

        // Get completed shipments
        var completedShipments = await _context.Shipments
            .Where(s => s.Status == ShipmentStatus.Delivered &&
                       s.CompletedAt.HasValue &&
                       s.CompletedAt.Value.Date == billingDate.Date)
            .ToListAsync();

        foreach (var shipment in completedShipments)
        {
            var record = new ShipmentBillingRecord
            {
                ShipmentId = shipment.ShipmentId,
                InvoiceNumber = $"INV-{billingDate:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                CustomerName = "Customer", // Would get from shipment
                CustomerCode = "CUST001", // Would get from shipment
                ShipmentDate = shipment.ActualPickupTime ?? DateTime.UtcNow,
                BillingDate = billingDate,
                DueDate = billingDate.AddDays(30), // Net 30 terms
                Status = BillingRecordStatus.Draft
            };

            // Calculate linehaul (distance-based)
            var ratePerMile = 2.50m; // Industry standard, would vary by customer
            record.BaseLineHaul = (decimal)(shipment.ActualDistanceMiles ?? shipment.PlannedDistanceMiles) * ratePerMile;

            // Fuel surcharge (already calculated)
            record.FuelSurcharge = shipment.FuelSurcharge;

            // Accessorial charges (tolls, detention, etc.)
            record.AccessorialCharges = shipment.AdditionalCharges;

            // Calculate tax
            record.TaxableAmount = record.BaseLineHaul + record.FuelSurcharge + record.AccessorialCharges;
            record.TaxAmount = record.TaxableAmount * 0.08m; // 8% state tax (varies)

            record.TotalInvoiceAmount = record.TaxableAmount + record.TaxAmount;

            billingRecords.Add(record);
        }

        // Persist billing records
        await _context.AddRangeAsync(billingRecords.Cast<object>().ToList());
        await _context.SaveChangesAsync();

        _logger.LogInformation("Generated {Count} billing records", billingRecords.Count);
        return billingRecords;
    }

    public async Task<List<ComplianceAuditEntry>> GenerateComplianceAuditAsync(DateTime auditDate)
    {
        _logger.LogInformation("Generating compliance audit for {Date}", auditDate);
        var auditEntries = new List<ComplianceAuditEntry>();

        // Get all shipments and events from audit date
        var shipments = await _context.Shipments
            .Where(s => s.UpdatedAt.Date == auditDate.Date)
            .ToListAsync();

        foreach (var shipment in shipments)
        {
            var entry = new ComplianceAuditEntry
            {
                ShipmentId = shipment.ShipmentId,
                AuditDate = auditDate,
                IsCompliant = true,
                Issues = new List<string>()
            };

            // Check HOS compliance
            if (shipment.HoursOfServiceUsed > shipment.HoursOfServiceAvailable)
            {
                entry.IsCompliant = false;
                entry.Issues.Add($"HOS violation: {shipment.HoursOfServiceUsed}h used, {shipment.HoursOfServiceAvailable}h available");
            }

            // Check ELD requirement
            if (!shipment.ELDRecorded && shipment.ActualDistanceMiles.HasValue && shipment.ActualDistanceMiles > 100)
            {
                entry.IsCompliant = false;
                entry.Issues.Add("ELD not recorded for >100 mile trip");
            }

            // Check delivery window
            if (shipment.ActualDeliveryTime.HasValue &&
                shipment.ActualDeliveryTime.Value > shipment.DeliveryScheduledTime.AddHours(3))
            {
                entry.Issues.Add("Warning: Delivery >3 hours late");
            }

            auditEntries.Add(entry);
        }

        _logger.LogInformation("Compliance audit complete: {Total} entries, {NonCompliant} non-compliant",
            auditEntries.Count, auditEntries.Count(e => !e.IsCompliant));

        return auditEntries;
    }

    public async Task<PerformanceMetrics> CalculatePerformanceMetricsAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Calculating performance metrics for {Start} to {End}", startDate, endDate);

        var shipments = await _context.Shipments
            .Where(s => s.UpdatedAt >= startDate && s.UpdatedAt <= endDate)
            .ToListAsync();

        var metrics = new PerformanceMetrics
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalShipments = shipments.Count,
            CompletedShipments = shipments.Count(s => s.Status == ShipmentStatus.Delivered),
            OnTimeDeliveries = 0,
            ExceptionShipments = shipments.Count(s => s.Status == ShipmentStatus.Exception),
            TotalRevenue = 0,
            TotalFuelCost = 0,
            AverageMilesSaved = 0
        };

        // Calculate on-time deliveries
        foreach (var shipment in shipments.Where(s => s.ActualDeliveryTime.HasValue))
        {
            if (shipment.ActualDeliveryTime <= shipment.DeliveryScheduledTime.AddMinutes(15))
            {
                metrics.OnTimeDeliveries++;
            }

            metrics.TotalRevenue += shipment.TotalRevenue;
            metrics.TotalFuelCost += shipment.FuelSurcharge;

            if (shipment.ActualDistanceMiles.HasValue)
            {
                metrics.AverageMilesSaved += Math.Max(0, 
                    shipment.PlannedDistanceMiles - shipment.ActualDistanceMiles.Value);
            }
        }

        metrics.OnTimePercentage = metrics.CompletedShipments > 0 ? 
            (metrics.OnTimeDeliveries / (double)metrics.CompletedShipments) * 100 : 0;

        var completedCount = metrics.CompletedShipments;
        if (completedCount > 0)
        {
            metrics.AverageMilesSaved /= completedCount;
        }

        metrics.NetRevenue = metrics.TotalRevenue - metrics.TotalFuelCost;
        metrics.ProfitMargin = (double)(metrics.TotalRevenue > 0 ? 
            (metrics.NetRevenue / metrics.TotalRevenue) * 100 : 0);

        return metrics;
    }

    private async Task<int> CompleteInTransitShipmentsAsync()
    {
        _logger.LogInformation("Checking for completed in-transit shipments");

        var inTransitShipments = await _context.Shipments
            .Where(s => s.Status == ShipmentStatus.InTransit &&
                       s.ActualDeliveryTime.HasValue)
            .ToListAsync();

        foreach (var shipment in inTransitShipments)
        {
            shipment.Status = ShipmentStatus.Delivered;
            shipment.CompletedAt = DateTime.UtcNow;
            shipment.UpdatedAt = DateTime.UtcNow;
        }

        if (inTransitShipments.Any())
        {
            await _context.SaveChangesAsync();
        }

        return inTransitShipments.Count;
    }

    private decimal CalculateExpectedFuelCost(Shipment shipment)
    {
        const double standardMPG = 6.5;
        const decimal standardFuelPrice = 3.50m;

        var gallonsUsed = (shipment.ActualDistanceMiles ?? shipment.PlannedDistanceMiles) / standardMPG;
        return (decimal)gallonsUsed * standardFuelPrice;
    }
}

public class BatchProcessResult
{
    public DateTime ProcessDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? TotalExecutionSeconds { get; set; }
    public BatchProcessStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    // Results
    public int CompletedShipments { get; set; }
    public List<BillingValidationResult> ValidationResults { get; set; } = new();
    public int ValidationsPassed { get; set; }
    public int ValidationsFailed { get; set; }
    public List<ShipmentBillingRecord> BillingRecords { get; set; } = new();
    public PerformanceMetrics? DailyMetrics { get; set; }
    public List<ComplianceAuditEntry> ComplianceIssues { get; set; } = new();
}

public enum BatchProcessStatus
{
    Pending,
    Running,
    Success,
    Failed,
    PartialSuccess
}

public class BillingValidationResult
{
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> Validations { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ComplianceAuditEntry
{
    public string ShipmentId { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class PerformanceMetrics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalShipments { get; set; }
    public int CompletedShipments { get; set; }
    public int OnTimeDeliveries { get; set; }
    public double OnTimePercentage { get; set; }
    public int ExceptionShipments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal NetRevenue { get; set; }
    public double ProfitMargin { get; set; }
    public double AverageMilesSaved { get; set; }
}
