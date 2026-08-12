using MapApp.API.Data;
using MapApp.API.DTOs.TMS;
using MapApp.API.Models.TMS;
using MapApp.API.Services.TMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapApp.API.Controllers.TMS;

/// <summary>
/// Schneider International TMS API
/// Manages shipments, real-time events, and billing
/// </summary>
[ApiController]
[Route("api/tms")]
public class ShipmentController : ControllerBase
{
    private readonly MapAppDbContext _context;
    private readonly IRealtimeEventProcessor _eventProcessor;
    private readonly IJustInTimeService _jitService;
    private readonly ILogger<ShipmentController> _logger;

    public ShipmentController(
        MapAppDbContext context,
        IRealtimeEventProcessor eventProcessor,
        IJustInTimeService jitService,
        ILogger<ShipmentController> logger)
    {
        _context = context;
        _eventProcessor = eventProcessor;
        _jitService = jitService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new shipment
    /// </summary>
    [HttpPost("shipments")]
    public async Task<ActionResult<CreateShipmentResponse>> CreateShipment([FromBody] CreateShipmentRequest request)
    {
        try
        {
            var shipment = new Shipment
            {
                TrackingNumber = $"SCH-{DateTime.UtcNow:yyyyMMddHHmmss}",
                OriginState = request.OriginState,
                DestinationState = request.DestinationState,
                Weight = request.Weight,
                Volume = request.Volume,
                DeclaredValue = request.DeclaredValue,
                PickupScheduledTime = request.PickupScheduledTime,
                DeliveryScheduledTime = request.DeliveryScheduledTime,
                PlannedDistanceMiles = request.PlannedDistanceMiles,
                PlannedDurationMinutes = request.PlannedDurationMinutes,
                BaseRate = request.BaseRate,
                Status = ShipmentStatus.Pending
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created shipment {ShipmentId} from {Origin} to {Destination}",
                shipment.ShipmentId, request.OriginState, request.DestinationState);

            return Ok(new CreateShipmentResponse
            {
                ShipmentId = shipment.ShipmentId,
                TrackingNumber = shipment.TrackingNumber,
                Status = shipment.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment");
            return StatusCode(500, new { message = "Error creating shipment", error = ex.Message });
        }
    }

    /// <summary>
    /// Get shipment by ID
    /// </summary>
    [HttpGet("shipments/{shipmentId}")]
    public async Task<ActionResult<ShipmentDto>> GetShipment(string shipmentId)
    {
        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);
            if (shipment == null)
                return NotFound($"Shipment {shipmentId} not found");

            return Ok(MapToDto(shipment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment");
            return StatusCode(500, new { message = "Error retrieving shipment" });
        }
    }

    private ShipmentDto MapToDto(Shipment shipment)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Report real-time event (accident, weather, construction)
    /// </summary>
    [HttpPost("shipments/{shipmentId}/events")]
    public async Task<ActionResult<ShipmentEventResponse>> ReportEvent(
        string shipmentId,
        [FromBody] ReportEventRequest request)
    {
        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);
            if (shipment == null)
                return NotFound($"Shipment {shipmentId} not found");

            var @event = new ShipmentEvent
            {
                ShipmentId = shipmentId,
                EventType = request.EventType,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                LocationDescription = request.LocationDescription,
                Details = request.Details,
                DurationMinutes = request.DurationMinutes
            };

            // Process event (real-time)
            await _eventProcessor.ProcessEventAsync(@event, shipment);

            // Save event
            _context.ShipmentEvents.Add(@event);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Event {EventType} reported for shipment {ShipmentId}",
                request.EventType, shipmentId);

            return Ok(new ShipmentEventResponse
            {
                EventId = @event.EventId,
                ShipmentId = shipmentId,
                EventType = @event.EventType.ToString(),
                DelayMinutes = @event.DelayMinutes,
                CostImpact = @event.CostImpact
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting event");
            return StatusCode(500, new { message = "Error reporting event" });
        }
    }

    /// <summary>
    /// Get shipment timeline/events
    /// </summary>
    [HttpGet("shipments/{shipmentId}/events")]
    public async Task<ActionResult<List<ShipmentEventResponse>>> GetShipmentEvents(string shipmentId)
    {
        try
        {
            var events = await _context.ShipmentEvents
                .Where(e => e.ShipmentId == shipmentId)
                .OrderBy(e => e.EventTime)
                .ToListAsync();

            var result = events.Select(e => new ShipmentEventResponse
            {
                EventId = e.EventId,
                ShipmentId = e.ShipmentId,
                EventType = e.EventType.ToString(),
                EventTime = e.EventTime,
                LocationDescription = e.LocationDescription,
                Details = e.Details,
                DelayMinutes = e.DelayMinutes,
                CostImpact = e.CostImpact
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment events");
            return StatusCode(500, new { message = "Error retrieving shipment events" });
        }
    }

    /// <summary>
    /// Update shipment status (pickup, delivery, etc.)
    /// </summary>
    [HttpPut("shipments/{shipmentId}/status")]
    public async Task<IActionResult> UpdateShipmentStatus(
        string shipmentId,
        [FromBody] UpdateShipmentStatusRequest request)
    {
        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);
            if (shipment == null)
                return NotFound($"Shipment {shipmentId} not found");

            var previousStatus = shipment.Status;
            shipment.Status = request.Status;

            // Update timestamps
            if (request.Status == ShipmentStatus.PickedUp && !shipment.ActualPickupTime.HasValue)
                shipment.ActualPickupTime = DateTime.UtcNow;

            if (request.Status == ShipmentStatus.Delivered && !shipment.ActualDeliveryTime.HasValue)
                shipment.ActualDeliveryTime = DateTime.UtcNow;
                shipment.CompletedAt = DateTime.UtcNow;
                shipment.ELDRecorded = true; // Mark ELD as recorded on delivery

            shipment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated shipment {ShipmentId} status from {Previous} to {New}",
                shipmentId, previousStatus, request.Status);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipment status");
            return StatusCode(500, new { message = "Error updating shipment status" });
        }
    }

    /// <summary>
    /// Immediate JIT assignment for urgent shipment
    /// </summary>
    [HttpPost("shipments/{shipmentId}/jit-assign")]
    public async Task<ActionResult<JitAssignmentResponse>> JitAssignUrgent(
        string shipmentId,
        [FromBody] JitAssignmentRequest request)
    {
        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);
            if (shipment == null)
                return NotFound($"Shipment {shipmentId} not found");

            var result = await _jitService.AssignUrgentShipmentAsync(shipment, request.MinutesUntilDeadline);

            if (result.IsFeasible)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new JitAssignmentResponse
            {
                ShipmentId = result.ShipmentId,
                IsUrgent = result.IsUrgent,
                IsFeasible = result.IsFeasible,
                Reason = result.Reason,
                AssignedTruckId = result.AssignedTruckId,
                AssignedDriverId = result.AssignedDriverId,
                RequiredMPH = result.RequiredAverageMPH,
                UrgencyPremium = result.UrgencyPremium
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing JIT assignment");
            return StatusCode(500, new { message = "Error processing JIT assignment" });
        }
    }
}

