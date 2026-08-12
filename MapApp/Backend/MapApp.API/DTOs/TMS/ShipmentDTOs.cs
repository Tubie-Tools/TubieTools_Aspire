namespace MapApp.API.DTOs.TMS;

using MapApp.API.Models.TMS;

/// ============================================================================
/// REQUEST DTOs - Data sent TO the API
/// ============================================================================

/// <summary>
/// Create a new shipment
/// POST /api/tms/shipments
/// </summary>
public class CreateShipmentRequest
{
    public string OriginState { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;

    // Cargo Details
    public decimal Weight { get; set; }         // lbs
    public decimal Volume { get; set; }         // cubic feet
    public decimal DeclaredValue { get; set; }  // $ for liability

    // Scheduling
    public DateTime PickupScheduledTime { get; set; }
    public DateTime DeliveryScheduledTime { get; set; }

    // Route Planning
    public double PlannedDistanceMiles { get; set; }
    public int PlannedDurationMinutes { get; set; }

    // Pricing
    public decimal BaseRate { get; set; }
}

/// <summary>
/// Report a real-time event (accident, weather, construction)
/// POST /api/tms/shipments/{shipmentId}/events
/// </summary>
public class ReportEventRequest
{
    public ShipmentEventType EventType { get; set; }

    // Location
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;

    // Event Details
    public string Details { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
}

/// <summary>
/// Update shipment status during lifecycle
/// PUT /api/tms/shipments/{shipmentId}/status
/// </summary>
public class UpdateShipmentStatusRequest
{
    public ShipmentStatus NewStatus { get; set; }

    // Actual Performance (recorded during execution)
    public DateTime? ActualPickupTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }

    // Route Variance
    public double? ActualDistanceMiles { get; set; }
    public int? ActualDurationMinutes { get; set; }

    // Compliance
    public bool? ELDRecorded { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Urgent JIT assignment request
/// POST /api/tms/shipments/{shipmentId}/jit-assign
/// </summary>
public class JitAssignmentRequest
{
    public int MinutesUntilDeadline { get; set; }
    public bool AllowPremiumPricing { get; set; } = true;
    public string? PreferredTruckId { get; set; }  // Optional preferred truck
}

/// <summary>
/// Validate billing accuracy
/// POST /api/tms/billing/validate
/// </summary>
public class ValidateBillingRequest
{
    public string ShipmentId { get; set; } = string.Empty;
    public bool IncludeDetailedReport { get; set; } = false;
    public string BillingId { get; set; }
}

/// <summary>
/// Generate billing records for completed shipments
/// POST /api/tms/billing/batch/end-of-day
/// </summary>
public class GenerateBillingRequest
{
    public DateTime BillingDate { get; set; } = DateTime.UtcNow;
    public bool IncludeRevenueRecognition { get; set; } = true;
    public string ShipmentId { get; internal set; }
}

/// <summary>
/// Request code-to-cash validation for all shipments
/// POST /api/tms/billing/validate/c2c
/// </summary>
public class ValidateCodeToCashRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludePastIssues { get; set; } = false;
}

/// ============================================================================
/// RESPONSE DTOs - Data returned FROM the API
/// ============================================================================

/// <summary>
/// Response from shipment creation
/// </summary>
public class CreateShipmentResponse
{
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Full shipment details
/// </summary>
public class ShipmentDto
{
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Route
    public string OriginState { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;

    // Distance & Time
    public double PlannedDistanceMiles { get; set; }
    public double? ActualDistanceMiles { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }

    // Billing Summary
    public decimal BaseRate { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal TotalRevenue { get; set; }

    // Timestamps
    public DateTime PickupScheduledTime { get; set; }
    public DateTime? ActualPickupTime { get; set; }
    public DateTime DeliveryScheduledTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Event details response
/// </summary>
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

/// <summary>
/// Billing validation response
/// </summary>
public class BillingValidationResponse
{
    public string ShipmentId { get; set; } = string.Empty;
    public bool IsValid { get; set; }

    public List<string> ValidationMessages { get; set; } = new();
    public List<string> Issues { get; set; } = new();

    public string Severity { get; set; } = string.Empty; // Info, Warning, Error, Critical
    public string? Recommendation { get; set; }
    public object BillingId { get; internal set; }
    public string InvoiceNumber { get; internal set; }
}

/// <summary>
/// Generate billing records response
/// </summary>
public class GenerateBillingResponse
{
    public int RecordsGenerated { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal TotalLinehaul { get; set; }
    public decimal TotalFuelSurcharge { get; set; }

    public List<string> FailureDetails { get; set; } = new();
    public DateTime ProcessDate { get; set; }
}

/// <summary>
/// Code-to-cash validation results
/// </summary>
public class CodeToCashValidationResponse
{
    public int TotalShipments { get; set; }
    public int PassedValidations { get; set; }
    public int FailedValidations { get; set; }
    public double PassPercentage { get; set; }

    public List<BillingValidationResponse> DetailedResults { get; set; } = new();

    public decimal TotalRevenueValidated { get; set; }
    public string? OverallStatus { get; set; } // Success, Warning, Error
}

/// <summary>
/// JIT assignment response
/// </summary>
public class JitAssignmentResponse
{
    public string ShipmentId { get; set; } = string.Empty;
    public bool IsFeasible { get; set; }
    public string? Reason { get; set; }

    // If assigned
    public string? AssignedTruckId { get; set; }
    public string? AssignedDriverId { get; set; }

    public DateTime? EstimatedPickupTime { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }

    public decimal UrgencyPremium { get; set; }
    public decimal AdjustedBaseRate { get; set; }
}

/// <summary>
/// Batch processing end-of-day response
/// </summary>
public class BatchProcessingResponse
{
    public string Status { get; set; } = string.Empty;

    public int CompletedShipments { get; set; }
    public int BillingRecordsGenerated { get; set; }

    public decimal TotalRevenueRecognized { get; set; }
    public decimal GrossProfit { get; set; }
    public double ProfitMargin { get; set; }

    public int ValidationsPassed { get; set; }
    public int ValidationsFailed { get; set; }

    public int ComplianceViolations { get; set; }

    public double OnTimePercentage { get; set; }
    public double AverageMPG { get; set; }

    public double ExecutionSeconds { get; set; }

    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// Error response (standardized across all endpoints)
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Truck status response
/// </summary>
public class TruckStatusDto
{
    public string TruckId { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal FuelPercentage { get; set; }
    public int ActiveShipments { get; set; }
}

/// <summary>
/// Driver status response
/// </summary>
public class DriverStatusDto
{
    public string DriverId { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int HoursWorkedThisWeek { get; set; }
    public int HoursAvailableThisWeek { get; set; }
    public bool RequiresMandatoryBreak { get; set; }
}
