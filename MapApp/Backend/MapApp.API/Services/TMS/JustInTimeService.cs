using MapApp.API.Data;
using MapApp.API.Models.TMS;
using Microsoft.EntityFrameworkCore;

namespace MapApp.API.Services.TMS;

/// <summary>
/// Just-In-Time (JIT) processing for immediate route optimizations
/// Triggered when shipments are within specific time windows
/// </summary>
public interface IJustInTimeService
{
    /// <summary>
    /// Immediately assign truck and optimize route for urgent shipment
    /// </summary>
    Task<JITAssignmentResult> AssignUrgentShipmentAsync(Shipment shipment, int minutesUntilDeadline);

    /// <summary>
    /// Find best available truck within time window
    /// </summary>
    Task<Truck?> FindBestAvailableTruckAsync(string originState, string destState, int hoursAvailable);

    /// <summary>
    /// Consolidate shipments for more efficient routing
    /// </summary>
    Task<ConsolidationResult> ConsolidateShipmentsAsync(List<Shipment> shipments);

    /// <summary>
    /// Check real-time driver availability and HOS compliance
    /// </summary>
    Task<DriverAvailability> CheckDriverAvailabilityAsync(string driverId);

    /// <summary>
    /// Optimize pickup sequence for multiple stops
    /// </summary>
    Task<List<Shipment>> OptimizePickupSequenceAsync(List<Shipment> shipmentsAtFacility);
}

public class JustInTimeService : IJustInTimeService
{
    private readonly IRouteOptimizationService _routeService;
    private readonly IFuelMetricsService _fuelService;
    private readonly ILogger<JustInTimeService> _logger;
    private readonly MapAppDbContext _context;

    public JustInTimeService(
        IRouteOptimizationService routeService,
        IFuelMetricsService fuelService,
        ILogger<JustInTimeService> logger,
        MapAppDbContext context)
    {
        _routeService = routeService;
        _fuelService = fuelService;
        _logger = logger;
        _context = context;
    }

    public async Task<JITAssignmentResult> AssignUrgentShipmentAsync(Shipment shipment, int minutesUntilDeadline)
    {
        _logger.LogWarning("Urgent JIT assignment for shipment {ShipmentId}, deadline in {Minutes} minutes",
            shipment.ShipmentId, minutesUntilDeadline);

        var result = new JITAssignmentResult
        {
            ShipmentId = shipment.ShipmentId,
            AssignmentTime = DateTime.UtcNow,
            IsUrgent = minutesUntilDeadline < 120 // < 2 hours
        };

        // Calculate required speed
        var requiredMPH = shipment.PlannedDistanceMiles / (minutesUntilDeadline / 60.0);
        result.RequiredAverageMPH = requiredMPH;

        // Check feasibility
        if (requiredMPH > 75) // DOT speed limit consideration
        {
            result.IsFeasible = false;
            result.Reason = "Required speed exceeds safe/legal limits";
            _logger.LogError("Urgent shipment {ShipmentId} requires {MPH} MPH - not feasible", 
                shipment.ShipmentId, requiredMPH);
            return result;
        }

        // Find best truck
        var bestTruck = await FindBestAvailableTruckAsync(
            shipment.OriginState,
            shipment.DestinationState,
            minutesUntilDeadline / 60);

        if (bestTruck == null)
        {
            result.IsFeasible = false;
            result.Reason = "No available trucks within time window";
            _logger.LogWarning("No trucks available for urgent shipment {ShipmentId}", shipment.ShipmentId);
            return result;
        }

        // Check driver HOS
        var driverAvailability = await CheckDriverAvailabilityAsync(bestTruck.TruckId);
        if (!driverAvailability.IsAvailable)
        {
            result.IsFeasible = false;
            result.Reason = $"Driver not available: {driverAvailability.Reason}";
            return result;
        }

        // Calculate premium for urgent handling
        var urgencyPremium = (minutesUntilDeadline < 60) ? 0.25m : 0.15m; // 15-25% premium
        shipment.BaseRate = shipment.BaseRate * (1 + urgencyPremium);

        // Assign
        shipment.AssignedTruckId = bestTruck.TruckId;
        shipment.DriverId = driverAvailability.DriverId;
        shipment.Status = ShipmentStatus.Assigned;

        result.AssignedTruckId = bestTruck.TruckId;
        result.AssignedDriverId = driverAvailability.DriverId;
        result.IsFeasible = true;
        result.UrgencyPremium = urgencyPremium;

        _logger.LogInformation("Urgent shipment {ShipmentId} assigned to truck {TruckId}",
            shipment.ShipmentId, bestTruck.TruckId);

        return result;
    }

