using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Service that bridges Dijkstra algorithm with geographic/mapping applications
    /// Handles coordinate conversion, location management, and route visualization
    /// </summary>
    public class MapDijkstraService
    {
        private readonly DijkstraAlgorithm _dijkstra;
        private readonly WeightedGraph _graph;
        private readonly Dictionary<int, (string Name, double Lat, double Lon)> _locationCache;

        public MapDijkstraService(WeightedGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _dijkstra = new DijkstraAlgorithm(graph);
            _locationCache = new Dictionary<int, (string, double, double)>();
        }

        /// <summary>
        /// Represents a location on the map
        /// </summary>
        public class MapLocation
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Type { get; set; }  // "warehouse", "store", "customer", etc.
            public Dictionary<string, object> Metadata { get; set; }

            public MapLocation()
            {
                Metadata = new Dictionary<string, object>();
            }

            public override string ToString() => $"{Name} ({Latitude:F4}, {Longitude:F4})";
        }

        /// <summary>
        /// Represents a route with geographic coordinates for mapping
        /// </summary>
        public class MapRoute
        {
            public string RouteId { get; set; }
            public List<MapLocation> Waypoints { get; set; }
            public List<(double Lat, double Lon)> Coordinates { get; set; }
            public double TotalDistance { get; set; }
            public double TotalCost { get; set; }
            public double EstimatedTimeMinutes { get; set; }
            public string Status { get; set; }  // "Active", "Completed", "Optimized"
            public Dictionary<string, object> Metadata { get; set; }

            public MapRoute()
            {
                Waypoints = new List<MapLocation>();
                Coordinates = new List<(double, double)>();
                Metadata = new Dictionary<string, object>();
                Status = "Optimized";
            }

            public override string ToString()
            {
                return $"{RouteId}: {string.Join(" → ", Waypoints.Select(w => w.Name))}, " +
                       $"Distance={TotalDistance:F2}km, Time={EstimatedTimeMinutes:F0}min";
            }
        }

        /// <summary>
        /// Adds a location to the map/network
        /// </summary>
        public void AddLocation(int id, string name, double latitude, double longitude, string locationType = "location")
        {
            _graph.AddVertex(id, name, (latitude, longitude));
            _locationCache[id] = (name, latitude, longitude);
        }

        /// <summary>
        /// Adds a road/connection between two locations
        /// </summary>
        public void AddRoad(int from, int to, double distanceKm, string roadName = null)
        {
            _graph.AddEdge(from, to, distanceKm, roadName);
        }

        /// <summary>
        /// Gets a location from the cache
        /// </summary>
        public MapLocation GetLocation(int locationId)
        {
            var vertex = _graph.GetVertex(locationId);
            var location = new MapLocation
            {
                Id = vertex.Id,
                Name = vertex.Label,
                Latitude = vertex.Coordinates?.Latitude ?? 0,
                Longitude = vertex.Coordinates?.Longitude ?? 0
            };
            return location;
        }

        /// <summary>
        /// Gets all locations as a list (useful for map markers)
        /// </summary>
        public List<MapLocation> GetAllLocations()
        {
            var locations = new List<MapLocation>();
            foreach (var vertex in _graph.GetVertices())
            {
                locations.Add(new MapLocation
                {
                    Id = vertex.Id,
                    Name = vertex.Label,
                    Latitude = vertex.Coordinates?.Latitude ?? 0,
                    Longitude = vertex.Coordinates?.Longitude ?? 0,
                    Metadata = vertex.Metadata
                });
            }
            return locations;
        }

        /// <summary>
        /// Searches for locations by name (case-insensitive)
        /// </summary>
        public List<MapLocation> SearchLocations(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<MapLocation>();

            var term = searchTerm.ToLower();
            return GetAllLocations()
                .Where(l => l.Name.ToLower().Contains(term))
                .ToList();
        }

        /// <summary>
        /// Calculates distance between two geographic coordinates (Haversine formula)
        /// </summary>
        public static double CalculateGPSDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Finds the optimal route from start to end location
        /// </summary>
        public MapRoute FindOptimalRoute(int startLocationId, int endLocationId, double costPerKm = 1.0)
        {
            var pathResult = _dijkstra.FindShortestPath(startLocationId, endLocationId);

            if (!pathResult.PathExists)
            {
                return new MapRoute
                {
                    Status = "No Route Available",
                    Metadata = new Dictionary<string, object> { { "Error", "No path found" } }
                };
            }

            // Build waypoints and coordinates from path
            var waypoints = new List<MapLocation>();
            var coordinates = new List<(double, double)>();

            foreach (var vertexId in pathResult.Path)
            {
                var location = GetLocation(vertexId);
                waypoints.Add(location);
                coordinates.Add((location.Latitude, location.Longitude));
            }

            var route = new MapRoute
            {
                RouteId = $"ROUTE-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                Waypoints = waypoints,
                Coordinates = coordinates,
                TotalDistance = pathResult.Distance,
                TotalCost = pathResult.Distance * costPerKm,
                EstimatedTimeMinutes = (pathResult.Distance / 60.0) * 60,  // Assuming 60 km/h avg
                Status = "Optimized",
            };

            route.Metadata["VerticesVisited"] = pathResult.VerticesVisited;
            route.Metadata["ComputationTimeMs"] = pathResult.ComputationTimeMs;
            route.Metadata["LocationCount"] = waypoints.Count;

            return route;
        }

        /// <summary>
        /// Finds optimal multi-stop route
        /// </summary>
        public MapRoute FindMultiStopRoute(int startLocationId, int[] stopLocationIds, double costPerKm = 1.0)
        {
            var logisticsService = new LogisticsDijkstraService(_graph);
            var route = logisticsService.FindOptimalMultiStopRoute(startLocationId, stopLocationIds, costPerKm);

            var mapRoute = new MapRoute
            {
                RouteId = route.RouteId,
                TotalDistance = route.TotalDistance,
                TotalCost = route.TotalCost,
                EstimatedTimeMinutes = route.EstimatedTimeMinutes,
                Status = "Optimized"
            };

            // Build waypoints and coordinates
            foreach (var vertexId in route.Stops)
            {
                var location = GetLocation(vertexId);
                mapRoute.Waypoints.Add(location);
                mapRoute.Coordinates.Add((location.Latitude, location.Longitude));
            }

            return mapRoute;
        }

        /// <summary>
        /// Gets all locations within a certain distance radius of a center point
        /// </summary>
        public List<MapLocation> GetLocationsWithinRadius(double centerLat, double centerLon, double radiusKm)
        {
            var locations = new List<MapLocation>();

            foreach (var location in GetAllLocations())
            {
                double distance = CalculateGPSDistance(centerLat, centerLon, location.Latitude, location.Longitude);
                if (distance <= radiusKm)
                {
                    location.Metadata["DistanceFromCenter"] = distance;
                    locations.Add(location);
                }
            }

            return locations.OrderBy(l => (double)l.Metadata["DistanceFromCenter"]).ToList();
        }

        /// <summary>
        /// Finds the nearest location to a coordinate
        /// </summary>
        public MapLocation FindNearestLocation(double latitude, double longitude)
        {
            var locations = GetAllLocations();
            if (locations.Count == 0)
                return null;

            return locations
                .Select(l => new { Location = l, Distance = CalculateGPSDistance(latitude, longitude, l.Latitude, l.Longitude) })
                .OrderBy(x => x.Distance)
                .First()
                .Location;
        }

        /// <summary>
        /// Converts a route to GeoJSON format for web mapping libraries
        /// </summary>
        public Dictionary<string, object> ExportAsGeoJSON(MapRoute route)
        {
            var features = new List<Dictionary<string, object>>();

            // Add waypoint features
            for (int i = 0; i < route.Waypoints.Count; i++)
            {
                var wp = route.Waypoints[i];
                features.Add(new Dictionary<string, object>
                {
                    { "type", "Feature" },
                    { "geometry", new {
                        type = "Point",
                        coordinates = new[] { wp.Longitude, wp.Latitude }
                    }},
                    { "properties", new {
                        name = wp.Name,
                        order = i + 1,
                        type = "waypoint"
                    }}
                });
            }

            // Add route line feature
            features.Add(new Dictionary<string, object>
            {
                { "type", "Feature" },
                { "geometry", new {
                    type = "LineString",
                    coordinates = route.Coordinates.Select(c => new[] { c.Lon, c.Lat }).ToArray()
                }},
                { "properties", new {
                    name = route.RouteId,
                    distance = route.TotalDistance,
                    time = route.EstimatedTimeMinutes,
                    cost = route.TotalCost,
                    type = "route"
                }}
            });

            return new Dictionary<string, object>
            {
                { "type", "FeatureCollection" },
                { "features", features }
            };
        }

        /// <summary>
        /// Exports route as polyline-encoded format (useful for Google Maps)
        /// </summary>
        public string ExportAsPolyline(MapRoute route)
        {
            // Simplified polyline encoding (production would need full polyline algorithm)
            return string.Join("|", route.Coordinates.Select(c => $"{c.Lat:F4},{c.Lon:F4}"));
        }

        /// <summary>
        /// Gets route summary for display
        /// </summary>
        public Dictionary<string, object> GetRouteSummary(MapRoute route)
        {
            return new Dictionary<string, object>
            {
                { "RouteId", route.RouteId },
                { "Status", route.Status },
                { "StartLocation", route.Waypoints.FirstOrDefault()?.Name },
                { "EndLocation", route.Waypoints.LastOrDefault()?.Name },
                { "TotalDistance", Math.Round(route.TotalDistance, 2) },
                { "TotalCost", Math.Round(route.TotalCost, 2) },
                { "EstimatedTimeMinutes", Math.Round(route.EstimatedTimeMinutes, 1) },
                { "WaypointCount", route.Waypoints.Count },
                { "Waypoints", route.Waypoints.Select(w => new { w.Id, w.Name, w.Latitude, w.Longitude }).ToList() }
            };
        }

        /// <summary>
        /// Exports complete route as JSON for API/persistence
        /// </summary>
        public Dictionary<string, object> ExportRouteAsJSON(MapRoute route)
        {
            return new Dictionary<string, object>
            {
                { "routeId", route.RouteId },
                { "status", route.Status },
                { "waypoints", route.Waypoints.Select(w => new {
                    id = w.Id,
                    name = w.Name,
                    latitude = w.Latitude,
                    longitude = w.Longitude,
                    type = w.Type
                }).ToList() },
                { "coordinates", route.Coordinates.Select(c => new { lat = c.Lat, lon = c.Lon }).ToList() },
                { "totalDistance", route.TotalDistance },
                { "totalCost", route.TotalCost },
                { "estimatedTimeMinutes", route.EstimatedTimeMinutes },
                { "metadata", route.Metadata }
            };
        }

        /// <summary>
        /// Clears the Dijkstra cache when graph changes
        /// </summary>
        public void RefreshCache()
        {
            _dijkstra.ClearCache();
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        public Dictionary<string, object> GetCacheStats()
        {
            return _dijkstra.GetCacheStatistics();
        }
    }
}
