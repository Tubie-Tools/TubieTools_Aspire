namespace MapApp.API.Models.TMS;

/// <summary>
/// Result of end-of-day batch processing
/// Contains aggregated metrics from all shipment and billing operations
/// </summary>
public class BatchProcessResult
{
    public DateTime ProcessDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? TotalExecutionSeconds { get; set; }

    // Shipment Completion
    public int CompletedShipments { get; set; }
    public int ShipmentsInException { get; set; }

    // Code-to-Cash Validation
    public List<BillingValidationResult> ValidationResults { get; set; } = new();
    public int ValidationsPassed { get; set; }
    public int ValidationsFailed { get; set; }

    // Billing Records Generated
    public List<ShipmentBillingRecord> BillingRecords { get; set; } = new();
    public decimal TotalRevenueRecognized { get; set; }

    // Performance Metrics
    public PerformanceMetrics? DailyMetrics { get; set; }

    // Compliance Audit
    public Task<List<ComplianceAuditEntry>> ComplianceIssues { get; set; }

    // Batch Status
    public BatchProcessStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    // Summary Statistics
    public decimal AverageRevenuePerShipment => 
        BillingRecords.Count > 0 ? TotalRevenueRecognized / BillingRecords.Count : 0;
}

/// <summary>
/// Status of batch processing execution
/// </summary>
public enum BatchProcessStatus
{
    Pending,         // Scheduled but not started
    Running,         // Currently executing
    Success,         // Completed successfully
    Failed,          // Encountered critical error
    PartialSuccess,  // Completed with warnings/errors in subset
    Cancelled        // Manually cancelled
}

/// <summary>
/// Result of validating a single shipment for code-to-cash accuracy
/// Used in batch processing to identify billing issues
/// </summary>
public class BillingValidationResult
{
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;

    // Validation Status
    public bool IsValid { get; set; }
    public List<string> Validations { get; set; } = new();
    public List<string> Issues { get; set; } = new();

    // Specific Checks Performed
    public bool DistanceVarianceCheck { get; set; } // <= 20% variance
    public bool ELDRecordCheck { get; set; }        // For >100 mile trips
    public bool BillingAmountCheck { get; set; }    // > $0
    public bool FuelSurchargeCheck { get; set; }    // <= 10% variance
    public bool DeliveryWindowCheck { get; set; }   // <= 3 hours late

    // Severity if issues found
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.None;

    // Recommendation
    public string? Recommendation { get; set; }
}

/// <summary>
/// Severity levels for validation issues
/// </summary>
public enum ValidationSeverity
{
    None,      // No issues
    Info,      // Informational only
    Warning,   // Should review but may be acceptable
    Error,     // Cannot proceed without fixing
    Critical   // Must escalate to management
}

/// <summary>
/// Daily performance metrics for KPI tracking and executive reporting
/// </summary>
public class PerformanceMetrics
{
    public DateTime Date { get; set; }

    // Volume
    public int TotalShipments { get; set; }
    public int DeliveredShipments { get; set; }
    public int ExceptionShipments { get; set; }

    // On-Time Performance (Target: >95%)
    public double OnTimePercentage { get; set; }
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public double AverageLateMiles { get; set; } // Avg how late

    // Fuel Economy (Target: 6.5 ±1.0 MPG)
    public double AverageMPG { get; set; }
    public double BestMPG { get; set; }
    public double WorstMPG { get; set; }

    // Cost Analysis
    public double AverageFuelCostPerMile { get; set; } // Target: $0.52-0.60
    public double AverageBillingPerMile { get; set; }

    // Financial Summary (Code-to-Cash)
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public double GrossProfitMargin { get; set; } // Target: >15%

    // Revenue Components
    public decimal TotalLinehaul { get; set; }
    public decimal TotalFuelSurcharge { get; set; }
    public decimal TotalAccessorials { get; set; }

    // Compliance
    public int ComplianceViolations { get; set; }
    public int HOSViolations { get; set; }
    public int ELDNonCompliant { get; set; }

    // Driver Performance
    public int UniqueDriversWorked { get; set; }
    public double AverageShipmentsPerDriver { get; set; }

    // Truck Utilization
    public int UniquesTrucksUsed { get; set; }
    public double AverageUtilization { get; set; } // % of available hours used

    // KPI Summary
    public bool MetOnTimeTarget => OnTimePercentage >= 0.95;
    public bool MetFuelEconomyTarget => AverageMPG >= 5.5 && AverageMPG <= 7.5;
    public bool MetProfitTarget => GrossProfitMargin >= 0.15;
}

/// <summary>
/// Single compliance audit entry for regulatory tracking
/// </summary>
public class ComplianceAuditEntry
{
    public string AuditId { get; set; } = Guid.NewGuid().ToString();
    public string ShipmentId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;

    // Compliance Category
    public ComplianceCategory Category { get; set; }
    public ComplianceStatus Status { get; set; }

    // Issue Details
    public string Issue { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }

    // Regulatory Significance
    public bool IsReportableToFMCSA { get; set; } // FMCSA = Federal Motor Carrier Safety Administration
    public bool RequiresCorrectiveAction { get; set; }
    public decimal PotentialFineAmount { get; set; }

    // Action Items
    public string? Recommendation { get; set; }
    public DateTime? TargetResolutionDate { get; set; }
    public string? ResolutionAction { get; set; }

    // Audit Metadata
    public DateTime AuditDate { get; set; } = DateTime.UtcNow;
    public string AuditedBy { get; set; } = string.Empty; // Employee ID or "System"
    public bool ManualReview { get; set; }
}

/// <summary>
/// Compliance categories tracked during audits
/// </summary>
public enum ComplianceCategory
{
    HoursOfService,      // DOT 49 CFR 395 - 70/11 hour limits
    ElectronicLogging,   // ELD mandatory for >100 miles
    VehicleInspection,   // Pre/post-trip inspections
    DriverQualification, // License, medical certification
    RecordKeeping,       // Documentation and retention
    WeightRestriction,   // Load weight compliance
    HazmatCompliance,    // Hazardous materials handling
    TaxCompliance,       // IRP/IFTA permits
    SafetyRating         // FMCSA safety metrics
}