/// <summary>
/// Billing and Code-to-Cash API
/// </summary>
[ApiController]
[Route("api/tms/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly IBatchProcessingService _batchService;
    private readonly IFuelMetricsService _fuelService;
    private readonly ILogger<BillingController> _logger;
    private readonly MapAppDbContext _context;

    public BillingController(
        IBillingService billingService,
        IBatchProcessingService batchService,
        IFuelMetricsService fuelService,
        ILogger<BillingController> logger,
        MapAppDbContext context)
    {
        _billingService = billingService;
        _batchService = batchService;
        _fuelService = fuelService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Generate billing record from completed shipment
    /// </summary>
    [HttpPost("records")]
    public async Task<ActionResult<BillingRecordResponse>> GenerateBillingRecord([FromBody] GenerateBillingRequest request)
    {
        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == request.ShipmentId);
            if (shipment == null)
                return NotFound("Shipment not found");

            if (shipment.Status != ShipmentStatus.Delivered)
                return BadRequest("Shipment must be delivered before billing");

            var record = await _billingService.GenerateBillingRecordAsync(shipment);
            await _context.SaveChangesAsync();

            return Ok(new BillingRecordResponse
            {
                BillingId = record.BillingId,
                InvoiceNumber = record.InvoiceNumber,
                CustomerName = record.CustomerName,
                TotalInvoiceAmount = record.TotalInvoiceAmount,
                Status = record.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing record");
            return StatusCode(500, new { message = "Error generating billing record" });
        }
    }

    /// <summary>
    /// Validate billing record for code-to-cash accuracy
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<BillingValidationResponse>> ValidateBilling([FromBody] ValidateBillingRequest request)
    {
        try
        {
            var record = await _context.BillingRecords.FirstOrDefaultAsync(b => b.BillingRecordId == request.BillingId);
            if (record == null)
                return NotFound("Billing record not found");

            var validation = await _billingService.ValidateBillingRecordAsync(record);

            return Ok(new BillingValidationResponse
            {
                 BillingId = validation.BillingId,
                InvoiceNumber = validation.InvoiceNumber,
                IsValid = validation.IsValid,
                ValidationMessages = validation.ValidationMessages,
                Issues = validation.Issues,
                Severity = validation.Severity,
                Recommendation = validation.Recommendation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating billing");
            return StatusCode(500, new { message = "Error validating billing" });
        }
    }

    /// <summary>
    /// Get code-to-cash metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<CodeToCashMetrics>> GetCodeToCashMetrics()
    {
        try
        {
            var shipments = await _context.Shipments.ToListAsync();
            var records = await _context.BillingRecords.ToListAsync();

            var metrics = new CodeToCashMetrics
            {
                TotalBilledRevenue = records.Sum(r => r.TotalInvoiceAmount),
                TotalCollected = records.Where(r => r.Status == BillingRecordStatus.Paid).Sum(r => r.TotalInvoiceAmount),
                OutstandingAmount = records.Where(r => r.Status != BillingRecordStatus.Paid && r.Status != BillingRecordStatus.WriteOff)
                    .Sum(r => r.TotalInvoiceAmount),
                DisputedAmount = records.Where(r => r.Status == BillingRecordStatus.Disputed).Sum(r => r.TotalInvoiceAmount),
                DSO = CalculateDaysOutstanding(records), // Days Sales Outstanding
                CollectionRate = records.Any() ? 
                    (records.Where(r => r.Status == BillingRecordStatus.Paid).Sum(r => r.TotalInvoiceAmount) / 
                     records.Sum(r => r.TotalInvoiceAmount)) * 100 : 0
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving code-to-cash metrics");
            return StatusCode(500, new { message = "Error retrieving metrics" });
        }
    }

    /// <summary>
    /// End-of-day batch processing
    /// </summary>
    [HttpPost("batch/end-of-day")]
    public async Task<ActionResult<BatchProcessResponse>> ProcessEndOfDay()
    {
        try
        {
            _logger.LogInformation("Initiating end-of-day batch processing");

            var result = await _batchService.ProcessEndOfDayAsync();

            return Ok(new BatchProcessResponse
            {
                ProcessDate = result.ProcessDate,
                Status = result.Status.ToString(),
                CompletedShipments = result.CompletedShipments,
                BillingRecordsGenerated = result.BillingRecords.Count,
                ValidationsPassed = result.ValidationsPassed,
                ValidationsFailed = result.ValidationsFailed,
                ComplianceIssuesFound = result.ComplianceIssues.Count,
                TotalExecutionSeconds = result.TotalExecutionSeconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing end-of-day batch");
            return StatusCode(500, new { message = "Error processing batch" });
        }
    }

    /// <summary>
    /// Validate code-to-cash for all recent shipments
    /// </summary>
    [HttpPost("validate/code-to-cash")]
    public async Task<ActionResult<CodeToCashValidationResponse>> ValidateCodeToCash()
    {
        try
        {
            var validations = await _batchService.ValidateCodeToCashAsync();

            var response = new CodeToCashValidationResponse
            {
                TotalValidations = validations.Count,
                PassedValidations = validations.Count(v => v.IsValid),
                FailedValidations = validations.Count(v => !v.IsValid),
                //ValidationDetails = validations
                //    .Where(v => !v.IsValid)
                //    .Select(v => new
                //    {
                //        v.ShipmentId,
                //        v.TrackingNumber,
                //        Issues = v.Issues
                //    })
                //    .ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating code-to-cash");
            return StatusCode(500, new { message = "Error validating code-to-cash" });
        }
    }

    private int CalculateDaysOutstanding(List<BillingRecord> records)
    {
        if (!records.Any(r => r.Status == BillingRecordStatus.Paid))
            return 0;

        var paidRecords = records.Where(r => r.Status == BillingRecordStatus.Paid).ToList();
        var daysOutstanding = paidRecords
            .Where(r => r.PaidDate.HasValue)
            .Average(r => (r.PaidDate.Value - r.BillingDate).TotalDays);

        return (int)daysOutstanding;
    }
}

#region Request/Response DTOs

public class CreateShipmentRequest
{
    public string OriginState { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public decimal DeclaredValue { get; set; }
    public DateTime PickupScheduledTime { get; set; }
    public DateTime DeliveryScheduledTime { get; set; }
    public double PlannedDistanceMiles { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public decimal BaseRate { get; set; }
}

public class CreateShipmentResponse
{
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ReportEventRequest
{
    public ShipmentEventType EventType { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
}

public class ShipmentEventResponse
{
    public string EventId { get; set; } = string.Empty;
    public string ShipmentId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public string LocationDescription { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public double? DelayMinutes { get; set; }
    public decimal? CostImpact { get; set; }
}

public class UpdateShipmentStatusRequest
{
    public ShipmentStatus Status { get; set; }
}

public class JitAssignmentRequest
{
    public int MinutesUntilDeadline { get; set; }
}

public class JitAssignmentResponse
{
    public string ShipmentId { get; set; } = string.Empty;
    public bool IsUrgent { get; set; }
    public bool IsFeasible { get; set; }
    public string? Reason { get; set; }
    public string? AssignedTruckId { get; set; }
    public string? AssignedDriverId { get; set; }
    public double RequiredMPH { get; set; }
    public decimal UrgencyPremium { get; set; }
}

public class BillingRecordResponse
{
    public string BillingId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalInvoiceAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CodeToCashMetrics
{
    public decimal TotalBilledRevenue { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal DisputedAmount { get; set; }
    public int DSO { get; set; } // Days Sales Outstanding
    public decimal CollectionRate { get; set; } // Percentage
}

public class BatchProcessResponse
{
    public DateTime ProcessDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CompletedShipments { get; set; }
    public int BillingRecordsGenerated { get; set; }
    public int ValidationsPassed { get; set; }
    public int ValidationsFailed { get; set; }
    public int ComplianceIssuesFound { get; set; }
    public double? TotalExecutionSeconds { get; set; }
}

public class CodeToCashValidationResponse
{
    public int TotalValidations { get; set; }
    public int PassedValidations { get; set; }
    public int FailedValidations { get; set; }
    public List<dynamic> ValidationDetails { get; set; } = new();
}

public class ShipmentDto
{
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string OriginState { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public double? ActualDistanceMiles { get; set; }
}

#endregion
