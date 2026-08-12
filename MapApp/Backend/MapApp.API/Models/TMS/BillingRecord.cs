namespace MapApp.API.Models.TMS;

/// <summary>
/// Represents a billing record for a completed shipment
/// Follows Schneider International code-to-cash revenue recognition (ASC 606)
/// </summary>
public class BillingRecord
{
    public string BillingRecordId { get; set; } = Guid.NewGuid().ToString();
    public string ShipmentId { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;

    // Billing Details
    public DateTime BillingDate { get; set; } = DateTime.UtcNow;
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerNumber { get; set; } = string.Empty;

    // Route Information
    public string OriginState { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;
    public double PlannedDistance { get; set; }
    public double ActualDistance { get; set; }

    // Financial Components (Code-to-Cash Breakdown)
    /// <summary>
    /// Linehaul: Distance-based revenue ($/mile × distance)
    /// Standard rate in trucking industry
    /// </summary>
    public decimal Linehaul { get; set; }

    /// <summary>
    /// Fuel Surcharge: FSI-indexed based on current fuel price
    /// Formula: Base surcharge × (1 + (FuelPrice - $2.50) × 0.06)
    /// Industry standard for fuel cost pass-through
    /// </summary>
    public decimal FuelSurcharge { get; set; }

    /// <summary>
    /// Accessorial Charges: Tolls, detention, hazmat, etc.
    /// Additional fees beyond linehaul
    /// </summary>
    public decimal AccessorialCharges { get; set; }

    /// <summary>
    /// Tax Amount: State-based sales tax on services
    /// Calculated based on origin/destination states
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Total Amount = Linehaul + FuelSurcharge + Accessories + Tax
    /// This is the gross invoice amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    // Billing Status Tracking
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Pending;
    public DateTime? InvoiceSentDate { get; set; }
    public DateTime? PaymentDueDate { get; set; }
    public DateTime? PaymentReceivedDate { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? AmountOutstanding { get; set; }

    // Revenue Recognition (ASC 606 Compliance)
    /// <summary>
    /// Revenue is recognized AFTER delivery is confirmed (not at pickup)
    /// This ensures compliance with ASC 606 revenue recognition standard
    /// </summary>
    public decimal RevenueRecognizedAmount { get; set; }

    /// <summary>
    /// The date revenue is recognized
    /// Usually equals ActualDeliveryTime from Shipment entity
    /// Ensures revenue matches performance obligation completion
    /// </summary>
    public DateTime? RevenueRecognitionDate { get; set; }

    // Cost of Service (for margin calculation)
    public decimal EstimatedCostOfService { get; set; }
    public decimal GrossProfit => TotalAmount - EstimatedCostOfService;
    public double GrossProfitMargin => (double)(GrossProfit / TotalAmount) * 100.0;

    // Compliance & Audit
    public bool IsAudited { get; set; }
    public DateTime? AuditDate { get; set; }
    public string? AuditNotes { get; set; }

    // HOS/ELD Compliance Flags
    public bool ELDRecorded { get; set; }
    public bool HOSCompliant { get; set; }
    public string? ComplianceNotes { get; set; }

    // Dispute Management
    public bool IsDisputed { get; set; }
    public DateTime? DisputeDate { get; set; }
    public string? DisputeReason { get; set; }
    public string? DisputeResolution { get; set; }
    public DateTime? DisputeResolvedDate { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Methods for code-to-cash calculations
    public bool IsFullyPaid => AmountPaid >= TotalAmount;
    public bool IsOverdue => (DateTime.UtcNow > PaymentDueDate) && !IsFullyPaid;
    public int DaysPastDue => IsOverdue ? (int)(DateTime.UtcNow - PaymentDueDate)?.TotalDays : 0;

    public decimal TotalInvoiceAmount { get; internal set; }
    public BillingRecordStatus Status { get; internal set; }
    public DateTime? PaidDate { get; internal set; }
}
