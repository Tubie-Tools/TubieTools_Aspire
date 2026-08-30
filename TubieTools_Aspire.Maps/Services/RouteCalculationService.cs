using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Maps.Services
{
    /// <summary>
    /// Blazor service that manages route planning operations
    /// Acts as bridge between Dijkstra algorithm and Blazor UI components
    /// </summary>
    public class RouteCalculationService
    {
        private WeightedGraph _graph;
        private MapDijkstraService _mapService;
        private List<MapDijkstraService.MapLocation> _allLocations;
        private MapDijkstraService.MapRoute _currentRoute;
        private List<MapDijkstraService.MapRoute> _routeHistory;

        public event Action OnRouteUpdated;
        public event Action OnLocationsUpdated;
        public event Action OnLoadingChanged;

        public bool IsLoading { get; private set; }
        public string ErrorMessage { get; private set; }
        public string SuccessMessage { get; private set; }

        public RouteCalculationService()
        {
            _allLocations = new List<MapDijkstraService.MapLocation>();
            _routeHistory = new List<MapDijkstraService.MapRoute>();
            _graph = new WeightedGraph(isDirected: false);
            _mapService = new MapDijkstraService(_graph);
        }

        /// <summary>
        /// Initializes the service with location and road data
        /// </summary>
        public async Task InitializeAsync(List<(int Id, string Name, double Lat, double Lon)> locations,
                                         List<(int From, int To, double DistanceKm, string RoadName)> roads)
        {
            try
            {
                SetLoading(true);
                ErrorMessage = null;

                // Add all locations
                foreach (var loc in locations)
                {
                    _mapService.AddLocation(loc.Id, loc.Name, loc.Lat, loc.Lon);
                }

                // Add all roads
                foreach (var road in roads)
                {
                    _mapService.AddRoad(road.From, road.To, road.DistanceKm, road.RoadName);
                }

                // Refresh location list
                _allLocations = _mapService.GetAllLocations();
                OnLocationsUpdated?.Invoke();

                SuccessMessage = $"Loaded {locations.Count} locations and {roads.Count} roads";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Initialization error: {ex.Message}";
            }
            finally
            {
                SetLoading(false);
            }
        }

        /// <summary>
        /// Calculates optimal route from start to end location
        /// </summary>
        public async Task<MapDijkstraService.MapRoute> CalculateRouteAsync(int startLocationId, int endLocationId, double costPerKm = 1.0)
        {
            try
            {
                SetLoading(true);
                ErrorMessage = null;

                // Validate inputs
                if (!_graph.ContainsVertex(startLocationId))
                    throw new ArgumentException($"Start location {startLocationId} not found");

                if (!_graph.ContainsVertex(endLocationId))
                    throw new ArgumentException($"End location {endLocationId} not found");

                // Calculate route
                _currentRoute = _mapService.FindOptimalRoute(startLocationId, endLocationId, costPerKm);

                if (_currentRoute.Status == "No Route Available")
                {
                    ErrorMessage = "No route available between these locations";
                    return _currentRoute;
                }

                // Add to history
                _routeHistory.Add(_currentRoute);

                SuccessMessage = $"Route calculated: {_currentRoute.TotalDistance:F2} km, " +
                               $"${_currentRoute.TotalCost:F2}, {_currentRoute.EstimatedTimeMinutes:F0} min";

                OnRouteUpdated?.Invoke();
                return _currentRoute;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Route calculation failed: {ex.Message}";
                return null;
            }
            finally
            {
                SetLoading(false);
            }
        }

        /// <summary>
        /// Calculates multi-stop route
        /// </summary>
        public async Task<MapDijkstraService.MapRoute> CalculateMultiStopRouteAsync(int startLocationId, int[] stopLocationIds, double costPerKm = 1.0)
        {
            try
            {
                SetLoading(true);
                ErrorMessage = null;

                _currentRoute = _mapService.FindMultiStopRoute(startLocationId, stopLocationIds, costPerKm);

                if (_currentRoute.Status == "No Route Available")
                {
                    ErrorMessage = "Could not optimize multi-stop route";
                    return _currentRoute;
                }

                _routeHistory.Add(_currentRoute);
                SuccessMessage = $"Multi-stop route optimized: {_currentRoute.TotalDistance:F2} km";

                OnRouteUpdated?.Invoke();
                return _currentRoute;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Multi-stop calculation failed: {ex.Message}";
                return null;
            }
            finally
            {
                SetLoading(false);
            }
        }

        /// <summary>
        /// Searches for locations by name
        /// </summary>
        public List<MapDijkstraService.MapLocation> SearchLocations(string searchTerm)
        {
            return _mapService.SearchLocations(searchTerm);
        }

        /// <summary>
        /// Gets all loaded locations
        /// </summary>
        public List<MapDijkstraService.MapLocation> GetAllLocations()
        {
            return _allLocations;
        }

        /// <summary>
        /// Gets a specific location by ID
        /// </summary>
        public MapDijkstraService.MapLocation GetLocation(int locationId)
        {
            return _mapService.GetLocation(locationId);
        }

        /// <summary>
        /// Gets current route
        /// </summary>
        public MapDijkstraService.MapRoute GetCurrentRoute()
        {
            return _currentRoute;
        }

        /// <summary>
        /// Gets route history
        /// </summary>
        public List<MapDijkstraService.MapRoute> GetRouteHistory()
        {
            return _routeHistory;
        }

        /// <summary>
        /// Gets locations within a radius
        /// </summary>
        public List<MapDijkstraService.MapLocation> GetLocationsWithinRadius(double centerLat, double centerLon, double radiusKm)
        {
            return _mapService.GetLocationsWithinRadius(centerLat, centerLon, radiusKm);
        }

        /// <summary>
        /// Finds nearest location to coordinates
        /// </summary>
        public MapDijkstraService.MapLocation FindNearestLocation(double latitude, double longitude)
        {
            return _mapService.FindNearestLocation(latitude, longitude);
        }

        /// <summary>
        /// Exports route as GeoJSON
        /// </summary>
        public Dictionary<string, object> ExportCurrentRouteAsGeoJSON()
        {
            if (_currentRoute == null)
                throw new InvalidOperationException("No current route to export");

            return _mapService.ExportAsGeoJSON(_currentRoute);
        }

        /// <summary>
        /// Exports route as JSON
        /// </summary>
        public Dictionary<string, object> ExportCurrentRouteAsJSON()
        {
            if (_currentRoute == null)
                throw new InvalidOperationException("No current route to export");

            return _mapService.ExportRouteAsJSON(_currentRoute);
        }

        /// <summary>
        /// Gets route summary
        /// </summary>
        public Dictionary<string, object> GetRouteSummary()
        {
            if (_currentRoute == null)
                throw new InvalidOperationException("No current route");

            return _mapService.GetRouteSummary(_currentRoute);
        }

        /// <summary>
        /// Clears current route
        /// </summary>
        public void ClearCurrentRoute()
        {
            _currentRoute = null;
            ErrorMessage = null;
            SuccessMessage = null;
            OnRouteUpdated?.Invoke();
        }

        /// <summary>
        /// Clears all history
        /// </summary>
        public void ClearHistory()
        {
            _routeHistory.Clear();
            _currentRoute = null;
            OnRouteUpdated?.Invoke();
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        public Dictionary<string, object> GetCacheStats()
        {
            return _mapService.GetCacheStats();
        }

        /// <summary>
        /// Refreshes cache (call after graph changes)
        /// </summary>
        public void RefreshCache()
        {
            _mapService.RefreshCache();
        }

        private void SetLoading(bool isLoading)
        {
            IsLoading = isLoading;
            OnLoadingChanged?.Invoke();
        }
    }
}
