using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TubieTools_Map.Services;

public class LogisticsOSRMClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LogisticsOSRMClient> _logger;

    public LogisticsOSRMClient(HttpClient httpClient, ILogger<LogisticsOSRMClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RoutingResponse?> CalculateRouteAsync(RoutingRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating route for {WaypointCount} waypoints", request.Waypoints.Count);

            var response = await _httpClient.PostAsJsonAsync("/api/routing/calculate", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RoutingResponse>();
            }

            _logger.LogWarning("Route calculation failed: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating route");
            return null;
        }
    }

    public async Task<CostResponse?> CalculateCostAsync(CostRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/cost/calculate", request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CostResponse>()
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cost");
            return null;
        }
    }
}

public class RoutingRequest
{
    [JsonPropertyName("waypoints")]
    public required List<Waypoint> Waypoints { get; set; }

    [JsonPropertyName("vehicleType")]
    public string VehicleType { get; set; } = "car";
}

public class Waypoint
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class RoutingResponse
{
    [JsonPropertyName("routeId")]
    public required string RouteId { get; set; }

    [JsonPropertyName("route")]
    public required List<Waypoint> Route { get; set; }

    [JsonPropertyName("distanceKm")]
    public required double DistanceKm { get; set; }

    [JsonPropertyName("durationSeconds")]
    public required int DurationSeconds { get; set; }

    [JsonPropertyName("segments")]
    public List<Segment>? Segments { get; set; }
}

public class Segment
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("distanceKm")]
    public double DistanceKm { get; set; }

    [JsonPropertyName("durationSeconds")]
    public int DurationSeconds { get; set; }
}

public class CostRequest
{
    [JsonPropertyName("routeId")]
    public required string RouteId { get; set; }

    [JsonPropertyName("vehicleType")]
    public required string VehicleType { get; set; }

    [JsonPropertyName("fuelPrice")]
    public double FuelPrice { get; set; }

    [JsonPropertyName("fuelEfficiency")]
    public double FuelEfficiency { get; set; }

    [JsonPropertyName("laborCost")]
    public double LaborCost { get; set; }
}

public class CostResponse
{
    [JsonPropertyName("routeId")]
    public required string RouteId { get; set; }

    [JsonPropertyName("fuelCost")]
    public decimal FuelCost { get; set; }

    [JsonPropertyName("laborCost")]
    public decimal LaborCost { get; set; }

    [JsonPropertyName("totalCost")]
    public decimal TotalCost { get; set; }
}