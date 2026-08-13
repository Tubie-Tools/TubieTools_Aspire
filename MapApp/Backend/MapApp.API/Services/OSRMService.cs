using MapApp.API.Models;
using System.Text.Json;

namespace MapApp.API.Services;

/// <summary>
/// Service for integrating with OSRM (Open Source Routing Machine)
/// For production, you would need to run OSRM locally or use their hosted service
/// </summary>
public interface IOSRMService
{
    /// <summary>
    /// Get route from OSRM between two coordinates
    /// </summary>
    Task<RouteResponse?> GetRouteAsync(double startLat, double startLon, double endLat, double endLon);

    /// <summary>
    /// Get distance matrix between multiple coordinates
    /// </summary>
    Task<DistanceMatrixResponse?> GetDistanceMatrixAsync(List<(double lat, double lon)> coordinates);
}

public class RouteResponse
{
    public List<Route>? Routes { get; set; }
}

public class Route
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public Geometry? Geometry { get; set; }
}

public class Geometry
{
    public string? Type { get; set; }
    public List<List<double>>? Coordinates { get; set; }
}

public class DistanceMatrixResponse
{
    public List<List<double>>? Distances { get; set; }
    public List<List<double>>? Durations { get; set; }
}

public class OSRMService : IOSRMService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OSRMService> _logger;
    private const string OsrmBaseUrl = "http://router.project-osrm.org"; // Free OSRM API - for development only

    public OSRMService(IHttpClientFactory httpClientFactory, ILogger<OSRMService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<RouteResponse?> GetRouteAsync(double startLat, double startLon, double endLat, double endLon)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{OsrmBaseUrl}/route/v1/driving/{startLon},{startLat};{endLon},{endLat}?overview=full";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"OSRM API returned {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var routeResponse = JsonSerializer.Deserialize<RouteResponse>(content);
            return routeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OSRM API");
            return null;
        }
    }

    public async Task<DistanceMatrixResponse?> GetDistanceMatrixAsync(List<(double lat, double lon)> coordinates)
    {
        try
        {
            if (coordinates.Count > 25)
            {
                _logger.LogWarning("OSRM distance matrix limited to 25 coordinates");
                coordinates = coordinates.Take(25).ToList();
            }

            // client should come from singleton IHttpClientFactory to avoid socket exhaustion

            var client = _httpClientFactory.CreateClient("osrmClient");
            var coordString = string.Join(";", coordinates.Select(c => $"{c.lon},{c.lat}"));
            var url = $"{OsrmBaseUrl}/table/v1/driving/{coordString}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"OSRM API returned {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var matrixResponse = JsonSerializer.Deserialize<DistanceMatrixResponse>(content);
            return matrixResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OSRM distance matrix API");
            return null;
        }
    }
}
