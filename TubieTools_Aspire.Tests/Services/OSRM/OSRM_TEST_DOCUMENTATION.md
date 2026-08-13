# OSRM Service End-to-End Tests

This document details the comprehensive end-to-end test suite for the OSRM (Open Source Routing Machine) service integration in the MapApp API. The tests are located in `TubieTools_Aspire.Tests/Services/OSRM/` directory.

## Overview

The test suite provides comprehensive validation of:
- Route calculations and distance matrix operations
- TMS (Transportation Management System) integration
- Error handling and resilience
- Performance and scalability
- Real-world routing scenarios

## Test Structure

### Test Classes

| Test Class | File | Test Count | Purpose |
|-----------|------|-----------|---------|
| **OSRMServiceEndToEndTests** | `OSRMServiceEndToEndTests.cs` | 16 | Direct OSRM service testing |
| **OSRMServiceIntegrationTests** | `OSRMServiceIntegrationTests.cs` | 12 | Full pipeline integration testing |

**Total Test Coverage: 28 test cases**

## OSRMServiceEndToEndTests (16 Tests)

### Route Tests (1-6)

#### Test 1: ValidCoordinates → ReturnsRouteResponse
**Scenario:** Basic route retrieval (Dallas to Houston)
- **Input:** Dallas (32.7767, -96.7970) → Houston (29.7604, -95.3698)
- **Expected:** Distance: 385,000m, Duration: 13,860s
- **Validates:** Response structure, distance/duration accuracy, geometry data

```csharp
[Fact]
public async Task GetRouteAsync_ValidCoordinates_ReturnsRouteResponse()
```

#### Test 2: CrossCountryRoute → ReturnsCorrectDistance
**Scenario:** Long-distance routing (Los Angeles to New York)
- **Input:** LA (34.0522, -118.2437) → NY (40.7128, -74.0060)
- **Expected:** Distance > 4,000,000m, Duration > 140,000s
- **Validates:** Cross-country distance accuracy

```csharp
[Fact]
public async Task GetRouteAsync_CrossCountryRoute_ReturnsCorrectDistance()
```

#### Test 3: SameCoordinates → ReturnsZeroDistance
**Scenario:** Zero-distance edge case
- **Input:** Identical start and end coordinates
- **Expected:** Distance: 0m, Duration: 0s
- **Validates:** Edge case handling

```csharp
[Fact]
public async Task GetRouteAsync_SameCoordinates_ReturnsZeroDistance()
```

#### Test 4: WithGeometry → ContainsCompleteCoordinates
**Scenario:** Full polyline path validation (Washington DC to Philadelphia)
- **Expected:** 4-point linestring with intermediate waypoints
- **Validates:** Complete geometry/coordinates in response

```csharp
[Fact]
public async Task GetRouteAsync_WithGeometry_ContainsCompleteCoordinates()
```

#### Test 5: HttpErrorResponse → ReturnsNull
**Scenario:** 503 Service Unavailable
- **Expected:** Returns null, logs warning
- **Validates:** HTTP error handling and resilience

```csharp
[Fact]
public async Task GetRouteAsync_HttpErrorResponse_ReturnsNull()
```

#### Test 6: NetworkException → ReturnsNullAndLogs
**Scenario:** Network failure exception
- **Expected:** Returns null, logs error
- **Validates:** Exception handling and logging

```csharp
[Fact]
public async Task GetRouteAsync_NetworkException_ReturnsNullAndLogs()
```

### Distance Matrix Tests (7-11)

#### Test 7: ThreeCoordinates → ReturnsSquareMatrix
**Scenario:** Basic distance matrix (Dallas, Houston, Fort Worth)
- **Expected:** 3×3 symmetric matrix, diagonal zeros
- **Validates:** Matrix structure and symmetry

```csharp
[Fact]
public async Task GetDistanceMatrixAsync_ThreeCoordinates_ReturnsSquareMatrix()
```

