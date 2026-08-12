namespace MapApp.API.Models.TMS;

/// <summary>
/// Fuel Metrics: Tracks real-time fuel costs, efficiency, and surcharge indexing
/// Critical for code-to-cash accuracy and margin management
/// </summary>
public class FuelMetrics
{
    public string MetricsId { get; set; } = Guid.NewGuid().ToString();
    public string TruckId { get; set; } = string.Empty;

    // Time Period for Metrics
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Actual Performance (from telematics/ELD)
    public double TotalMiles { get; set; }
    public double TotalGallonsUsed { get; set; }
    public double ActualMPG { get; set; }
    public double TotalFuelCost { get; set; }
    public double CostPerMile { get; set; }

    // Fuel Price Information
    public decimal FuelPricePerGallon { get; set; } // Average for period
    public decimal MinFuelPrice { get; set; }
    public decimal MaxFuelPrice { get; set; }

    // Fuel Surcharge Index (FSI)
    /// <summary>
    /// FSI Calculation: 6% surcharge increase per $0.01 above $2.50 base
    /// Base price: $2.50/gallon
    /// Example: $3.50 = $1.00 variance × 0.06 = 6% surcharge
    /// </summary>
    public decimal FuelSurchargeIndex { get; set; }
    public decimal FuelSurchargePercentage { get; set; }

    // Benchmarking (Industry Standard: 6.5 MPG)
    public const double BenchmarkMPG = 6.5;
    public double EfficiencyVariance { get; set; } // % above/below benchmark
    public bool IsBelowBenchmark { get; set; }

    // Cost Analysis
    public double ExpectedFuelCost { get; set; } // At benchmark MPG
    public double FuelCostVariance { get; set; } // Actual vs expected

    // Regional Tracking
    public string PrimaryRegion { get; set; } = string.Empty;
    public string? SecondaryRegion { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Supporting DTO for fuel efficiency analysis and reporting
/// </summary>
public class FuelEfficiencyMetrics
{
    public string TruckId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Performance
    public double ActualMPG { get; set; }
    public double TotalMiles { get; set; }
    public double TotalGallonsUsed { get; set; }
    public double TotalFuelCost { get; set; }
    public double CostPerMile { get; set; }

    // Benchmarking
    public const double IndustryBenchmark = 6.5;
    public double EfficiencyVariance { get; set; } // % variance from benchmark
    public bool IsBelowBenchmark { get; set; }

    // Performance Notes
    public string PerformanceStatus => 
        IsBelowBenchmark ? "Below Benchmark - Review Driver Training" : "Meeting Benchmark";
}

/// <summary>
/// Fuel cost calculation details for a specific shipment
/// Used during billing to calculate fuel surcharge component
/// </summary>
public class FuelCostCalculation
{
    public string ShipmentId { get; set; } = string.Empty;
    public double Distance { get; set; }
    public decimal FuelPricePerGallon { get; set; }

    // Industry standard: 6.5 MPG for class 8 trucks
    public const double StandardMPG = 6.5;

    public double GallonsRequired => Distance / StandardMPG;
    public decimal TotalFuelCost { get; set; }
    public decimal CostPerMile { get; set; }

    /// <summary>
    /// Fuel Surcharge Calculation
    /// Base: $2.50/gallon
    /// Surcharge: 6% per $0.01 variance from base
    /// </summary>
    public decimal BaseFuelPrice => 2.50m;
    public decimal PriceVariance => FuelPricePerGallon - BaseFuelPrice;
    public decimal SurchargePercentage => PriceVariance * 0.06m;
    public decimal FuelSurchargeAmount => (decimal)Distance * 0.15m * (1 + SurchargePercentage);
}

/// <summary>
/// Route factor types affecting fuel costs and routing decisions
/// </summary>
public enum RouteFactorType
{
    Weather,           // Rain, snow, ice affecting fuel economy
    TrafficAccident,   // Highway closures
    Construction,      // Road work, lane reductions
    Incident,          // General roadway incidents
    RoadClosure,       // Complete road closure
    FuelPriceVolatility, // Regional fuel price changes
    HazmatRestriction, // Hazmat routing requirements
    WeightRestriction  // Bridge/road weight limits
}

/// <summary>
/// Status tracking for route factors
/// </summary>
public enum FactorStatus
{
    Active,      // Currently impacting routes
    Resolved,    // Condition cleared
    Cancelled,   // False alarm or prediction
    Monitoring   // Being monitored but not yet impacting routes
}

/// <summary>
/// Compliance status for audit and regulatory purposes
/// </summary>
public enum ComplianceStatus
{
    Compliant,    // Meets all DOT requirements
    NonCompliant, // Violation(s) detected
    UnderReview,  // Audit in progress
    Waived        // Compliant under special authorization
}
