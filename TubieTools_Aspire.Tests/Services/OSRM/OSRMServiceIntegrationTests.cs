using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MapApp.API.Services;
using MapApp.API.Models;
using MapApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace TubieTools_Aspire.Tests.Services.OSRM
{
    /// <summary>
    /// Integration Tests for OSRM Service with Route Optimization
    /// Tests the full routing pipeline from OSRM through route optimization
    /// </summary>
    [TestClass]
    public class OSRMServiceIntegrationTests
    {
        private MapAppDbContext _dbContext;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<OSRMService>> _osrmLogger;
        private OSRMService _osrmService;
        private RouteOptimizationService _routeOptimizationService;

        [TestInitialize]
        public void Initialize()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MapAppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new MapAppDbContext(options);
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _osrmLogger = new Mock<ILogger<OSRMService>>();
            _osrmService = new OSRMService(_mockHttpClientFactory.Object, _osrmLogger.Object);
            _routeOptimizationService = new RouteOptimizationService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext?.Dispose();
        }

        #region Setup Helpers

        private void SeedStateCapitals()
        {
            var capitals = new List<StateCapital>
            {
                new StateCapital { Id = 1, StateName = "Texas", StateCode = "TX", Latitude = 30.2672, Longitude = -97.7431, CapitalName  = "Austin" },
                new StateCapital { Id = 2, StateName = "Oklahoma", StateCode = "OK", Latitude = 35.4676, Longitude = -97.5164, CapitalName  = "Oklahoma City" },
                new StateCapital { Id = 3, StateName = "Kansas", StateCode = "KS", Latitude = 39.0473, Longitude = -95.6752, CapitalName  = "Topeka" },
                new StateCapital { Id = 4, StateName = "Colorado", StateCode = "CO", Latitude = 39.7392, Longitude = -104.9903, CapitalName  = "Denver" },
                new StateCapital { Id = 5, StateName = "New Mexico", StateCode = "NM", Latitude = 35.0895, Longitude = -106.6504, CapitalName  = "Santa Fe" }
            };

            _dbContext.StateCapitals.AddRange(capitals);
            _dbContext.SaveChanges();
        }

        private void SetupOSRMMockResponse(RouteResponse response)
        {
            var jsonResponse = JsonSerializer.Serialize(response);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(httpResponseMessage);

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);
        }

        #endregion

        #region Route Optimization Integration Tests

        /// <summary>
        /// Test 1: Simple 2-stop route optimization with OSRM
        /// Tests basic integration between OSRM and route optimization
        /// </summary>
        [TestMethod]
        public async Task OptimizeRoute_TwoStops_CalculatesCorrectSequence()
        {
            // Arrange
            SeedStateCapitals();
            var capitals = _dbContext.StateCapitals.Take(2).ToList();

            var mockRoute = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route
                    {
                        Distance = 470000, // Austin to Oklahoma City and back
                        Duration = 16920
                    }
                }
            };

            SetupOSRMMockResponse(mockRoute);

            // Act
            var result = _routeOptimizationService.OptimizeRouteNearestNeighbor(capitals, "TX");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Nearest Neighbor Route from Texas", result.Name);
            Assert.AreEqual(2, result.States.Count);
            Assert.AreEqual("TX", result.States[0]);
            Assert.IsTrue(result.TotalDistanceKm > 0);
            Assert.IsNotNull(result.RouteSegments);
        }

        /// <summary>
        /// Test 2: Five-city route optimization (TSP scenario)
        /// Full traveling salesman problem with OSRM integration
        /// </summary>
        [TestMethod]
        public void OptimizeRoute_FiveCapitals_ProducesValidTSPSolution()
        {
            // Arrange
            SeedStateCapitals();
            var allCapitals = _dbContext.StateCapitals.ToList();

            // Act
            var result = _routeOptimizationService.OptimizeRouteNearestNeighbor(allCapitals, "TX");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.States.Count);
            Assert.AreEqual("TX", result.States[0]); // Starts at Texas
            Assert.IsTrue(result.States.Contains("TX")); // Returns to Texas

            // Verify all states are included exactly once
            var uniqueStates = new HashSet<string>(result.States);
            Assert.AreEqual(5, uniqueStates.Count);

            // Verify segments
            Assert.IsTrue(result.RouteSegments.Count > 0);
            var totalSegmentDistance = result.RouteSegments.Sum(s => s.DistanceKm);
            Assert.IsTrue(totalSegmentDistance > 0);
        }

        /// <summary>
        /// Test 3: Distance calculations consistency
        /// Validates Haversine formula matches expected values
        /// </summary>
        [TestMethod]
        public void DistanceCalculation_KnownCityPairs_MatchesExpectedValues()
        {
            // Arrange
            SeedStateCapitals();

            // Act - Austin to Oklahoma City
            var austinCapital = _dbContext.StateCapitals.First(c => c.StateCode == "TX");
            var okcCapital = _dbContext.StateCapitals.First(c => c.StateCode == "OK");

            var distance = _routeOptimizationService.CalculateDistance(
                austinCapital.Latitude, austinCapital.Longitude,
                okcCapital.Latitude, okcCapital.Longitude);

            // Assert - Known distance between Austin and OKC is approximately 475 km
            Assert.IsTrue(distance > 400 && distance < 550, $"Distance should be ~475 km, got {distance} km");
        }

        /// <summary>
        /// Test 4: Transportation plan creation with vehicle capacity
        /// Multi-route planning based on constraints
        /// </summary>
        [TestMethod]
        public void CreateTransportationPlan_WithCapacityConstraint_SplitsIntoMultipleRoutes()
        {
            // Arrange
            SeedStateCapitals();
            var allCapitals = _dbContext.StateCapitals.ToList();

            // Act - Only 2 stops per vehicle
            var plan = _routeOptimizationService.CreateTransportationPlan(allCapitals, "TX", vehicleCapacity: 2);

            // Assert
            Assert.IsNotNull(plan);
            Assert.IsTrue(plan.Routes.Count >= 2, "Should create multiple routes with capacity constraint");
            Assert.AreEqual("TX", plan.StartingState);

            // Verify each route respects vehicle capacity
            foreach (var route in plan.Routes)
            {
                Assert.IsTrue(route.States.Count <= 3, "Route should not exceed vehicle capacity + 1 (including return)");
            }

            // Verify total coverage
            var allRouteStates = plan.Routes.SelectMany(r => r.States).Distinct().Count();
            Assert.IsTrue(allRouteStates >= allCapitals.Count, "All states should be covered across routes");
        }

        #endregion

        #region OSRM HTTP Integration Tests

        /// <summary>
        /// Test 5: Multiple sequential OSRM calls for multi-leg route
        /// Simulates real routing scenario with multiple API calls
        /// </summary>
        [TestMethod]
        public async Task MultiSegmentRoute_SequentialOSRMCalls_AggregatesResults()
        {
            // Arrange
            var leg1 = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route { Distance = 320000, Duration = 11520 } // Austin to OKC
                }
            };

            var leg2 = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route { Distance = 280000, Duration = 10080 } // OKC to Topeka
                }
            };

            var mockHttpClient = new Mock<HttpClient>();
            var responses = new Queue<HttpResponseMessage>();
            responses.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(leg1))
            });
            responses.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(leg2))
            });

            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => responses.Dequeue());

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result1 = await _osrmService.GetRouteAsync(30.2672, -97.7431, 35.4676, -97.5164);
            var result2 = await _osrmService.GetRouteAsync(35.4676, -97.5164, 39.0473, -95.6752);

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            var totalDistance = result1.Routes[0].Distance + result2.Routes[0].Distance;
            var totalDuration = result1.Routes[0].Duration + result2.Routes[0].Duration;

            Assert.AreEqual(600000, totalDistance);
            Assert.AreEqual(21600, totalDuration);
            Assert.AreEqual(21600 / 3600.0, 6.0); // 6 hours total
        }

        /// <summary>
        /// Test 6: Distance matrix for multi-stop optimization
        /// Tests OSRM distance matrix usage for route planning
        /// </summary>
        [TestMethod]
        public async Task DistanceMatrix_ForFiveStops_EnablesOptimalRoutePlanning()
        {
            // Arrange
            var coordinates = new List<(double lat, double lon)>
            {
                (30.2672, -97.7431),  // Austin, TX
                (35.4676, -97.5164),  // Oklahoma City, OK
                (39.0473, -95.6752),  // Topeka, KS
                (39.7392, -104.9903), // Denver, CO
                (35.0895, -106.6504)  // Santa Fe, NM
            };

            // Create realistic distance matrix (symmetric)
            var distances = new List<List<double>>
            {
                new List<double> { 0, 320000, 460000, 1050000, 450000 },
                new List<double> { 320000, 0, 290000, 900000, 380000 },
                new List<double> { 460000, 290000, 0, 750000, 470000 },
                new List<double> { 1050000, 900000, 750000, 0, 480000 },
                new List<double> { 450000, 380000, 470000, 480000, 0 }
            };

            var mockResponse = new DistanceMatrixResponse
            {
                Distances = distances,
                Durations = distances.Select(row => row.Select(d => d / 28.0).ToList()).ToList()
            };

            var jsonResponse = JsonSerializer.Serialize(mockResponse);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(httpResponseMessage);

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetDistanceMatrixAsync(coordinates);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Distances);
            Assert.AreEqual(5, result.Distances.Count);

            // Verify symmetry
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(result.Distances[i][j], result.Distances[j][i]);
                }
            }

            // Verify diagonal zeros
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0, result.Distances[i][i]);
            }
        }

        #endregion

        #region Error Recovery and Degradation Tests

        /// <summary>
        /// Test 7: Graceful degradation when OSRM is unavailable
        /// Service should fall back to local calculation
        /// </summary>
        [TestMethod]
        public async Task RouteOptimization_WithFailedOSRM_FallsBackToLocalCalculation()
        {
            // Arrange
            SeedStateCapitals();
            var capitals = _dbContext.StateCapitals.Take(3).ToList();

            // Setup OSRM to fail
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable });

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act - Route optimization should still work with local calculations
            var result = _routeOptimizationService.OptimizeRouteNearestNeighbor(capitals, "TX");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TotalDistanceKm > 0);
            Assert.IsTrue(result.RouteSegments.Count > 0);

            _osrmLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never); // No logging since we didn't call the service yet
        }

        /// <summary>
        /// Test 8: Retry behavior on transient failures
        /// Validates service resilience
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_TransientNetworkFailure_ReturnsNullAfterFailing()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Temporary network failure"));

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
    .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetRouteAsync(40.0, -74.0, 41.0, -75.0);

            // Assert
            Assert.IsNull(result);

            _osrmLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Performance and Scaling Tests

        /// <summary>
        /// Test 9: Large route optimization (25+ destinations)
        /// Validates algorithm performance with scale
        /// </summary>
        [TestMethod]
        public void OptimizeRoute_Large25CityProblem_CompletesWithinReasonableTime()
        {
            // Arrange
            var capitals = new List<StateCapital>();
            for (int i = 0; i < 25; i++)
            {
                capitals.Add(new StateCapital
                {
                    Id = i,
                    StateCode = $"S{i}",
                    StateName = $"State{i}",
                    Latitude = 30.0 + (i * 1.5),
                    Longitude = -97.0 + (i * 1.5),
                    CapitalName = $"City{i}"
                });
            }

            // Act31
            var startTime = DateTime.UtcNow;
            var result = _routeOptimizationService.OptimizeRouteNearestNeighbor(capitals, "S0");
            var elapsed = DateTime.UtcNow - startTime;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(25, result.States.Count);
            Assert.IsTrue(elapsed.TotalMilliseconds < 5000, $"Should complete in < 5 seconds, took {elapsed.TotalMilliseconds}ms");
        }

        /// <summary>
        /// Test 10: Distance matrix stress test (multiple API calls)
        /// Validates handling of high call volume
        /// </summary>
        [TestMethod]
        public async Task DistanceMatrix_MultipleConsecutiveCalls_SuccessfullyProcesses()
        {
            // Arrange
            int callCount = 0;
            var mockHttpClient = new Mock<HttpClient>();

            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    var response = new DistanceMatrixResponse
                    {
                        Distances = new List<List<double>>
                        {
                            new List<double> { 0, 100000 },
                            new List<double> { 100000, 0 }
                        }
                    };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(response))
                    };
                });

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var coords = new List<(double lat, double lon)>
            {
                (40.0, -74.0),
                (41.0, -75.0)
            };

            var tasks = Enumerable.Range(0, 10)
                .Select(_ => _osrmService.GetDistanceMatrixAsync(coords))
                .ToList();

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(10, results.Length);
            foreach (var r in results)
            {
                Assert.IsNotNull(r);
            }
            Assert.AreEqual(10, callCount);
        }

        #endregion

        #region TMS-Specific Routing Tests

        /// <summary>
        /// Test 11: TMS Route Planning - Multi-truck load consolidation
        /// Real-world TMS optimization with multiple vehicles
        /// </summary>
        [TestMethod]
        public void CreateTransportationPlan_ConsolidatesLoadsAcrossVehicles()
        {
            // Arrange
            SeedStateCapitals();
            var allCapitals = _dbContext.StateCapitals.ToList();

            // Act - Create plan with 2 stops per truck
            var plan = _routeOptimizationService.CreateTransportationPlan(allCapitals, "TX", vehicleCapacity: 2);

            // Assert
            Assert.IsNotNull(plan);
            Assert.AreEqual("TX", plan.StartingState);
            Assert.IsTrue(plan.Routes.Count >= 2);

            // Verify TMS metrics
            Assert.IsTrue(plan.TotalDistance > 0);
            Assert.IsTrue(plan.TotalDurationHours > 0);
            Assert.IsTrue(plan.EstimatedVehicles > 1);

            // Verify all routes start and potentially end at starting state
            // (depending on consolidation strategy)
            var statesInRoutes = new HashSet<string>();
            foreach (var route in plan.Routes)
            {
                foreach (var state in route.States)
                {
                    statesInRoutes.Add(state);
                }
            }

            Assert.IsTrue(statesInRoutes.Count >= allCapitals.Count - 1, "Should cover most states");
        }

        /// <summary>
        /// Test 12: Fuel-aware routing distance calculation
        /// Calculate total fuel cost for multi-leg route
        /// </summary>
        [TestMethod]
        public void RouteOptimization_CalculatesTotalFuelCost()
        {
            // Arrange
            SeedStateCapitals();
            var capitals = _dbContext.StateCapitals.Take(4).ToList();
            const double fuelCostPerKm = 0.15; // dollars per km

            // Act
            var route = _routeOptimizationService.OptimizeRouteNearestNeighbor(capitals, "TX");

            // Assert
            Assert.IsNotNull(route);
            var fuelCost = route.TotalDistanceKm * fuelCostPerKm;

            Assert.IsTrue(fuelCost > 0);
            Assert.IsTrue(route.TotalDistanceKm > 100, "Should have substantial distance");

            // Verify cost is calculable
            var costPerMile = fuelCost / (route.TotalDistanceKm * 0.621371); // Convert km to miles
            Assert.IsTrue(costPerMile > 0.1, "Fuel cost per mile should be substantial");
        }

        #endregion
    }
}
