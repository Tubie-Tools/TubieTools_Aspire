namespace MapApp.API.Models.TMS;

/// <summary>
/// Result of Just-In-Time (JIT) urgent shipment assignment
/// Captures feasibility analysis and assignment details
/// </summary>
public class JITAssignmentResult
{
    public string ShipmentId { get; set; } = string.Empty;
    public DateTime AssignmentTime { get; set; }

    // Urgency Classification
    public bool IsUrgent { get; set; }           // < 2 hours = urgent
    public bool IsUltraUrgent { get; set; }      // < 1 hour = ultra-urgent

    // Feasibility Analysis
    public double RequiredAverageMPH { get; set; }
    public bool IsFeasible { get; set; }
    public string? Reason { get; set; }          // Why not feasible, if applicable

    // Truck & Driver Assignment
    public string? AssignedTruckId { get; set; }
    public string? AssignedDriverId { get; set; }
    public decimal? TruckDistance { get; set; }  // Distance to pickup from current location
    public int? EstimatedPickupMinutes { get; set; }

    // Premium Pricing for Urgency
    /// <summary>
    /// Urgency Premium:
    /// < 1 hour: 25% premium
    /// 1-2 hours: 15% premium
    /// Ensures profitability of high-touch JIT handling
    /// </summary>
    public decimal UrgencyPremium { get; set; }
    public decimal AdjustedBaseRate { get; set; }

    // ETA Calculation
    public DateTime EstimatedPickupTime { get; set; }
    public DateTime EstimatedDeliveryTime { get; set; }
    public bool CanMeetDeadline { get; set; }
}

/// <summary>
/// Real-time driver availability and HOS status
/// Used for feasibility checking in JIT assignment
/// </summary>
public class DriverAvailability
{
    public string DriverId { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;

    // Availability Status
    public bool IsAvailable { get; set; }
    public string Reason { get; set; } = string.Empty;

    // Hours of Service (DOT 49 CFR 395)
    public int HoursWorkedThisWeek { get; set; }  // Current week total
    public int HoursAvailableThisWeek { get; set; } = 70; // Weekly limit (70 hours)
    public int HoursAvailableToday { get; set; }  // Before mandatory break

    // Break Status
    public DateTime LastBreakTime { get; set; }
    public int MinutesSinceLastBreak { get; set; }
    public bool RequiresMandatoryBreak { get; set; } // After 11 hours driving
    public int MinutesUntilMandatoryBreak { get; set; }

    // Current Status
    public DriverStatus CurrentStatus { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;

    // Feasibility Summary
    public string FeasibilityMessage { get; set; } = string.Empty;
}

/// <summary>
/// Shipment consolidation analysis
/// Evaluates efficiency gains from combining multiple shipments
/// </summary>
public class ConsolidationResult
{
    public DateTime EvaluationTime { get; set; }

    // Input
    public int OriginalShipments { get; set; }
    public List<string> ShipmentIds { get; set; } = new();

    // Analysis
    public int ConsolidatedShipments { get; set; }
    public bool IsFeasible { get; set; }
    public string? Reason { get; set; }

    // Efficiency Gains
    public decimal CostSavings { get; set; }           // Absolute $ savings
    public double EfficiencyGain { get; set; }         // % improvement
    public double OriginalTotalMiles { get; set; }
    public double ConsolidatedTotalMiles { get; set; }
    public double MilesSaved { get; set; }

    // Financial Impact
    public decimal RevenueImpact { get; set; }
    public decimal NetBenefit { get; set; } // Savings - revenue loss

    // Pickup Sequence (if consolidated)
    public List<string> OptimizedSequence { get; set; } = new();
    public string? ConsolidatedTruckId { get; set; }
}

/// <summary>
/// Route optimization result
/// Returned when system evaluates rerouting due to real-time factors
/// </summary>
public class OptimizeRouteResult
{
    public string ShipmentId { get; set; } = string.Empty;

    // Routes
    public List<string> OriginalRoute { get; set; } = new();  // State sequence
    public List<string> OptimizedRoute { get; set; } = new(); // State sequence

    // Distance Impact
    public double OriginalDistance { get; set; }
    public double OptimizedDistance { get; set; }
    public double DistanceSavings { get; set; }
    public double DistanceSavingsPercentage { get; set; }

    // Time Impact
    public int OriginalTime { get; set; }  // Minutes
    public int OptimizedTime { get; set; } // Minutes
    public int TimeSavings { get; set; }   // Minutes

    // Cost Impact
    public decimal OriginalEstimatedCost { get; set; }
    public decimal OptimizedEstimatedCost { get; set; }
    public decimal CostSavings { get; set; }

    // Reason for Optimization
    public string OptimizationReason { get; set; } = string.Empty;

    // Calculation Metadata
    public DateTime CalculationTime { get; set; }
    public int OptimizationIterations { get; set; }
}

/// <summary>
/// Real-time update broadcast to clients
/// Used for WebSocket/SignalR push notifications
/// </summary>
public class ShipmentUpdate
{
    public string ShipmentId { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty; // "StatusChange", "DelayAlert", "Reroute", etc.
    public string Message { get; set; } = string.Empty;

    // Update Details
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Customer-Facing
    public string? CustomerNotificationText { get; set; }
    public bool RequiresCustomerAction { get; set; }

    // Timestamp and Severity
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public UpdateSeverity Severity { get; set; } = UpdateSeverity.Info;

    // Routing
    public List<string>? PushToRoles { get; set; } // "Dispatcher", "Driver", "Customer", "Management"
}

/// <summary>
/// Severity levels for shipment updates
/// Determines notification priority and escalation
/// </summary>
public enum UpdateSeverity
{
    Info,        // Informational only
    Warning,     // Take note, may affect delivery
    Alert,       // Immediate attention recommended
    Critical     // Emergency escalation required
}