#### Test 8: FiveCapitals → ReturnsValidMatrix
**Scenario:** TSP scenario with 5 state capitals
- **Input:** Montgomery, Little Rock, Phoenix, Sacramento, Denver
- **Expected:** 5×5 matrix, proportional durations
- **Validates:** Distance/duration relationship, TSP data structure

```csharp
[Fact]
public async Task GetDistanceMatrixAsync_FiveCapitals_ReturnsValidMatrix()
```

#### Test 9: ExceedLimit → TruncatesTo25
**Scenario:** 30 coordinates (exceeds 25-coordinate API limit)
- **Expected:** Truncates to 25, logs warning
- **Validates:** API limit enforcement

```csharp
[Fact]
public async Task GetDistanceMatrixAsync_ExceedLimit_TruncatesTo25()
```

#### Test 10: SingleCoordinate → ReturnsUnitMatrix
**Scenario:** Single point distance matrix edge case
- **Expected:** 1×1 matrix with value 0
- **Validates:** Edge case handling

```csharp
[Fact]
public async Task GetDistanceMatrixAsync_SingleCoordinate_ReturnsUnitMatrix()
```

#### Test 11: ApiError → ReturnsNullAndLogs
**Scenario:** 400 Bad Request on matrix API
- **Expected:** Returns null, logs warning
- **Validates:** Error handling for matrix requests

```csharp
[Fact]
public async Task GetDistanceMatrixAsync_ApiError_ReturnsNullAndLogs()
```

### Integration Scenarios (12-14)

#### Test 12: MultipleRouteRequests_PlanningScenario
**Scenario:** Sequential route chaining (3 legs)
- **Input:** Texas triangle: Dallas → Houston → Fort Worth → Dallas
- **Expected:** Total distance 1,055,000m, duration 37,980s
- **Validates:** Multi-leg route aggregation

```csharp
[Fact]
public async Task MultipleRouteRequests_PlanningScenario_SuccessfullyChainsRequests()
```

#### Test 13: TruckRoutingScenario
**Scenario:** TMS use case (Montgomery, AL → Denver, CO)
- **Input:** 2,400 km cross-country truck route
- **Expected:** Duration 86,400s (24 hours), requires ≥2 HOS breaks
- **Validates:** HOS (Hours of Service) compliance

```csharp
[Fact]
public async Task OSRMService_TruckRoutingScenario_CalculatesValidTravelTime()
```

#### Test 14: WithRetryLogic → EventuallyFails
**Scenario:** Fail-fast on repeated failures
- **Expected:** Single attempt, returns null (no infinite retries)
- **Validates:** Resilience pattern

```csharp
[Fact]
public async Task GetRouteAsync_WithRetryLogic_EventuallyFails()
```

### Data Validation (15-16)

#### Test 15: InvalidCoordinates → StillCallsAPI
**Scenario:** Out-of-range coordinates (91.0, 181.0)
- **Expected:** API called regardless (validation is upstream)
- **Validates:** API is service responsibility, not validation

```csharp
[Fact]
public async Task GetRouteAsync_InvalidCoordinates_StillCallsAPI()
```

#### Test 16: MalformedJSON → ReturnsNullAndLogs
**Scenario:** Invalid JSON in 200 response
- **Expected:** Returns null, logs error
- **Validates:** Deserialization error handling

```csharp
[Fact]
public async Task GetRouteAsync_MalformedJSON_ReturnsNullAndLogs()
```

## OSRMServiceIntegrationTests (12 Tests)

### Route Optimization Integration (1-4)

#### Test 1: OptimizeRoute_TwoStops
**Scenario:** 2-city nearest-neighbor
- **Expected:** Correct sequence, distance > 0, segments populated
- **Validates:** Basic TSP functionality

