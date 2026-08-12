namespace MapApp.API.Models.TMS;

/// <summary>
/// Represents an active shipment with real-time tracking
/// Following Schneider International TMS standards
/// </summary>
public class Shipment
{
    public string ShipmentId { get; set; } = Guid.NewGuid().ToString();
    public string TrackingNumber { get; set; } = string.Empty;

    // Route information
    public string OriginState { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;
    public string AssignedTruckId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;

    // Shipment details
    public decimal Weight { get; set; } // lbs
    public decimal Volume { get; set; } // cubic feet
    public decimal DeclaredValue { get; set; } // $ for billing
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    // Timestamps (HOS - Hours of Service compliance)
    public DateTime PickupScheduledTime { get; set; }
    public DateTime DeliveryScheduledTime { get; set; }
    public DateTime? ActualPickupTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }

    // Route optimization
    public List<string> PlannedRouteStates { get; set; } = new();
    public double PlannedDistanceMiles { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public double? ActualDistanceMiles { get; set; }
    public int? ActualDurationMinutes { get; set; }

    // Billing (Code-to-Cash)
    public decimal BaseRate { get; set; }
    public decimal FuelSurcharge { get; set; }
    public decimal AdditionalCharges { get; set; } // Tolls, accessorials
    public decimal TotalRevenue { get; set; }
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Pending;

    // Compliance
    public bool ELDRecorded { get; set; } // Electronic Logging Device
    public int HoursOfServiceUsed { get; set; }
    public int HoursOfServiceAvailable { get; set; } = 70; // Weekly limit

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum ShipmentStatus
{
    Pending,
    Assigned,
    PickedUp,
    InTransit,
    Delivered,
    Cancelled,
    Exception
}

public enum BillingStatus
{
    Pending,
    ReadyForBilling,
    Invoiced,
    Paid,
    Disputed
}

/// <summary>
/// Real-time event tracking for shipments
/// </summary>
public class ShipmentEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string ShipmentId { get; set; } = string.Empty;
    public ShipmentEventType EventType { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    // Location data
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;

    // Event details
    public string Details { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }

    // Impact metrics
    public double? DelayMinutes { get; set; }
    public decimal? CostImpact { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ShipmentEventType
{
    Pickup,
    Departure,
    Arrival,
    Delivery,
    Accident,
    WeatherDelay,
    ConstructionDelay,
    FuelStop,
    MechanicalIssue,
    HosViolation,
    TrafficDelay,
    RouteChange,
    Exception
}

/// <summary>
/// Truck/Vehicle information with real-time status
/// </summary>
public class Truck
{
    public string TruckId { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty; // Schneider unit ID
    public string VIN { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;

    // Location tracking
    public double CurrentLatitude { get; set; }
    public double CurrentLongitude { get; set; }
    public DateTime LastPositionUpdate { get; set; } = DateTime.UtcNow;

    // Operational metrics
    public decimal FuelPercentage { get; set; }
    public decimal MilesSinceLastFuel { get; set; }
    public double AverageMPG { get; set; } = 6.5; // Industry standard
    public decimal FuelCost { get; set; } // Per gallon, real-time

    // Status
    public TruckStatus Status { get; set; } = TruckStatus.Available;
    public int ActiveShipmentsCount { get; set; }
    public double TotalCurrentMiles { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TruckStatus
{
    Available,
    InTransit,
    AtFacility,
    NeedsFuel,
    Maintenance,
    OutOfService
}

/// <summary>
/// Driver information with HOS tracking
/// </summary>
public class Driver
{
    public string DriverId { get; set; } = string.Empty;
    public string DriverNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;

    // Hours of Service (DOT compliance)
    public int HoursWorkedThisWeek { get; set; }
    public int HoursAvailableThisWeek { get; set; } = 70; // Federal limit
    public DateTime HOSWeekStartDate { get; set; }

    // Driving status
    public DriverStatus Status { get; set; } = DriverStatus.Available;
    public DateTime LastBreak { get; set; }
    public int BreaksRequired { get; set; } // Regulatory requirement

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum DriverStatus
{
    Available,
    OnDuty,
    Sleeping,
    OffDuty,
    OnBreak,
    Suspended
}

/// <summary>
/// Real-time factor affecting routes
/// </summary>
public class RouteFactor
{
    public string FactorId { get; set; } = Guid.NewGuid().ToString();
    public RouteFactorType FactorType { get; set; }
    public DateTime ReportTime { get; set; } = DateTime.UtcNow;

    // Location
    public string AffectedState { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ImpactRadiusMiles { get; set; }

    // Impact details
    public int EstimatedDelayMinutes { get; set; }
    public decimal ImpactedShipmentCount { get; set; }
    public decimal EstimatedCostImpact { get; set; }

    // Status
    public FactorStatus Status { get; set; } = FactorStatus.Active;
    public DateTime? ResolvedTime { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

//public enum RouteFactorType
//{
//    WeatherEvent,
//    Accident,
//    Construction,
//    RoadClosure,
//    TrafficJam,
//    Hazmat
//}

//public enum FactorStatus
//{
//    Active,
//    Ongoing,
//    Clearing,
//    Resolved,
//    Archived
//}

///// <summary>
///// Gas price and fuel economy tracking
///// </summary>
//public class FuelMetrics
//{
//    public string MetricId { get; set; } = Guid.NewGuid().ToString();
//    public string TruckId { get; set; } = string.Empty;

//    // Current metrics
//    public decimal PricePerGallon { get; set; }
//    public double MilesPerGallon { get; set; }
//    public decimal CostPerMile { get; set; }

//    // Historical tracking
//    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
//    public double TotalMilesMeasured { get; set; }
//    public double TotalGallonsUsed { get; set; }

//    // Regional variations
//    public string State { get; set; } = string.Empty;
//    public decimal StateAveragePrice { get; set; }

//    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
//}

/// <summary>
/// Billing record for code-to-cash process
/// </summary>
public class ShipmentBillingRecord
{
    public string BillingId { get; set; } = Guid.NewGuid().ToString();
    public string ShipmentId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;

    // Billing details
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;

    // Revenue breakdown
    public decimal BaseLineHaul { get; set; } // Distance-based
    public decimal FuelSurcharge { get; set; }
    public decimal AccessorialCharges { get; set; } // Tolls, detention, etc.
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalInvoiceAmount { get; set; }

    // Timeline
    public DateTime ShipmentDate { get; set; }
    public DateTime BillingDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    // Status
    public BillingRecordStatus Status { get; set; } = BillingRecordStatus.Draft;
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum BillingRecordStatus
{
    Draft,
    Submitted,
    Approved,
    Invoiced,
    Paid,
    WriteOff,
    Disputed
}
