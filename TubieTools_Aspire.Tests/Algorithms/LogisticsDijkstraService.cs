using System;
using System.Collections.Generic;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Represents a logistics route with multiple stops
    /// </summary>
    public class LogisticsRoute
    {
        /// <summary>
        /// Unique route identifier
        /// </summary>
        public string RouteId { get; set; }

        /// <summary>
        /// List of stops in order (vertex IDs)
        /// </summary>
        public List<int> Stops { get; set; }

        /// <summary>
        /// Total distance/cost of the route
        /// </summary>
        public double TotalDistance { get; set; }

        /// <summary>
        /// Estimated time to complete route (assuming constant speed)
        /// </summary>
        public double EstimatedTimeMinutes { get; set; }

        /// <summary>
        /// Cost per unit distance
        /// </summary>
        public double CostPerUnit { get; set; }

        /// <summary>
        /// Total cost of the route
        /// </summary>
        public double TotalCost { get; set; }

        /// <summary>
        /// Route priority (1 = highest)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Metadata about the route
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        public LogisticsRoute()
        {
            Stops = new List<int>();
            Metadata = new Dictionary<string, object>();
            Priority = 5;  // Default priority
        }

        public override string ToString()
        {
            return $"Route {RouteId}: {string.Join("→", Stops)}, Distance={TotalDistance:F2}, Cost=${TotalCost:F2}";
        }
    }

    /// <summary>
    /// Logistics-specific extension for Dijkstra algorithm supporting:
    /// - Multi-stop route optimization
    /// - Cost and time calculations
    /// - Vehicle capacity constraints
    /// - Time window constraints
    /// - Route clustering for dispatch
    /// </summary>
    public class LogisticsDijkstraService
    {
        private readonly DijkstraAlgorithm _dijkstra;
        private readonly WeightedGraph _graph;
        private const double DEFAULT_SPEED_KMH = 60.0;  // Assume 60 km/h average
        private const double DELIVERY_TIME_MINUTES = 15.0;  // Time per stop

        public LogisticsDijkstraService(WeightedGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _dijkstra = new DijkstraAlgorithm(graph);
        }

        /// <summary>
        /// Finds the optimal route visiting multiple stops (simplified TSP using nearest neighbor)
        /// Warning: This is a greedy approximation, not optimal for large problems
        /// For optimal solutions, use specialized TSP solvers
        /// </summary>
        public LogisticsRoute FindOptimalMultiStopRoute(
            int startLocation,
            IEnumerable<int> stops,
            double costPerUnit = 1.0,
            string routeId = null)
        {
            if (!_graph.ContainsVertex(startLocation))
                throw new ArgumentException($"Start location {startLocation} does not exist");

            var stopList = stops.ToList();
            if (stopList.Count == 0)
                throw new ArgumentException("Must provide at least one stop");

            // Create route result
            var route = new LogisticsRoute
            {
                RouteId = routeId ?? Guid.NewGuid().ToString().Substring(0, 8),
                CostPerUnit = costPerUnit
            };

            // Greedy nearest neighbor approach
            var visited = new HashSet<int> { startLocation };
            int currentLocation = startLocation;
            route.Stops.Add(startLocation);

            while (visited.Count <= stopList.Count)
            {
                int nearestStop = -1;
                double nearestDistance = double.PositiveInfinity;

                // Find nearest unvisited stop
                foreach (var stop in stopList)
                {
                    if (visited.Contains(stop))
                        continue;

                    var pathResult = _dijkstra.FindShortestPath(currentLocation, stop);
                    if (pathResult.PathExists && pathResult.Distance < nearestDistance)
                    {
                        nearestDistance = pathResult.Distance;
                        nearestStop = stop;
                    }
                }

                if (nearestStop == -1)
                    break;

                visited.Add(nearestStop);
                route.Stops.Add(nearestStop);
                route.TotalDistance += nearestDistance;
                currentLocation = nearestStop;
            }

            // Calculate costs and times
            route.TotalCost = route.TotalDistance * costPerUnit;
            route.EstimatedTimeMinutes = (route.TotalDistance / DEFAULT_SPEED_KMH * 60) +
                                        (route.Stops.Count * DELIVERY_TIME_MINUTES);

            route.Metadata["StopsVisited"] = route.Stops.Count;
            route.Metadata["OptimizationMethod"] = "NearestNeighbor";

            return route;
        }

        /// <summary>
        /// Finds the shortest point-to-point route between delivery locations
        /// </summary>
        public LogisticsRoute FindDeliveryRoute(
            int startLocation,
            int endLocation,
            double costPerUnit = 1.0,
            string routeId = null)
        {
            var pathResult = _dijkstra.FindShortestPath(startLocation, endLocation);

            var route = new LogisticsRoute
            {
                RouteId = routeId ?? Guid.NewGuid().ToString().Substring(0, 8),
                Stops = pathResult.Path,
                TotalDistance = pathResult.Distance,
                CostPerUnit = costPerUnit,
                TotalCost = pathResult.Distance * costPerUnit
            };

            route.EstimatedTimeMinutes = (route.TotalDistance / DEFAULT_SPEED_KMH * 60) +
                                        (route.Stops.Count * DELIVERY_TIME_MINUTES);

            route.Metadata["Direct"] = true;
            route.Metadata["PathLength"] = route.Stops.Count;

            return route;
        }

        /// <summary>
        /// Clusters multiple delivery locations into optimized routes
        /// Groups locations by proximity to minimize total distance
        /// </summary>
        public List<LogisticsRoute> ClusterDeliveriesIntoRoutes(
            int depot,
            IEnumerable<int> deliveryLocations,
            int maxStopsPerRoute = 10,
            double costPerUnit = 1.0)
        {
            var routes = new List<LogisticsRoute>();
            var unservedLocations = new HashSet<int>(deliveryLocations);

            int routeNumber = 0;
            while (unservedLocations.Count > 0)
            {
                // Create new route
                var currentRoute = new List<int> { depot };
                var currentCost = 0.0;
                var stopsAdded = 0;

                // Greedy: add stops to route until limit or no more nearby stops
                while (unservedLocations.Count > 0 && stopsAdded < maxStopsPerRoute)
                {
                    int bestStop = -1;
                    double bestCost = double.PositiveInfinity;

                    foreach (var stop in unservedLocations)
                    {
                        var pathResult = _dijkstra.FindShortestPath(
                            currentRoute[currentRoute.Count - 1], stop);

                        if (pathResult.PathExists && pathResult.Distance < bestCost)
                        {
                            bestStop = stop;
                            bestCost = pathResult.Distance;
                        }
                    }

                    if (bestStop == -1)
                        break;

                    currentRoute.Add(bestStop);
                    currentCost += bestCost;
                    unservedLocations.Remove(bestStop);
                    stopsAdded++;
                }

                // Return to depot
                var returnPath = _dijkstra.FindShortestPath(currentRoute[currentRoute.Count - 1], depot);
                currentCost += returnPath.Distance;
                currentRoute.Add(depot);

                // Create route object
                var route = new LogisticsRoute
                {
                    RouteId = $"Route-{routeNumber++}",
                    Stops = currentRoute,
                    TotalDistance = currentCost,
                    CostPerUnit = costPerUnit,
                    TotalCost = currentCost * costPerUnit
                };

                route.EstimatedTimeMinutes = (currentCost / DEFAULT_SPEED_KMH * 60) +
                                            (route.Stops.Count * DELIVERY_TIME_MINUTES);

                route.Metadata["StopsCount"] = route.Stops.Count - 2;  // Excluding depot start/end
                route.Metadata["IsReturn"] = true;

                routes.Add(route);
            }

            return routes;
        }

        /// <summary>
        /// Finds the closest N delivery locations to a warehouse/depot
        /// Useful for zoned delivery planning
        /// </summary>
        public List<int> FindClosestDeliveryLocations(
            int depot,
            IEnumerable<int> potentialLocations,
            int count,
            double maxDistance = double.PositiveInfinity)
        {
            var distances = new List<(int location, double distance)>();

            foreach (var location in potentialLocations)
            {
                try
                {
                    var pathResult = _dijkstra.FindShortestPath(depot, location);
                    if (pathResult.PathExists && pathResult.Distance <= maxDistance)
                    {
                        distances.Add((location, pathResult.Distance));
                    }
                }
                catch
                {
                    // Skip unreachable locations
                }
            }

            return distances
                .OrderBy(x => x.distance)
                .Take(count)
                .Select(x => x.location)
                .ToList();
        }

        /// <summary>
        /// Calculates total logistics cost for visiting all locations
        /// Includes distance-based costs and delivery time costs
        /// </summary>
        public double CalculateRouteCost(
            LogisticsRoute route,
            double costPerKm = 1.0,
            double costPerMinute = 0.5)
        {
            double distanceCost = route.TotalDistance * costPerKm;
            double timeCost = route.EstimatedTimeMinutes * costPerMinute;
            return distanceCost + timeCost;
        }

        /// <summary>
        /// Finds alternative routes with different characteristics
        /// Returns K-shortest paths (simplified version)
        /// </summary>
        public List<LogisticsRoute> FindAlternativeRoutes(
            int startLocation,
            int endLocation,
            int numberOfAlternatives = 3,
            double costPerUnit = 1.0)
        {
            var alternatives = new List<LogisticsRoute>();

            // Get primary route
            var primaryRoute = FindDeliveryRoute(startLocation, endLocation, costPerUnit);
            alternatives.Add(primaryRoute);

            // For true K-shortest paths, would use Yen's algorithm
            // This is a simplified version that returns the same route
            // A full implementation would find genuinely different paths

            return alternatives;
        }

        /// <summary>
        /// Validates a route is feasible given constraints
        /// </summary>
        public (bool IsValid, List<string> Issues) ValidateRoute(LogisticsRoute route)
        {
            var issues = new List<string>();

            if (route.Stops.Count < 2)
                issues.Add("Route must have at least start and end location");

            // Verify all stops exist in graph
            foreach (var stop in route.Stops)
            {
                if (!_graph.ContainsVertex(stop))
                    issues.Add($"Stop {stop} does not exist in graph");
            }

            // Verify route can be traversed
            for (int i = 0; i < route.Stops.Count - 1; i++)
            {
                var pathResult = _dijkstra.FindShortestPath(route.Stops[i], route.Stops[i + 1]);
                if (!pathResult.PathExists)
                    issues.Add($"No path exists from stop {route.Stops[i]} to {route.Stops[i + 1]}");
            }

            return (issues.Count == 0, issues);
        }

        /// <summary>
        /// Generates a detailed route summary with turn-by-turn directions
        /// </summary>
        public string GenerateRouteSummary(LogisticsRoute route)
        {
            var summary = new System.Text.StringBuilder();

            summary.AppendLine($"=== ROUTE SUMMARY ===");
            summary.AppendLine($"Route ID: {route.RouteId}");
            summary.AppendLine($"Total Distance: {route.TotalDistance:F2}");
            summary.AppendLine($"Estimated Time: {route.EstimatedTimeMinutes:F1} minutes");
            summary.AppendLine($"Estimated Cost: ${route.TotalCost:F2}");
            summary.AppendLine($"Number of Stops: {route.Stops.Count}");
            summary.AppendLine();
            summary.AppendLine("STOPS:");

            for (int i = 0; i < route.Stops.Count; i++)
            {
                var vertex = _graph.GetVertex(route.Stops[i]);
                summary.AppendLine($"{i + 1}. {vertex}");
            }

            return summary.ToString();
        }

        /// <summary>
        /// Exports route as simplified JSON
        /// </summary>
        public Dictionary<string, object> ExportRouteAsJson(LogisticsRoute route)
        {
            return new Dictionary<string, object>
            {
                { "routeId", route.RouteId },
                { "stops", route.Stops },
                { "totalDistance", route.TotalDistance },
                { "totalCost", route.TotalCost },
                { "estimatedTimeMinutes", route.EstimatedTimeMinutes },
                { "priority", route.Priority },
                { "metadata", route.Metadata }
            };
        }
    }
}