    public async Task<Truck?> FindBestAvailableTruckAsync(string originState, string destState, int hoursAvailable)
    {
        _logger.LogInformation("Finding best truck for {OriginState} -> {DestState}, {Hours}h available",
            originState, destState, hoursAvailable);

        // Simplified truck scoring - would be more complex in production
        var availableTrucks = new List<(Truck truck, decimal score)>();

        // Get trucks in or near origin state
        var trucksInRegion = await _context.Trucks
            .Where(t => t.CurrentState == originState && t.Status == TruckStatus.Available)
            .ToListAsync();

        double estimatedDistance = 500; // Placeholder - would calculate actual distance

        foreach (var truck in trucksInRegion)
        {
            var score = CalculateTruckScore(truck, hoursAvailable, estimatedDistance);
            availableTrucks.Add((truck, score));
        }

        // If no exact matches, search adjacent states
        if (!availableTrucks.Any())
        {
            _logger.LogInformation("No trucks in origin state, searching adjacent regions");
            // Would implement regional search logic
        }

        // Return highest-scoring truck
        return availableTrucks.OrderByDescending(x => x.score).FirstOrDefault().truck;
    }

    public async Task<ConsolidationResult> ConsolidateShipmentsAsync(List<Shipment> shipments)
    {
        _logger.LogInformation("Evaluating consolidation for {Count} shipments", shipments.Count);

        var result = new ConsolidationResult
        {
            OriginalShipments = shipments.Count,
            EvaluationTime = DateTime.UtcNow
        };

        if (shipments.Count < 2)
        {
            result.IsFeasible = false;
            result.Reason = "Minimum 2 shipments required for consolidation";
            return result;
        }

        // Group by geographic region
        var groupedByRegion = shipments.GroupBy(s => s.DestinationState).ToList();

        // Check weight/cube constraints
        var totalWeight = shipments.Sum(s => s.Weight);
        var totalVolume = shipments.Sum(s => s.Volume);

        const decimal maxWeight = 45000; // Truck limit
        const decimal maxVolume = 2700; // 53ft trailer

        if (totalWeight > maxWeight || totalVolume > maxVolume)
        {
            result.IsFeasible = false;
            result.Reason = $"Exceeds capacity: {totalWeight}lbs (max {maxWeight}), {totalVolume}cu ft (max {maxVolume})";
            return result;
        }

        // Calculate cost savings from consolidation
        var consolidatedDistance = CalculateConsolidatedDistance(shipments);
        var originalDistance = shipments.Sum(s => s.PlannedDistanceMiles);
        var distanceSaved = originalDistance - consolidatedDistance;

        result.ConsolidatedDistance = consolidatedDistance;
        result.DistanceSaved = distanceSaved;
        result.FuelSavings = CalculateFuelCost(distanceSaved);
        result.ConsolidatedShipments = groupedByRegion.Count;
        result.IsFeasible = true;

        _logger.LogInformation("Consolidation feasible: {Original}mi -> {Consolidated}mi, save {Saved}mi",
            originalDistance, consolidatedDistance, distanceSaved);

        return result;
    }

    public async Task<DriverAvailability> CheckDriverAvailabilityAsync(string driverId)
    {
        _logger.LogInformation("Checking driver availability: {DriverId}", driverId);

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

        var availability = new DriverAvailability
        {
            DriverId = driverId,
            CheckTime = DateTime.UtcNow,
            IsAvailable = true,
            Reason = string.Empty
        };

        if (driver == null)
        {
            availability.IsAvailable = false;
            availability.Reason = "Driver not found";
            return availability;
        }

        // Check status
        if (driver.Status != DriverStatus.Available)
        {
            availability.IsAvailable = false;
            availability.Reason = $"Driver status: {driver.Status}";
            return availability;
        }

        // Check HOS (Hours of Service)
        if (driver.HoursWorkedThisWeek >= driver.HoursAvailableThisWeek)
        {
            availability.IsAvailable = false;
            availability.Reason = $"HOS limit reached: {driver.HoursWorkedThisWeek}/{driver.HoursAvailableThisWeek}h";
            return availability;
        }

        // Check break requirement
        var hoursSinceLastBreak = (DateTime.UtcNow - driver.LastBreak).TotalHours;
        if (hoursSinceLastBreak > 11) // DOT requirement: break every 11 hours
        {
            availability.IsAvailable = false;
            availability.Reason = "Mandatory break required";
            return availability;
        }

        availability.HoursAvailable = Math.Min(
            14 - hoursSinceLastBreak, // Remaining time before mandatory break
            driver.HoursAvailableThisWeek - driver.HoursWorkedThisWeek); // Remaining weekly HOS

        return availability;
    }

