namespace MapApp.API.Models;

/// <summary>
/// Represents a route segment between two locations
/// </summary>
public class RouteSegment
{
    public int Id { get; set; }
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public double Latitude1 { get; set; }
    public double Longitude1 { get; set; }
    public double Latitude2 { get; set; }
    public double Longitude2 { get; set; }
}

/// <summary>
/// Represents an optimized route visiting multiple state capitals
/// </summary>
public class OptimizedRoute
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> States { get; set; } = new();
    public double TotalDistanceKm { get; set; }
    public int TotalDurationMinutes { get; set; }
    public List<RouteSegment> RouteSegments { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Algorithm { get; set; } = "Nearest-Neighbor"; // Algorithm used for optimization
}

/// <summary>
/// Represents transportation logistics for visiting all state capitals
/// </summary>
public class TransportationPlan
{
    public int Id { get; set; }
    public string StartingState { get; set; } = string.Empty;
    public List<OptimizedRoute> Routes { get; set; } = new();
    public double TotalDistance { get; set; }
    public int TotalDurationHours { get; set; }
    public int EstimatedVehicles { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
