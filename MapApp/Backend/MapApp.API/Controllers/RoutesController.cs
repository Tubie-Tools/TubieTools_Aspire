using MapApp.API.Data;
using MapApp.API.DTOs;
using MapApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace MapApp.API.Controllers;

/// <summary>
/// Controller for managing route optimization and transportation planning
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly MapAppDbContext _context;
    private readonly IRouteOptimizationService _routeService;
    private readonly IOSRMService _osrmService;
    private readonly IMapper _mapper;
    private readonly ILogger<RoutesController> _logger;

    public RoutesController(
        MapAppDbContext context,
        IRouteOptimizationService routeService,
        IOSRMService osrmService,
        IMapper mapper,
        ILogger<RoutesController> logger)
    {
        _context = context;
        _routeService = routeService;
        _osrmService = osrmService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Generate optimized route visiting all state capitals using nearest neighbor algorithm
    /// </summary>
    [HttpPost("optimize")]
    public async Task<ActionResult<OptimizedRouteDto>> OptimizeRoute([FromBody] OptimizeRouteRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.StartingState))
                return BadRequest("Starting state must be specified");

            var capitals = await _context.StateCapitals.ToListAsync();
            if (!capitals.Any())
                return NotFound("No state capitals found");

            var optimizedRoute = _routeService.OptimizeRouteNearestNeighbor(capitals, request.StartingState);

            // Persist route to database
            _context.OptimizedRoutes.Add(optimizedRoute);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<OptimizedRouteDto>(optimizedRoute);
            dto.StateNames = optimizedRoute.States
                .Select(code => capitals.FirstOrDefault(c => c.StateCode == code)?.StateName ?? code)
                .ToList();

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing route");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generate transportation plan for multi-vehicle logistics
    /// Splits 50 states into optimal routes for multiple vehicles
    /// </summary>
    [HttpPost("transportation-plan")]
    public async Task<ActionResult<TransportationPlanDto>> CreateTransportationPlan([FromBody] TransportationPlanRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.StartingState))
                return BadRequest("Starting state must be specified");

            var capitals = await _context.StateCapitals.ToListAsync();
            if (!capitals.Any())
                return NotFound("No state capitals found");

            var plan = _routeService.CreateTransportationPlan(
                capitals,
                request.StartingState,
                request.VehicleCapacity);

            // Persist plan to database
            _context.TransportationPlans.Add(plan);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<TransportationPlanDto>(plan);

            // Enrich with state names
            foreach (var route in dto.Routes)
            {
                route.StateNames = route.States
                    .Select(code => capitals.FirstOrDefault(c => c.StateCode == code)?.StateName ?? code)
                    .ToList();
            }

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transportation plan");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get saved optimized route by ID
    /// </summary>
    [HttpGet("{routeId:int}")]
    public async Task<ActionResult<OptimizedRouteDto>> GetRoute(int routeId)
    {
        try
        {
            var route = await _context.OptimizedRoutes
                .Include(r => r.RouteSegments)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                return NotFound("Route not found");

            var dto = _mapper.Map<OptimizedRouteDto>(route);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route {RouteId}", routeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all saved routes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OptimizedRouteDto>>> GetAllRoutes()
    {
        try
        {
            var routes = await _context.OptimizedRoutes
                .Include(r => r.RouteSegments)
                .ToListAsync();

            var dtos = _mapper.Map<List<OptimizedRouteDto>>(routes);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get route segments for a specific route
    /// </summary>
    [HttpGet("{routeId:int}/segments")]
    public async Task<ActionResult<IEnumerable<RouteSegmentDto>>> GetRouteSegments(int routeId)
    {
        try
        {
            var route = await _context.OptimizedRoutes
                .Include(r => r.RouteSegments)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                return NotFound("Route not found");

            var capitals = await _context.StateCapitals.ToListAsync();
            var segmentDtos = route.RouteSegments.Select(s => 
            {
                var dto = _mapper.Map<RouteSegmentDto>(s);
                dto.FromCapital = capitals.FirstOrDefault(c => c.StateCode == s.FromState)?.CapitalName ?? s.FromState;
                dto.ToCapital = capitals.FirstOrDefault(c => c.StateCode == s.ToState)?.CapitalName ?? s.ToState;
                return dto;
            }).ToList();

            return Ok(segmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route segments for route {RouteId}", routeId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Calculate distance between two state capitals
    /// </summary>
    [HttpPost("distance")]
    public async Task<ActionResult<DistanceCalculationDto>> CalculateDistance([FromBody] DistanceRequestDto request)
    {
        try
        {
            var fromCapital = await _context.StateCapitals
                .FirstOrDefaultAsync(c => c.StateCode == request.FromState);
            var toCapital = await _context.StateCapitals
                .FirstOrDefaultAsync(c => c.StateCode == request.ToState);

            if (fromCapital == null || toCapital == null)
                return BadRequest("One or both states not found");

            var distance = _routeService.CalculateDistance(
                fromCapital.Latitude, fromCapital.Longitude,
                toCapital.Latitude, toCapital.Longitude);

            var durationMinutes = (int)(distance / 50 * 60); // Assume 50 km/h average

            return Ok(new DistanceCalculationDto
            {
                FromState = request.FromState,
                FromCapital = fromCapital.CapitalName,
                ToState = request.ToState,
                ToCapital = toCapital.CapitalName,
                DistanceKm = distance,
                DurationMinutes = durationMinutes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating distance");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class OptimizeRouteRequestDto
{
    public string StartingState { get; set; } = string.Empty;
}

public class TransportationPlanRequestDto
{
    public string StartingState { get; set; } = string.Empty;
    public int VehicleCapacity { get; set; } = 10; // Number of capitals per vehicle
}

public class DistanceRequestDto
{
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
}

public class DistanceCalculationDto
{
    public string FromState { get; set; } = string.Empty;
    public string FromCapital { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string ToCapital { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
}
