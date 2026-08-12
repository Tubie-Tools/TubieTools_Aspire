namespace MapApp.API.DTOs;

/// <summary>
/// DTO for state capital information returned to clients
/// </summary>
public class StateCapitalDto
{
    public string StateCode { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string CapitalName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool HasSoldProducts { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int ProductsSold { get; set; }
    public string Region { get; set; } = string.Empty;
    public string PinColor { get; set; } = string.Empty; // For map visualization
}

/// <summary>
/// DTO for route segment information
/// </summary>
public class RouteSegmentDto
{
    public string FromState { get; set; } = string.Empty;
    public string FromCapital { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string ToCapital { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public double Latitude1 { get; set; }
    public double Longitude1 { get; set; }
    public double Latitude2 { get; set; }
    public double Longitude2 { get; set; }
}

/// <summary>
/// DTO for optimized route information
/// </summary>
public class OptimizedRouteDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> States { get; set; } = new();
    public List<string> StateNames { get; set; } = new();
    public double TotalDistanceKm { get; set; }
    public int TotalDurationMinutes { get; set; }
    public List<RouteSegmentDto> RouteSegments { get; set; } = new();
    public string Algorithm { get; set; } = string.Empty;
}

/// <summary>
/// DTO for transportation plan
/// </summary>
public class TransportationPlanDto
{
    public string StartingState { get; set; } = string.Empty;
    public List<OptimizedRouteDto> Routes { get; set; } = new();
    public double TotalDistance { get; set; }
    public int TotalDurationHours { get; set; }
    public int EstimatedVehicles { get; set; }
    public DateTime CreatedAt { get; set; }
}