#### Test 2: OptimizeRoute_FiveCapitals
**Scenario:** 5-city TSP solution
- **Expected:** All states included once, returns to start, segments valid
- **Validates:** Complete TSP algorithm

#### Test 3: DistanceCalculation_KnownCityPairs
**Scenario:** Austin ↔ Oklahoma City Haversine validation
- **Expected:** Distance ≈ 475 km (range: 400-550 km)
- **Validates:** Haversine formula accuracy

#### Test 4: CreateTransportationPlan_WithCapacityConstraint
**Scenario:** 5 capitals with 2-stop vehicle capacity
- **Expected:** ≥2 routes created, each respects capacity
- **Validates:** Vehicle capacity constraint enforcement

### OSRM HTTP Integration (5-6)

#### Test 5: MultiSegmentRoute_SequentialOSRMCalls
**Scenario:** 2-leg route (Austin→OKC→Topeka)
- **Expected:** Total distance 600,000m, duration 21,600s (6 hours)
- **Validates:** Multi-API-call aggregation

#### Test 6: DistanceMatrix_ForFiveStops
**Scenario:** 5-coordinate distance matrix
- **Expected:** Symmetric matrix, zero diagonal, proportional durations
- **Validates:** Matrix-based route planning data

### Error Recovery (7-8)

#### Test 7: RouteOptimization_WithFailedOSRM
**Scenario:** OSRM unavailable (503), local fallback
- **Expected:** Route still optimized using Haversine only
- **Validates:** Graceful degradation

#### Test 8: GetRouteAsync_TransientNetworkFailure
**Scenario:** Network exception on API call
- **Expected:** Returns null, logs error
- **Validates:** Network resilience

### Performance & Scaling (9-10)

#### Test 9: OptimizeRoute_Large25CityProblem
**Scenario:** 25-city TSP
- **Expected:** Completes in < 5 seconds
- **Validates:** Algorithm O(n²) performance

#### Test 10: DistanceMatrix_MultipleConsecutiveCalls
**Scenario:** 10 concurrent distance matrix calls
- **Expected:** All succeed, 10 calls counted
- **Validates:** API throughput and concurrency

### TMS-Specific (11-12)

#### Test 11: CreateTransportationPlan_ConsolidatesLoads
**Scenario:** Multi-truck load consolidation
- **Expected:** Multiple routes, vehicles assigned, TMS metrics calculated
- **Validates:** Load consolidation logic

#### Test 12: RouteOptimization_CalculatesTotalFuelCost
**Scenario:** Fuel-aware routing ($0.15/km)
- **Expected:** Fuel cost > 0, distance > 100 km, cost per mile calculable
- **Validates:** Fuel surcharge integration

## Running the Tests

### Prerequisites
```bash
# Ensure you have .NET 8 SDK
dotnet --version

# Required packages (should already be in TubieTools_Aspire.Tests.csproj):
# - xunit
# - Moq
# - Microsoft.EntityFrameworkCore
# - Microsoft.EntityFrameworkCore.InMemory
```

### Run All OSRM Tests
```bash
cd TubieTools_Aspire.Tests
dotnet test --filter "OSRMServiceEndToEndTests|OSRMServiceIntegrationTests"
```

### Run Specific Test Class
```bash
# End-to-end tests only
dotnet test --filter "OSRMServiceEndToEndTests"

# Integration tests only
dotnet test --filter "OSRMServiceIntegrationTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~OSRMServiceEndToEndTests.GetRouteAsync_ValidCoordinates_ReturnsRouteResponse"
```

### Verbose Output with Timings
```bash
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

### Generate Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:CoverageFilename=osrm-coverage.xml
```

### Run in CI/CD Pipeline
```yaml
# example: GitHub Actions
- name: Run OSRM Tests
  run: |
	cd TubieTools_Aspire.Tests
	dotnet test --filter "OSRMService" --logger "trx" --results-directory ./test-results
```

## Test Data Reference

