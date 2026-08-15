//using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TubieTools_Map.Data;
using TubieTools_Map.Data.Models;

namespace TubieTools_Map.Services;

public class RouteService
{
    private readonly MapAppDbContext _context;
    private readonly LogisticsOSRMClient _logisticsClient;
    //private readonly IMapper _mapper;
    private readonly ILogger<RouteService> _logger;

    public RouteService(MapAppDbContext context, LogisticsOSRMClient logisticsClient,
        //IMapper mapper, 
        ILogger<RouteService> logger)
    {
        _context = context;
        _logisticsClient = logisticsClient;
        //_mapper = mapper;
        _logger = logger;
    }

    public async Task<MapRoute?> CreateRouteAsync(string routeName, List<Waypoint> waypoints,
        string vehicleType, string userId)
    {
        try
        {
            var routingRequest = new RoutingRequest
            {
                Waypoints = waypoints,
                VehicleType = vehicleType
            };

            var routingResponse = await _logisticsClient.CalculateRouteAsync(routingRequest);
            if (routingResponse == null)
            {
                _logger.LogWarning("Failed to calculate route from OSRM");
                return null;
            }

            var route = new MapRoute
            {
                RouteId = routingResponse.RouteId,
                RouteName = routeName,
                CreatedBy = userId,
                DistanceKm = routingResponse.DistanceKm,
                EstimatedDuration = TimeSpan.FromSeconds(routingResponse.DurationSeconds),
                VehicleType = vehicleType,
                Waypoints = JsonSerializer.Serialize(waypoints),
                Segments = routingResponse.Segments?.Select(s => new RouteSegment
                {
                    SegmentIndex = s.Index,
                    DistanceKm = s.DistanceKm,
                    Duration = TimeSpan.FromSeconds(s.DurationSeconds)
                }).ToList() ?? new List<RouteSegment>()
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Route {RouteId} created successfully", route.RouteId);
            return route;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating route");
            return null;
        }
    }

    public async Task<List<MapRoute>> GetRoutesByUserAsync(string userId)
    {
        return await _context.Routes
            .Where(r => r.CreatedBy == userId)
            .Include(r => r.Segments)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<MapRoute>> GetAllRoutesAsync()
    {
        return await _context.Routes
            .Include(r => r.Segments)
            .Include(r => r.Redirections)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<MapRoute?> GetRouteByIdAsync(string routeId)
    {
        return await _context.Routes
            .Include(r => r.Segments)
            .Include(r => r.Redirections)
            .FirstOrDefaultAsync(r => r.RouteId == routeId);
    }

    public async Task<RouteRedirection?> CreateRedirectionAsync(string originalRouteId,
        string reason, string description, List<Waypoint>? alternativeWaypoints)
    {
        try
        {
            var originalRoute = await GetRouteByIdAsync(originalRouteId);
            if (originalRoute == null)
                return null;

            MapRoute? alternativeRoute = null;
            if (alternativeWaypoints?.Count > 0)
            {
                alternativeRoute = await CreateRouteAsync(
                    $"Redirect of {originalRoute.RouteName}",
                    alternativeWaypoints,
                    originalRoute.VehicleType ?? "car",
                    originalRoute.CreatedBy);
            }

            var redirection = new RouteRedirection
            {
                OriginalRouteId = originalRouteId,
                Reason = reason,
                Description = description,
                AlternativeRouteId = alternativeRoute?.RouteId,
                IsActive = true
            };

            _context.RouteRedirections.Add(redirection);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Redirection {RedirId} created for route {RouteId}",
                redirection.RedirectionId, originalRouteId);
            return redirection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating redirection");
            return null;
        }
    }

    public async Task<bool> DeleteRouteAsync(string routeId)
    {
        try
        {
            var route = await GetRouteByIdAsync(routeId);
            if (route == null)
                return false;

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting route");
            return false;
        }
    }

    public async Task<bool> UpdateRouteStatusAsync(string routeId, string status)
    {
        try
        {
            var route = await GetRouteByIdAsync(routeId);
            if (route == null)
                return false;

            route.Status = status;
            route.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route status");
            return false;
        }
    }
}