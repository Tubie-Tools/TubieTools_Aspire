using MapApp.API.Models;

namespace MapApp.API.Services;

/// <summary>
/// Service for optimizing routes using various algorithms
/// </summary>
public interface IRouteOptimizationService
{
    /// <summary>
    /// Calculate distance between two points using Haversine formula
    /// </summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Optimize route using nearest neighbor algorithm
    /// </summary>
    OptimizedRoute OptimizeRouteNearestNeighbor(List<StateCapital> capitals, string startState);

    /// <summary>
    /// Create a transportation plan for all state capitals
    /// </summary>
    TransportationPlan CreateTransportationPlan(List<StateCapital> allCapitals, string startingState, int vehicleCapacity = 10);
}

public class RouteOptimizationService : IRouteOptimizationService
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Calculates distance between two geographic points using Haversine formula
    /// </summary>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));
        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Optimizes route using nearest neighbor greedy algorithm
    /// Time Complexity: O(n²)
    /// This is suitable for the traveling salesman problem with moderate number of nodes
    /// </summary>
    public OptimizedRoute OptimizeRouteNearestNeighbor(List<StateCapital> capitals, string startState)
    {
        if (!capitals.Any())
            throw new ArgumentException("Capital list cannot be empty");

        // Start from specified state
        var start = capitals.FirstOrDefault(c => c.StateCode == startState) 
            ?? capitals.First();

        var visited = new HashSet<string> { start.StateCode };
        var route = new List<string> { start.StateCode };
        var currentCapital = start;
        var totalDistance = 0.0;
        var routeSegments = new List<RouteSegment>();

        // Nearest neighbor algorithm: always go to the closest unvisited capital
        while (visited.Count < capitals.Count)
        {
            var nearest = capitals
                .Where(c => !visited.Contains(c.StateCode))
                .OrderBy(c => CalculateDistance(
                    currentCapital.Latitude, currentCapital.Longitude,
                    c.Latitude, c.Longitude))
                .First();

            var distance = CalculateDistance(
                currentCapital.Latitude, currentCapital.Longitude,
                nearest.Latitude, nearest.Longitude);

            totalDistance += distance;
            visited.Add(nearest.StateCode);
            route.Add(nearest.StateCode);

            // Create route segment
            var segment = new RouteSegment
            {
                FromState = currentCapital.StateCode,
                ToState = nearest.StateCode,
                DistanceKm = distance,
                DurationMinutes = (int)(distance / 50 * 60), // Assume 50 km/h average speed
                Latitude1 = currentCapital.Latitude,
                Longitude1 = currentCapital.Longitude,
                Latitude2 = nearest.Latitude,
                Longitude2 = nearest.Longitude
            };

            routeSegments.Add(segment);
            currentCapital = nearest;
        }

        // Return to start
        var returnDistance = CalculateDistance(
            currentCapital.Latitude, currentCapital.Longitude,
            start.Latitude, start.Longitude);

        totalDistance += returnDistance;
        routeSegments.Add(new RouteSegment
        {
            FromState = currentCapital.StateCode,
            ToState = start.StateCode,
            DistanceKm = returnDistance,
            DurationMinutes = (int)(returnDistance / 50 * 60),
            Latitude1 = currentCapital.Latitude,
            Longitude1 = currentCapital.Longitude,
            Latitude2 = start.Latitude,
            Longitude2 = start.Longitude
        });

        return new OptimizedRoute
        {
            Name = $"Nearest Neighbor Route from {start.StateName}",
            States = route,
            TotalDistanceKm = totalDistance,
            TotalDurationMinutes = routeSegments.Sum(s => s.DurationMinutes),
            RouteSegments = routeSegments,
            Algorithm = "Nearest-Neighbor"
        };
    }

    /// <summary>
    /// Creates a transportation plan splitting all states into manageable routes
    /// Each route respects vehicle capacity constraints
    /// </summary>
    public TransportationPlan CreateTransportationPlan(
        List<StateCapital> allCapitals, 
        string startingState, 
        int vehicleCapacity = 10)
    {
        var plan = new TransportationPlan
        {
            StartingState = startingState,
            Routes = new List<OptimizedRoute>()
        };

        var start = allCapitals.FirstOrDefault(c => c.StateCode == startingState);
        if (start == null)
            throw new ArgumentException($"Starting state {startingState} not found");

        // Split capitals into routes based on vehicle capacity
        var currentRoute = new List<StateCapital> { start };
        var currentDistance = 0.0;
        var totalDistance = 0.0;

        var sorted = allCapitals
            .Where(c => c.StateCode != startingState)
            .OrderBy(c => CalculateDistance(
                start.Latitude, start.Longitude,
                c.Latitude, c.Longitude))
            .ToList();

        foreach (var capital in sorted)
        {
            if (currentRoute.Count >= vehicleCapacity)
            {
                // Finalize current route
                var optimizedRoute = OptimizeRouteNearestNeighbor(currentRoute, currentRoute[0].StateCode);
                plan.Routes.Add(optimizedRoute);
                totalDistance += optimizedRoute.TotalDistanceKm;
                currentDistance = 0;
                currentRoute = new List<StateCapital> { start };
            }

            currentRoute.Add(capital);
        }

        // Add remaining capitals as final route
        if (currentRoute.Count > 1)
        {
            var finalRoute = OptimizeRouteNearestNeighbor(currentRoute, currentRoute[0].StateCode);
            plan.Routes.Add(finalRoute);
            totalDistance += finalRoute.TotalDistanceKm;
        }

        plan.TotalDistance = totalDistance;
        plan.TotalDurationHours = (int)(totalDistance / 50); // 50 km/h average
        plan.EstimatedVehicles = plan.Routes.Count;

        return plan;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