    public async Task<List<Shipment>> OptimizePickupSequenceAsync(List<Shipment> shipmentsAtFacility)
    {
        _logger.LogInformation("Optimizing pickup sequence for {Count} shipments at facility", 
            shipmentsAtFacility.Count);

        // Sort by:
        // 1. Most time-sensitive first (earliest delivery deadline)
        // 2. Heaviest items first (easier to load bottom of truck)
        // 3. Destination proximity (for less backtracking)

        var optimizedSequence = shipmentsAtFacility
            .OrderBy(s => s.DeliveryScheduledTime) // Earliest deadline first
            .ThenByDescending(s => s.Weight) // Heaviest first
            .ThenBy(s => CalculateDestinationDistance(s.DestinationState))
            .ToList();

        _logger.LogInformation("Optimized pickup sequence: {Sequence}",
            string.Join(" -> ", optimizedSequence.Select(s => s.ShipmentId.Substring(0, 8))));

        return optimizedSequence;
    }

    private decimal CalculateTruckScore(Truck truck, int hoursAvailable, double requiredDistance)
    {
        decimal score = 100;

        // Fuel level bonus (full tanks score higher)
        score += truck.FuelPercentage * 10;

        // Availability bonus
        if (truck.ActiveShipmentsCount == 0)
            score += 50;
        else
            score -= truck.ActiveShipmentsCount * 10;

        // Distance feasibility (compare to HOS available)
        var requiredHours = requiredDistance / 60.0; // 60 MPH average
        if (requiredHours <= hoursAvailable)
            score += 50;
        else
            score -= 100; // Penalize if not feasible

        return score;
    }

    private double CalculateConsolidatedDistance(List<Shipment> shipments)
    {
        // Simplified calculation - would use actual TSP solver
        var totalDistance = shipments.Sum(s => s.PlannedDistanceMiles);
        return totalDistance * 0.85; // Assume 15% optimization from consolidation
    }

    private decimal CalculateFuelCost(double distanceMiles)
    {
        const double mpg = 6.5;
        const decimal fuelPrice = 3.50m;
        return (decimal)(distanceMiles / mpg) * fuelPrice;
    }

    private double CalculateDestinationDistance(string destState)
    {
        // Placeholder - would calculate actual distance from facility
        var distanceMap = new Dictionary<string, double>
        {
            ["CA"] = 50, ["NV"] = 100, ["AZ"] = 150
        };
        return distanceMap.ContainsKey(destState) ? distanceMap[destState] : 500;
    }
}

public class JITAssignmentResult
{
    public string ShipmentId { get; set; } = string.Empty;
    public DateTime AssignmentTime { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsFeasible { get; set; }
    public string? Reason { get; set; }
    public string? AssignedTruckId { get; set; }
    public string? AssignedDriverId { get; set; }
    public double RequiredAverageMPH { get; set; }
    public decimal UrgencyPremium { get; set; }
}

public class DriverAvailability
{
    public string DriverId { get; set; } = string.Empty;
    public DateTime CheckTime { get; set; }
    public bool IsAvailable { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double HoursAvailable { get; set; }
}

public class ConsolidationResult
{
    public DateTime EvaluationTime { get; set; }
    public int OriginalShipments { get; set; }
    public int ConsolidatedShipments { get; set; }
    public bool IsFeasible { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double OriginalDistance { get; set; }
    public double ConsolidatedDistance { get; set; }
    public double DistanceSaved { get; set; }
    public decimal FuelSavings { get; set; }
}