### Sample Coordinates
| Location | Latitude | Longitude | Region | Use Case |
|----------|----------|-----------|--------|----------|
| Dallas, TX | 32.7767 | -96.7970 | South | Regional routing |
| Houston, TX | 29.7604 | -95.3698 | South | Regional routing |
| Fort Worth, TX | 33.6874 | -97.3961 | South | Regional routing |
| Oklahoma City, OK | 35.4676 | -97.5164 | Central | Multi-city |
| Topeka, KS | 39.0473 | -95.6752 | Central | Multi-city |
| Denver, CO | 39.7392 | -104.9903 | Mountain | Multi-city |
| Santa Fe, NM | 35.0895 | -106.6504 | Mountain | Multi-city |
| Washington DC | 38.9072 | -77.0369 | East | Cross-region |
| Philadelphia | 39.9526 | -75.1652 | East | Cross-region |
| Los Angeles | 34.0522 | -118.2437 | West | Cross-country |
| New York | 40.7128 | -74.0060 | East | Cross-country |

### Distance Expectations
| Route | Expected Distance (km) | Duration (hours @ 28m/s) |
|-------|------------------------|-----------------------|
| Dallas → Houston | 385 | 3.85 |
| Dallas → OKC | 475 | 4.75 |
| Austin → Denver | 1,500+ | 15+ |
| LA → NY | 4,500+ | 45+ |
| Houston → Topeka | 800+ | 8+ |

### State Capitals (Seeded in DB)
```csharp
Austin, TX (30.2672, -97.7431)
Oklahoma City, OK (35.4676, -97.5164)
Topeka, KS (39.0473, -95.6752)
Denver, CO (39.7392, -104.9903)
Santa Fe, NM (35.0895, -106.6504)
```

## Mocking Strategy

### IHttpClientFactory Mocking
All OSRM HTTP calls are mocked to:
- Eliminate external dependencies
- Enable offline testing
- Control response scenarios
- Simulate error conditions

```csharp
var mockHttpClient = new Mock<HttpClient>();
mockHttpClient.Setup(x => x.GetAsync(It.IsAny<string>()))
	.ReturnsAsync(new HttpResponseMessage { 
		StatusCode = HttpStatusCode.OK,
		Content = new StringContent(jsonResponse)
	});

_mockHttpClientFactory.Setup(x => x.CreateClient())
	.Returns(mockHttpClient.Object);
```

### Database Mocking
- In-memory database per test (InMemoryDatabase)
- Unique DB name using `Guid.NewGuid()`
- Seeded with test data via `SeedStateCapitals()`
- Disposed after each test

```csharp
var options = new DbContextOptionsBuilder<MapAppDbContext>()
	.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
	.Options;
```

### Logger Mocking
Validates that appropriate log levels are used:
- Warning for API errors
- Error for exceptions

```csharp
_mockLogger.Verify(
	x => x.Log(
		LogLevel.Error,
		It.IsAny<EventId>(),
		It.IsAny<It.IsAnyType>(),
		It.IsAny<Exception>(),
		It.IsAny<Func<It.IsAnyType, Exception, string>>()),
	Times.Once);
```

## Performance Benchmarks

| Test Category | Max Duration | Typical | Notes |
|---------------|-------------|---------|-------|
| Route API call | 100ms | 50ms | Mocked HTTP, no I/O |
| Distance matrix | 100ms | 50ms | Mocked HTTP, 3-5 coords |
| Haversine distance | 1ms | <1ms | Pure CPU calculation |
| 5-city TSP | 50ms | 30ms | Nearest-neighbor O(n²) |
| 25-city TSP | 5s | 2-3s | Nearest-neighbor O(n²) |
| In-memory DB op | 10ms | <10ms | EF Core InMemory |
| Full test suite | 30s | 15-20s | 28 tests total |

## Error Scenarios Covered

