using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MapApp.API.Services;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.Tests.Services.OSRM
{

    /// <summary>
    /// End-to-End Tests for OSRMService
    /// Tests real-world routing scenarios, distance matrix calculations, and error handling
    /// These tests use mocked HTTP responses to simulate OSRM API behavior.
    /// 
    /// This vibe coded nightmare cannot instantiate OSRMService directly due to its dependency on IHttpClientFactory and ILogger.
    /// The objects that come in are mocked using Moq to simulate the behavior of the OSRM API without making real HTTP calls.
    /// The tests never pass due  to the limitation of instantiating an httpclient.CreateClient resulting in null.
    /// </summary>
    [TestClass]
    public class OSRMServiceEndToEndTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<OSRMService>> _mockLogger;
        private OSRMService _osrmService;

        [TestInitialize]
        public void Initialize()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<OSRMService>>();
            _osrmService = new OSRMService(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        #region Route Tests

        /// <summary>
        /// Test 1: Get route between two coordinate points (Dallas to Houston)
        /// Validates successful route retrieval with distance and duration
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_ValidCoordinates_ReturnsRouteResponse()
        {
            // Arrange
            var dallaLat = 32.7767;
            var dallaLon = -96.7970;
            var houstonLat = 29.7604;
            var houstonLon = -95.3698;

            var mockResponse = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route
                    {
                        Distance = 385000, // meters (385 km)
                        Duration = 13860,  // seconds (~3.85 hours)
                        Geometry = new Geometry
                        {
                            Type = "LineString",
                            Coordinates = new List<List<double>>
                            {
                                new List<double> { dallaLon, dallaLat },
                                new List<double> { houstonLon, houstonLat }
                            }
                        }
                    }
                }
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
            var result = await _osrmService.GetRouteAsync(dallaLat, dallaLon, houstonLat, houstonLon);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Routes);
            Assert.AreEqual(1, result.Routes.Count);
            Assert.AreEqual(385000, result.Routes[0].Distance);
            Assert.AreEqual(13860, result.Routes[0].Duration);
            Assert.IsNotNull(result.Routes[0].Geometry);
            Assert.AreEqual("LineString", result.Routes[0].Geometry.Type);
            Assert.IsNotNull(result.Routes[0].Geometry.Coordinates);

            // Verify HTTP call was made
            mockHttpClient.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Test 2: Get route across state boundaries (Los Angeles to New York)
        /// Long-distance route validation
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_CrossCountryRoute_ReturnsCorrectDistance()
        {
            // Arrange
            var laLat = 34.0522;
            var laLon = -118.2437;
            var nyLat = 40.7128;
            var nyLon = -74.0060;

            var mockResponse = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route
                    {
                        Distance = 4505000, // ~4500 km
                        Duration = 162180   // ~45 hours driving
                    }
                }
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
            var result = await _osrmService.GetRouteAsync(laLat, laLon, nyLat, nyLon);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Routes);
            var route = result.Routes[0];
            Assert.IsTrue(route.Distance > 4000000, "Cross-country distance should be > 4000 km");
            Assert.IsTrue(route.Duration > 140000, "Cross-country duration should be > 38 hours");
        }

        /// <summary>
        /// Test 3: Get route with same start and end coordinates
        /// Edge case: zero distance route
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_SameCoordinates_ReturnsZeroDistance()
        {
            // Arrange
            var lat = 35.0895;
            var lon = -106.6504;

            var mockResponse = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route
                    {
                        Distance = 0,
                        Duration = 0
                    }
                }
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
            var result = await _osrmService.GetRouteAsync(lat, lon, lat, lon);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Routes);
            Assert.AreEqual(1, result.Routes.Count);
            Assert.AreEqual(0, result.Routes[0].Distance);
            Assert.AreEqual(0, result.Routes[0].Duration);
        }

        /// <summary>
        /// Test 4: Get route with full geometry/coordinates
        /// Validates complete polyline path
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_WithGeometry_ContainsCompleteCoordinates()
        {
            // Arrange
            var startLat = 38.9072;
            var startLon = -77.0369; // Washington DC
            var endLat = 39.9526;
            var endLon = -75.1652;   // Philadelphia

            var mockRoute = new Route
            {
                Distance = 310000, // 310 km
                Duration = 11160,  // 3.1 hours
                Geometry = new Geometry
                {
                    Type = "LineString",
                    Coordinates = new List<List<double>>
                    {
                        new List<double> { startLon, startLat },
                        new List<double> { -76.5000, 39.0000 },
                        new List<double> { -75.5000, 39.5000 },
                        new List<double> { endLon, endLat }
                    }
                }
            };

            var mockResponse = new RouteResponse { Routes = new List<Route> { mockRoute } };
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
            var result = await _osrmService.GetRouteAsync(startLat, startLon, endLat, endLon);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Routes[0].Geometry);
            Assert.AreEqual(4, result.Routes[0].Geometry.Coordinates.Count);
            Assert.AreEqual(startLon, result.Routes[0].Geometry.Coordinates[0][0]);
            Assert.AreEqual(startLat, result.Routes[0].Geometry.Coordinates[0][1]);
        }

        /// <summary>
        /// Test 5: HTTP error handling (API timeout)
        /// Validates error resilience
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_HttpErrorResponse_ReturnsNull()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable });

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetRouteAsync(40.0, -74.0, 41.0, -75.0);

            // Assert
            Assert.IsNull(result);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test 6: Network exception handling
        /// Validates exception resilience
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_NetworkException_ReturnsNullAndLogs()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetRouteAsync(40.0, -74.0, 41.0, -75.0);

            // Assert
            Assert.IsNull(result);

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Distance Matrix Tests

        /// <summary>
        /// Test 7: Get distance matrix for 3 coordinates
        /// Validates symmetric distance matrix
        /// </summary>
        [TestMethod]
        public async Task GetDistanceMatrixAsync_ThreeCoordinates_ReturnsSquareMatrix()
        {
            // Arrange
            var coordinates = new List<(double lat, double lon)>
            {
                (32.7767, -96.7970), // Dallas
                (29.7604, -95.3698), // Houston
                (33.6874, -97.3961)  // Fort Worth
            };

            var mockResponse = new DistanceMatrixResponse
            {
                Distances = new List<List<double>>
                {
                    new List<double> { 0, 385000, 85000 },           // Dallas to [Dallas, Houston, Fort Worth]
                    new List<double> { 385000, 0, 315000 },          // Houston to [Dallas, Houston, Fort Worth]
                    new List<double> { 85000, 315000, 0 }            // Fort Worth to [Dallas, Houston, Fort Worth]
                },
                Durations = new List<List<double>>
                {
                    new List<double> { 0, 13860, 3060 },             // in seconds
                    new List<double> { 13860, 0, 11340 },
                    new List<double> { 3060, 11340, 0 }
                }
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
            Assert.IsNotNull(result.Durations);
            Assert.AreEqual(3, result.Distances.Count);
            Assert.AreEqual(3, result.Distances[0].Count);

            // Verify diagonal is zero (each point to itself)
            Assert.AreEqual(0, result.Distances[0][0]);
            Assert.AreEqual(0, result.Distances[1][1]);
            Assert.AreEqual(0, result.Distances[2][2]);

            // Verify symmetry
            Assert.AreEqual(result.Distances[0][1], result.Distances[1][0]);
            Assert.AreEqual(result.Distances[0][2], result.Distances[2][0]);
        }

        /// <summary>
        /// Test 8: Distance matrix with 5 state capitals
        /// Simulates real TSP scenario
        /// </summary>
        [TestMethod]
        public async Task GetDistanceMatrixAsync_FiveCapitals_ReturnsValidMatrix()
        {
            // Arrange
            var coordinates = new List<(double lat, double lon)>
            {
                (32.3668, -86.2934), // Montgomery, AL
                (34.7465, -92.2896), // Little Rock, AR
                (33.4484, -112.0742), // Phoenix, AZ
                (38.5816, -121.4944), // Sacramento, CA
                (39.7392, -104.9903)  // Denver, CO
            };

            var mockResponse = new DistanceMatrixResponse
            {
                Distances = new List<List<double>>
                {
                    new List<double> { 0, 650000, 2100000, 2900000, 2400000 },
                    new List<double> { 650000, 0, 1800000, 2600000, 2100000 },
                    new List<double> { 2100000, 1800000, 0, 950000, 1200000 },
                    new List<double> { 2900000, 2600000, 950000, 0, 1600000 },
                    new List<double> { 2400000, 2100000, 1200000, 1600000, 0 }
                },
                Durations = new List<List<double>>
                {
                    new List<double> { 0, 23400, 75600, 104400, 86400 },
                    new List<double> { 23400, 0, 64800, 93600, 75600 },
                    new List<double> { 75600, 64800, 0, 34200, 43200 },
                    new List<double> { 104400, 93600, 34200, 0, 57600 },
                    new List<double> { 86400, 75600, 43200, 57600, 0 }
                }
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
            Assert.AreEqual(5, result.Distances.Count);
            foreach (var row in result.Distances)
            {
                Assert.AreEqual(5, row.Count);
            }
            Assert.AreEqual(5, result.Durations.Count);

            // Verify each duration is related to distance (approximately distance / 35 m/s)
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (result.Distances[i][j] > 0)
                    {
                        var expectedDuration = result.Distances[i][j] / 35;
                        Assert.IsTrue(
                            Math.Abs(result.Durations[i][j] - expectedDuration) < expectedDuration * 0.2,
                            $"Duration [{i},{j}] should be proportional to distance");
                    }
                }
            }
        }

        /// <summary>
        /// Test 9: Distance matrix exceeding 25 coordinate limit
        /// Validates truncation to valid API limits
        /// </summary>
        [TestMethod]
        public async Task GetDistanceMatrixAsync_ExceedLimit_TruncatesTo25()
        {
            // Arrange
            var coordinates = new List<(double lat, double lon)>();
            for (int i = 0; i < 30; i++)
            {
                coordinates.Add((32.0 + i * 0.1, -96.0 + i * 0.1));
            }

            var mockResponse = new DistanceMatrixResponse
            {
                Distances = Enumerable.Range(0, 25)
                    .Select(i => Enumerable.Range(0, 25).Select(j => i == j ? 0.0 : 100000.0).ToList())
                    .ToList(),
                Durations = Enumerable.Range(0, 25)
                    .Select(i => Enumerable.Range(0, 25).Select(j => i == j ? 0.0 : 3600.0).ToList())
                    .ToList()
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
            Assert.AreEqual(25, result.Distances.Count);

            // Verify warning was logged about truncation
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Test 10: Single coordinate distance matrix
        /// Edge case: matrix with one point
        /// </summary>
        [TestMethod]
        public async Task GetDistanceMatrixAsync_SingleCoordinate_ReturnsUnitMatrix()
        {
            // Arrange
            var coordinates = new List<(double lat, double lon)>
            {
                (32.7767, -96.7970) // Dallas only
            };

            var mockResponse = new DistanceMatrixResponse
            {
                Distances = new List<List<double>>
                {
                    new List<double> { 0 }
                },
                Durations = new List<List<double>>
                {
                    new List<double> { 0 }
                }
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
            Assert.AreEqual(1, result.Distances.Count);
            Assert.AreEqual(1, result.Distances[0].Count);
            Assert.AreEqual(0, result.Distances[0][0]);
        }

        /// <summary>
        /// Test 11: Distance matrix with HTTP error
        /// Error handling for matrix requests
        /// </summary>
        [TestMethod]
        public async Task GetDistanceMatrixAsync_ApiError_ReturnsNullAndLogs()
        {
            // Arrange
            var coordinates = new List<(double lat, double lon)>
            {
                (40.0, -74.0),
                (41.0, -75.0)
            };

            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
    .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetDistanceMatrixAsync(coordinates);

            // Assert
            Assert.IsNull(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Integration Scenarios

        /// <summary>
        /// Test 12: Route planning scenario - Planning a route across 4 states
        /// End-to-end workflow
        /// </summary>
        [TestMethod]
        public async Task MultipleRouteRequests_PlanningScenario_SuccessfullyChainsRequests()
        {
            // Arrange
            var route1 = new RouteResponse
            {
                Routes = new List<Route> { new Route { Distance = 385000, Duration = 13860 } }
            };
            var route2 = new RouteResponse
            {
                Routes = new List<Route> { new Route { Distance = 250000, Duration = 9000 } }
            };
            var route3 = new RouteResponse
            {
                Routes = new List<Route> { new Route { Distance = 420000, Duration = 15120 } }
            };

            var mockHttpClient = new Mock<HttpClient>();
            var responses = new Queue<HttpResponseMessage>();
            responses.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(route1))
            });
            responses.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(route2))
            });
            responses.Enqueue(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(route3))
            });

            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => responses.Dequeue());

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result1 = await _osrmService.GetRouteAsync(32.7767, -96.7970, 29.7604, -95.3698);
            var result2 = await _osrmService.GetRouteAsync(29.7604, -95.3698, 33.6874, -97.3961);
            var result3 = await _osrmService.GetRouteAsync(33.6874, -97.3961, 32.7767, -96.7970);

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.IsNotNull(result3);

            var totalDistance = result1.Routes[0].Distance + result2.Routes[0].Distance + result3.Routes[0].Distance;
            var totalDuration = result1.Routes[0].Duration + result2.Routes[0].Duration + result3.Routes[0].Duration;

            Assert.AreEqual(1055000, totalDistance);
            Assert.AreEqual(37980, totalDuration);

            mockHttpClient.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Exactly(3));
        }

        /// <summary>
        /// Test 13: Truck routing for TMS - Route optimization scenario
        /// Real-world TMS use case
        /// </summary>
        [TestMethod]
        public async Task OSRMService_TruckRoutingScenario_CalculatesValidTravelTime()
        {
            // Arrange - Cross-state truck route
            var pickupLat = 32.3668;  // Montgomery, AL
            var pickupLon = -86.2934;
            var deliveryLat = 39.7392; // Denver, CO
            var deliveryLon = -104.9903;

            var mockResponse = new RouteResponse
            {
                Routes = new List<Route>
                {
                    new Route
                    {
                        Distance = 2400000, // 2400 km
                        Duration = 86400    // 24 hours at 100 km/h average
                    }
                }
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
            var result = await _osrmService.GetRouteAsync(pickupLat, pickupLon, deliveryLat, deliveryLon);

            // Assert
            Assert.IsNotNull(result);
            var route = result.Routes[0];

            // Distance: 2400 km
            Assert.AreEqual(2400000, route.Distance);

            // Duration: 24 hours (86400 seconds)
            Assert.AreEqual(86400, route.Duration);

            // HOS Validation: Driver needs 2 mandatory 10-hour breaks for 24-hour drive
            var totalHours = route.Duration / 3600.0;
            var mandatoryBreaks = (int)(totalHours / 11.0); // One break every 11 hours
            Assert.IsTrue(mandatoryBreaks >= 2, "Should require at least 2 mandatory breaks");
        }

        /// <summary>
        /// Test 14: Fail-fast behavior with multiple retries
        /// Tests resilience under degraded service
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_WithRetryLogic_EventuallyFails()
        {
            // Arrange
            int attemptCount = 0;
            var mockHttpClient = new Mock<HttpClient>();

            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    attemptCount++;
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable };
                });

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act - Simulating retry logic
            var result = await _osrmService.GetRouteAsync(40.0, -74.0, 41.0, -75.0);

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(1, attemptCount); // Service doesn't retry internally
        }

        #endregion

        #region Data Validation Tests

        /// <summary>
        /// Test 15: Invalid coordinate handling
        /// Validates coordinate boundary conditions
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_InvalidCoordinates_StillCallsAPI()
        {
            // Arrange - Coordinates outside normal range (but technically valid lat/lon)
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(
                        new RouteResponse { Routes = new List<Route>() }))
                });

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetRouteAsync(91.0, 181.0, -91.0, -181.0);

            // Assert
            Assert.IsNotNull(result);

            // API is called regardless - validation is upstream
            mockHttpClient.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Test 16: JSON deserialization edge cases
        /// Handles malformed API responses
        /// </summary>
        [TestMethod]
        public async Task GetRouteAsync_MalformedJSON_ReturnsNullAndLogs()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ invalid json }")
                });

            _mockHttpClientFactory.Setup(x => x.CreateClient())
                .Returns(mockHttpClient.Object);

            // Act
            var result = await _osrmService.GetRouteAsync(40.0, -74.0, 41.0, -75.0);

            // Assert
            Assert.IsNull(result);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion
    }
}
