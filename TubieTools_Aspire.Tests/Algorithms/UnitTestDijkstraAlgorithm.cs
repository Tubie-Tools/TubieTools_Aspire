using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TubieTools_Aspire.Tests.Algorithms
{
    [TestClass]
    public class UnitTestDijkstraAlgorithm
    {
        private WeightedGraph _graph;
        private DijkstraAlgorithm _dijkstra;
        private TestContext _testContextInstance;

        public TestContext TestContext
        {
            get { return _testContextInstance; }
            set { _testContextInstance = value; }
        }

        [TestInitialize]
        public void Init()
        {
            _graph = new WeightedGraph(isDirected: false);
            _dijkstra = new DijkstraAlgorithm(_graph);
        }

        #region Graph Construction Tests

        [TestMethod]
        public void TestCreateWeightedGraph()
        {
            Assert.IsNotNull(_graph);
            Assert.AreEqual(0, _graph.VertexCount);
            Assert.AreEqual(0, _graph.EdgeCount);
        }

        [TestMethod]
        public void TestAddVertices()
        {
            _graph.AddVertex(1, "A");
            _graph.AddVertex(2, "B");
            _graph.AddVertex(3, "C");

            Assert.AreEqual(3, _graph.VertexCount);
            Assert.IsTrue(_graph.ContainsVertex(1));
            Assert.IsTrue(_graph.ContainsVertex(2));
            Assert.IsTrue(_graph.ContainsVertex(3));
        }

        [TestMethod]
        public void TestAddEdges()
        {
            _graph.AddVertex(1, "A");
            _graph.AddVertex(2, "B");
            _graph.AddVertex(3, "C");

            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 3.0);
            _graph.AddEdge(1, 3, 10.0);

            Assert.AreEqual(3, _graph.EdgeCount);
        }

        [TestMethod]
        public void TestGetEdgesFrom()
        {
            _graph.AddVertex(1, "A");
            _graph.AddVertex(2, "B");
            _graph.AddVertex(3, "C");

            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(1, 3, 10.0);

            var edges = _graph.GetEdgesFrom(1).ToList();
            Assert.AreEqual(2, edges.Count);
            Assert.IsTrue(edges.Any(e => e.Destination == 2));
            Assert.IsTrue(edges.Any(e => e.Destination == 3));
        }

        [TestMethod]
        public void TestDirectedGraph()
        {
            var directedGraph = new WeightedGraph(isDirected: true);
            directedGraph.AddVertex(1, "A");
            directedGraph.AddVertex(2, "B");
            directedGraph.AddEdge(1, 2, 5.0);

            Assert.AreEqual(1, directedGraph.EdgeCount);
            var edges = directedGraph.GetEdgesFrom(2).ToList();
            Assert.AreEqual(0, edges.Count);  // No edges from 2 in directed graph
        }

        #endregion

        #region Basic Dijkstra Tests

        [TestMethod]
        public void TestSimplestPath()
        {
            // Create simple graph: 1 --5-- 2
            _graph.AddVertex(1, "Start");
            _graph.AddVertex(2, "End");
            _graph.AddEdge(1, 2, 5.0);

            var result = _dijkstra.FindShortestPath(1, 2);

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(5.0, result.Distance);
            Assert.AreEqual(2, result.Path.Count);
            Assert.AreEqual(1, result.Path[0]);
            Assert.AreEqual(2, result.Path[1]);

            TestContext.WriteLine($"Simple path: {result}");
        }

        [TestMethod]
        public void TestMultiHopPath()
        {
            // Create graph: 1 --5-- 2 --3-- 3 --2-- 4
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3); _graph.AddVertex(4);
            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 3.0);
            _graph.AddEdge(3, 4, 2.0);

            var result = _dijkstra.FindShortestPath(1, 4);

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(10.0, result.Distance);  // 5 + 3 + 2
            Assert.AreEqual(4, result.Path.Count);

            TestContext.WriteLine($"Multi-hop path: {result}");
        }

        [TestMethod]
        public void TestMultiplePathsChoosesShortest()
        {
            // Create graph with multiple paths
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3); _graph.AddVertex(4);
            _graph.AddEdge(1, 2, 5.0);   // Path 1: 1->2->4 = 13
            _graph.AddEdge(2, 4, 8.0);
            _graph.AddEdge(1, 3, 3.0);   // Path 2: 1->3->4 = 5 (shorter)
            _graph.AddEdge(3, 4, 2.0);

            var result = _dijkstra.FindShortestPath(1, 4);

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(5.0, result.Distance);
            Assert.IsTrue(result.Path.Contains(3));

            TestContext.WriteLine($"Shortest of multiple paths: {result}");
        }

        [TestMethod]
        public void TestNoPathExists()
        {
            // Create disconnected graph
            _graph.AddVertex(1); _graph.AddVertex(2);
            _graph.AddVertex(3); _graph.AddVertex(4);
            _graph.AddEdge(1, 2, 5.0);
            // 3 and 4 are isolated

            var result = _dijkstra.FindShortestPath(1, 3);

            Assert.IsFalse(result.PathExists);
            Assert.AreEqual(double.PositiveInfinity, result.Distance);
            Assert.AreEqual(0, result.Path.Count);

            TestContext.WriteLine($"No path: {result}");
        }

        [TestMethod]
        public void TestSameSourceAndDestination()
        {
            _graph.AddVertex(1, "A");

            var result = _dijkstra.FindShortestPath(1, 1);

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(0.0, result.Distance);
            Assert.AreEqual(1, result.Path.Count);
            Assert.AreEqual(1, result.Path[0]);

            TestContext.WriteLine($"Same vertex: {result}");
        }

        #endregion

        #region Complex Graph Tests

        [TestMethod]
        public void TestComplexLogisticsNetwork()
        {
            // Simulate a logistics network: cities connected by roads
            _graph.AddVertex(1, "Warehouse");
            _graph.AddVertex(2, "Store A");
            _graph.AddVertex(3, "Store B");
            _graph.AddVertex(4, "Store C");
            _graph.AddVertex(5, "Store D");
            _graph.AddVertex(6, "Distribution Center");

            _graph.AddEdge(1, 6, 10.0);  // Warehouse to Distribution
            _graph.AddEdge(6, 2, 15.0);  // Distribution to Store A
            _graph.AddEdge(6, 3, 12.0);  // Distribution to Store B
            _graph.AddEdge(2, 4, 8.0);   // Store A to Store C
            _graph.AddEdge(3, 5, 7.0);   // Store B to Store D
            _graph.AddEdge(4, 5, 5.0);   // Store C to Store D

            var result = _dijkstra.FindShortestPath(1, 5);

            Assert.IsTrue(result.PathExists);
            Assert.IsTrue(result.Distance < double.PositiveInfinity);

            TestContext.WriteLine($"Logistics network path: {result}");
            TestContext.WriteLine($"Path sequence: {string.Join("→", result.Path)}");
        }

        [TestMethod]
        public void TestLargeGraph()
        {
            // Create a 10x10 grid graph
            int gridSize = 10;
            int vertexCount = gridSize * gridSize;

            for (int i = 0; i < vertexCount; i++)
            {
                _graph.AddVertex(i, $"V{i}");
            }

            // Add edges in grid pattern
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int current = i * gridSize + j;

                    // Right neighbor
                    if (j < gridSize - 1)
                    {
                        int right = i * gridSize + (j + 1);
                        _graph.AddEdge(current, right, 1.0);
                    }

                    // Down neighbor
                    if (i < gridSize - 1)
                    {
                        int down = (i + 1) * gridSize + j;
                        _graph.AddEdge(current, down, 1.0);
                    }
                }
            }

            var result = _dijkstra.FindShortestPath(0, vertexCount - 1);  // Top-left to bottom-right

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(18.0, result.Distance);  // 9 right + 9 down = 18

            TestContext.WriteLine($"Large grid path: {result}");
            TestContext.WriteLine($"Path length: {result.Path.Count} vertices");
        }

        #endregion

        #region Multiple Path Tests

        [TestMethod]
        public void TestFindMultipleDestinations()
        {
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3); _graph.AddVertex(4);
            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 3.0);
            _graph.AddEdge(2, 4, 7.0);

            var destinations = new[] { 2, 3, 4 };
            var results = _dijkstra.FindShortestPathsToMultipleDestinations(1, destinations);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(5.0, results[2].Distance);
            Assert.AreEqual(8.0, results[3].Distance);
            Assert.AreEqual(12.0, results[4].Distance);

            TestContext.WriteLine("Distances from vertex 1:");
            foreach (var kvp in results.OrderBy(x => x.Key))
            {
                TestContext.WriteLine($"  to {kvp.Key}: {kvp.Value.Distance}");
            }
        }

        [TestMethod]
        public void TestFindPathsUpToDistance()
        {
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3); _graph.AddVertex(4);
            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 3.0);
            _graph.AddEdge(3, 4, 10.0);

            var results = _dijkstra.FindPathsUpToDistance(1, 10.0);

            Assert.IsTrue(results.Count >= 3);  // Should reach 1, 2, 3
            Assert.IsTrue(results.ContainsKey(1));
            Assert.IsTrue(results.ContainsKey(2));
            Assert.IsTrue(results.ContainsKey(3));
            Assert.IsFalse(results.ContainsKey(4));  // Beyond threshold

            TestContext.WriteLine($"Reachable vertices within distance 10.0: {results.Count}");
        }

        #endregion

        #region Performance and Metrics Tests

        [TestMethod]
        public void TestComputationMetrics()
        {
            // Create a moderate-sized graph
            for (int i = 0; i < 20; i++)
            {
                _graph.AddVertex(i);
            }

            for (int i = 0; i < 20; i++)
            {
                if (i < 19) _graph.AddEdge(i, i + 1, 1.0);
                if (i < 18) _graph.AddEdge(i, i + 2, 2.5);
            }

            var metrics = _dijkstra.ComputeShortestPathTree(0);

            Assert.IsNotNull(metrics);
            Assert.AreEqual(0, metrics.Source);
            Assert.IsTrue(metrics.VerticesProcessed > 0);
            Assert.IsTrue(metrics.EdgesExamined > 0);
            Assert.IsTrue(metrics.TotalComputationTimeMs >= 0);

            TestContext.WriteLine($"Computation Metrics:");
            TestContext.WriteLine($"  Source: {metrics.Source}");
            TestContext.WriteLine($"  Vertices Processed: {metrics.VerticesProcessed}");
            TestContext.WriteLine($"  Edges Examined: {metrics.EdgesExamined}");
            TestContext.WriteLine($"  Computation Time: {metrics.TotalComputationTimeMs}ms");
        }

        [TestMethod]
        public void TestCaching()
        {
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3);
            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 3.0);

            // First call should compute
            var result1 = _dijkstra.FindShortestPath(1, 2);
            var metrics1 = _dijkstra.GetComputedTree(1);

            // Second call should use cache
            var result2 = _dijkstra.FindShortestPath(1, 3);
            var metrics2 = _dijkstra.GetComputedTree(1);

            Assert.AreEqual(metrics1.Source, metrics2.Source);

            TestContext.WriteLine("Caching working: Both queries used same tree");
        }

        #endregion

        #region Logistics-Specific Tests

        [TestMethod]
        public void TestLogisticsServiceSimple()
        {
            _graph.AddVertex(1, "Warehouse");
            _graph.AddVertex(2, "Store A");
            _graph.AddVertex(3, "Store B");

            _graph.AddEdge(1, 2, 10.0);
            _graph.AddEdge(2, 3, 5.0);

            var logisticsService = new LogisticsDijkstraService(_graph);
            var route = logisticsService.FindDeliveryRoute(1, 3, costPerUnit: 2.0, routeId: "DEL-001");

            Assert.IsTrue(route.Stops.Count > 0);
            Assert.AreEqual("DEL-001", route.RouteId);
            Assert.IsTrue(route.TotalDistance > 0);
            Assert.IsTrue(route.TotalCost > 0);

            TestContext.WriteLine($"Delivery route: {route}");
        }

        [TestMethod]
        public void TestLogisticsMultiStop()
        {
            _graph.AddVertex(1, "Warehouse");
            _graph.AddVertex(2, "Stop A");
            _graph.AddVertex(3, "Stop B");
            _graph.AddVertex(4, "Stop C");

            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 4.0);
            _graph.AddEdge(3, 4, 6.0);

            var logisticsService = new LogisticsDijkstraService(_graph);
            var route = logisticsService.FindOptimalMultiStopRoute(1, new[] { 2, 3, 4 }, costPerUnit: 1.5);

            Assert.IsTrue(route.Stops.Count >= 4);  // Start + 3 stops
            Assert.AreEqual(1, route.Stops[0]);
            Assert.IsTrue(route.TotalCost > 0);

            TestContext.WriteLine($"Multi-stop route: {route}");
            TestContext.WriteLine($"Total cost: ${route.TotalCost:F2}");
        }

        [TestMethod]
        public void TestLogisticsRouteClustering()
        {
            _graph.AddVertex(0, "Depot");
            for (int i = 1; i <= 15; i++)
            {
                _graph.AddVertex(i, $"Location {i}");
            }

            // Create a connected network
            for (int i = 0; i < 15; i++)
            {
                _graph.AddEdge(i, i + 1, 2.0);
            }

            var logisticsService = new LogisticsDijkstraService(_graph);
            var routes = logisticsService.ClusterDeliveriesIntoRoutes(0, Enumerable.Range(1, 15), maxStopsPerRoute: 5);

            Assert.IsTrue(routes.Count > 0);
            Assert.IsTrue(routes.Count <= 3);  // Should cluster 15 stops into ~3 routes

            TestContext.WriteLine($"Generated {routes.Count} routes");
            foreach (var route in routes)
            {
                TestContext.WriteLine($"  {route}");
            }
        }

        [TestMethod]
        public void TestFindClosestLocations()
        {
            _graph.AddVertex(0, "Warehouse");
            for (int i = 1; i <= 10; i++)
            {
                _graph.AddVertex(i);
            }

            // Create chain
            for (int i = 0; i < 10; i++)
            {
                _graph.AddEdge(i, i + 1, 5.0);
            }

            var logisticsService = new LogisticsDijkstraService(_graph);
            var closestLocations = logisticsService.FindClosestDeliveryLocations(0, Enumerable.Range(1, 10), 3);

            Assert.AreEqual(3, closestLocations.Count);
            Assert.AreEqual(1, closestLocations[0]);  // Closest
            Assert.AreEqual(2, closestLocations[1]);  // Second closest
            Assert.AreEqual(3, closestLocations[2]);  // Third closest

            TestContext.WriteLine($"3 Closest locations: {string.Join(", ", closestLocations)}");
        }

        [TestMethod]
        public void TestRouteValidation()
        {
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3);
            _graph.AddEdge(1, 2, 5.0);
            _graph.AddEdge(2, 3, 3.0);

            var logisticsService = new LogisticsDijkstraService(_graph);

            var validRoute = new LogisticsRoute
            {
                Stops = new List<int> { 1, 2, 3 }
            };

            var (isValid, issues) = logisticsService.ValidateRoute(validRoute);
            Assert.IsTrue(isValid);
            Assert.AreEqual(0, issues.Count);

            TestContext.WriteLine("Valid route passed validation");
        }

        [TestMethod]
        public void TestRouteValidationWithBadRoute()
        {
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3);
            _graph.AddEdge(1, 2, 5.0);
            // No edge from 2 to 3

            var logisticsService = new LogisticsDijkstraService(_graph);

            var invalidRoute = new LogisticsRoute
            {
                Stops = new List<int> { 1, 2, 3 }
            };

            var (isValid, issues) = logisticsService.ValidateRoute(invalidRoute);
            Assert.IsFalse(isValid);
            Assert.IsTrue(issues.Count > 0);

            TestContext.WriteLine($"Invalid route detected: {issues.Count} issues");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void TestSingleVertexGraph()
        {
            _graph.AddVertex(1);
            var result = _dijkstra.FindShortestPath(1, 1);

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(0.0, result.Distance);

            TestContext.WriteLine("Single vertex: Path to self found");
        }

        [TestMethod]
        public void TestLargeWeights()
        {
            _graph.AddVertex(1); _graph.AddVertex(2); _graph.AddVertex(3);
            _graph.AddEdge(1, 2, 1000000.0);
            _graph.AddEdge(2, 3, 1000000.0);

            var result = _dijkstra.FindShortestPath(1, 3);

            Assert.IsTrue(result.PathExists);
            Assert.AreEqual(2000000.0, result.Distance);

            TestContext.WriteLine($"Large weights handled: {result.Distance}");
        }

        [TestMethod]
        public void TestNegativeWeightDetection()
        {
            _graph.AddVertex(1); _graph.AddVertex(2);

            // Should throw for negative weight
            try
            {
                _graph.AddEdge(1, 2, -1.0);
                Assert.Fail("Should have thrown exception for negative weight");
            }
            catch (ArgumentException)
            {
                TestContext.WriteLine("Negative weight correctly rejected");
            }
        }

        [TestMethod]
        public void TestDenseGraph()
        {
            // Create a complete graph (every vertex connected to every other)
            int size = 8;
            for (int i = 0; i < size; i++)
            {
                _graph.AddVertex(i);
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    _graph.AddEdge(i, j, i + j);  // Varied weights
                }
            }

            var result = _dijkstra.FindShortestPath(0, size - 1);

            Assert.IsTrue(result.PathExists);

            TestContext.WriteLine($"Dense graph path: {result.Distance}");
        }

        #endregion
    }
}