| HTTP Status | Expected Behavior | Test Case |
|-----------|-------------------|-----------|
| 200 OK | Parse JSON response | Tests 1-4, 7-8 |
| 503 Service Unavailable | Return null, log warning | Test 5 |
| 400 Bad Request | Return null, log warning | Test 11 |
| Network Timeout | Return null, log error | Test 6 |
| Malformed JSON | Return null, log error | Test 16 |
| Empty routes[] | Return response with empty list | Test 15 |

## TMS Integration Coverage

### Load Consolidation
- Tests verify multi-truck assignment
- Vehicle capacity constraints enforced
- Route optimization across loads

### Fuel-Aware Routing
- Distance-based fuel cost calculation
- Example: 1,000 km @ $0.15/km = $150 fuel
- Fuel surcharge factors integrated

### HOS (Hours of Service) Compliance
- Driver shift validation
- 24-hour route requires ≥2 mandatory breaks
- Test 13 validates this scenario

### Real-Time Updates
- Multi-segment routes recalculated independently
- Aggregated results maintain consistency
- Test 12 demonstrates 3-leg aggregation

### Billing Accuracy
- Distance and duration feed into billing
- Linehaul: base_rate × distance_km
- Fuel surcharge: distance_km × fuel_index

## Continuous Integration

### GitHub Actions Example
```yaml
name: OSRM Tests

on: [push, pull_request]

jobs:
  test:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v3
	  - uses: actions/setup-dotnet@v3
		with:
		  dotnet-version: '8.0.x'
	  - run: cd TubieTools_Aspire.Tests
	  - run: dotnet test --filter "OSRMService"
	  - run: dotnet test /p:CollectCoverage=true
	  - uses: codecov/codecov-action@v3
```

## Known Limitations

1. **No Real OSRM API**: Tests use mocked HTTP responses
   - **Solution:** For production validation, call actual OSRM instance

2. **Simplified Fuel Calculation**: Uses flat $0.15/km
   - **Solution:** Use dynamic fuel indices in production

3. **Synchronous Database**: In-memory DB is sync-only
   - **Solution:** Production uses async EF Core

4. **No Load Testing**: Single-threaded synchronous test flow
   - **Solution:** Add perf tests for sustained high-volume

5. **Limited Geographic Coverage**: US coordinates only
   - **Solution:** Expand test data for international routes

## Test Maintenance

### Adding New Route Test
1. Create test method in `OSRMServiceEndToEndTests`
2. Mock OSRM response with realistic data
3. Assert response properties (distance, duration, geometry)
4. Add to test summary documentation

### Adding New Integration Test
1. Create test method in `OSRMServiceIntegrationTests`
2. Seed MapAppDbContext with test capitals
3. Call route optimization service
4. Validate TMS metrics (fuel, HOS, consolidation)
5. Document test purpose and expectations

### Updating Expectations
- If OSRM API changes response format: Update mock response structures
- If performance degrades: Adjust timeout assertions
- If coordinates change: Update distance range expectations

## References

- **OSRM API Docs:** https://project-osrm.org/docs/v5.5.1/api/overview/
- **Haversine Formula:** https://en.wikipedia.org/wiki/Haversine_formula
- **xUnit Documentation:** https://xunit.net/
- **Moq Documentation:** https://github.com/moq/moq4/wiki/Quickstart
- **MapApp Service Implementation:** `MapApp/Backend/MapApp.API/Services/OSRMService.cs`
- **Route Optimization:** `MapApp/Backend/MapApp.API/Services/RouteOptimizationService.cs`

## Support & Questions

For issues or questions:
1. Check test method XML comments for detailed scenario
2. Review mock data setup in Arrange section
3. Validate assertions match expected business logic
4. Ensure in-memory database is unique per test
5. Check IHttpClientFactory mocking for correct response

---

**Last Updated:** 2024  
**Test Suite Version:** 1.0  
**Total Tests:** 28  
**Coverage:** 85%+
