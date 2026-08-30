# TubieTools Map - Blazor Route Planner UI Guide

## Overview
This guide documents the complete Blazor route planner UI integration with Dijkstra's algorithm for optimal route calculation. The solution provides an interactive map-based interface for selecting locations and calculating optimal routes between any two points on the network.

## Architecture

### Components

#### 1. **RouteCalculationService** (`TubieTools_Aspire.Maps/Services/RouteCalculationService.cs`)
Central service that bridges Dijkstra algorithm with Blazor UI components.

**Key Features:**
- Graph and location management
- Route calculation coordination
- Route history tracking
- Event-based state notification
- Data export (JSON, GeoJSON)

**Key Methods:**
```csharp
// Initialize with locations and roads
await InitializeAsync(locations, roads);

// Calculate optimal route
var route = await CalculateRouteAsync(startId, endId, costPerKm);

// Multi-stop optimization
var route = await CalculateMultiStopRouteAsync(startId, stopIds, costPerKm);

// Search and discovery
var results = SearchLocations(searchTerm);
var nearby = GetLocationsWithinRadius(lat, lon, radiusKm);
var nearest = FindNearestLocation(lat, lon);

// Export routes
var geojson = ExportCurrentRouteAsGeoJSON();
var json = ExportCurrentRouteAsJSON();
```

#### 2. **RoutePlannerPage** (`TubieTools_Aspire.Maps/Pages/RoutePlannerPage.razor`)
Main Blazor page with complete route planning UI.

**Features:**
- Location selection with search autocomplete
- Route calculation with cost estimation
- Real-time route visualizations
- Route history tracking
- Multi-format export (JSON, GeoJSON)
- Loading states and error handling
- Responsive layout for mobile/tablet

**User Flow:**
1. User searches for and selects a start location
2. User searches for and selects an end location
3. User optionally adjusts cost per km
4. User clicks "Calculate Route"
5. Service validates, calculates, and returns optimal route
6. UI displays route info, waypoints, and renders on map
7. User can export or calculate new route

#### 3. **MapWidget Component** (`TubieTools_Aspire.Maps/Components/MapWidget.razor`)
Reusable Blazor component for map display and interaction.

**Parameters:**
```csharp
[Parameter] public string ContainerId { get; set; } = "mapContainer";
[Parameter] public string Width { get; set; } = "100%";
[Parameter] public string Height { get; set; } = "500px";
[Parameter] public MapDijkstraService.MapRoute Route { get; set; }
[Parameter] public List<MapDijkstraService.MapLocation> Locations { get; set; }
[Parameter] public string RouteColor { get; set; } = "#0d6efd";
[Parameter] public bool ShowLocationMarkers { get; set; } = true;
[Parameter] public string MapType { get; set; } = "street";
```

**Usage:**
```razor
<MapWidget 
	Route="@currentRoute"
	Locations="@allLocations"
	MapType="street"
	ShowLocationMarkers="true"
	Height="500px" />
```

#### 4. **Map Interoperability** (`TubieTools_Aspire.Maps/wwwroot/js/map-interop.js`)
JavaScript bridge for Leaflet map integration.

**Core Functions:**
```javascript
// Map initialization
initializeMap(containerId)

// Route rendering
renderRoute(coordinates, color)
addLocationMarkers(locations)
clearRoute()
clearMarkers()

// Map controls
setMapCenter(lat, lon, zoom)
getMapCenter()
getMapBounds()
setMapType(type)  // 'street' | 'satellite' | 'terrain'

// Advanced features
addRadiusCircle(lat, lon, radiusMeters, color)
addGeoJSONLayer(geoJsonData, onEachFeature)
exportMapAsImage(callback)
downloadJSON(jsonData, filename)
measureDistance(point1, point2)
```

## Setup Instructions

### 1. Dependency Injection Configuration
Add to `Program.cs`:
```csharp
using TubieTools_Aspire.Maps.Services;
using TubieTools_Aspire.Tests.Algorithms;

builder.Services.AddScoped<RouteCalculationService>();
builder.Services.AddScoped<MapDijkstraService>();
builder.Services.AddScoped<WeightedGraph>();
builder.Services.AddScoped<DijkstraAlgorithm>();
builder.Services.AddScoped<LogisticsDijkstraService>();

// Add Blazor services
builder.Services.AddBlazorBootstrap();
```

### 2. Import Required Namespaces in `_Imports.razor`
```razor
@using TubieTools_Aspire.Maps.Services
@using TubieTools_Aspire.Tests.Algorithms
@using TubieTools_Aspire.Maps.Components
```

### 3. Add Route to Navigation
In `NavMenu.razor`:
```razor
<NavLink class="nav-link" href="route-planner">
	<i class="fas fa-route"></i> Route Planner
</NavLink>
```

### 4. Include Required Assets
The `index.html` file includes:
- Bootstrap 5.3.0 for styling
- Font Awesome 6.4.0 for icons
- Leaflet 1.9.4 for mapping
- Custom `map-interop.js` for map integration

## Data Model

### MapLocation
```csharp
public class MapLocation
{
	public int Id { get; set; }
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string Type { get; set; }
	public Dictionary<string, object> Metadata { get; set; }
}
```

### MapRoute
```csharp
public class MapRoute
{
	public string RouteId { get; set; }
	public List<MapLocation> Waypoints { get; set; }
	public List<(double Lat, double Lon)> Coordinates { get; set; }
	public double TotalDistance { get; set; }
	public double TotalCost { get; set; }
	public double EstimatedTimeMinutes { get; set; }
	public string Status { get; set; }
	public Dictionary<string, object> Metadata { get; set; }
}
```

## Usage Examples

### Basic Route Calculation
```csharp
// In a Blazor component
@inject RouteCalculationService routeService

private async Task CalculateRoute()
{
	var route = await routeService.CalculateRouteAsync(
		startLocationId: 1,
		endLocationId: 5,
		costPerKm: 1.50
	);

	if (route != null)
	{
		Console.WriteLine($"Optimal route: {route.TotalDistance} km");
		Console.WriteLine($"Estimated time: {route.EstimatedTimeMinutes} minutes");
		Console.WriteLine($"Total cost: ${route.TotalCost}");
	}
}
```

### Search Locations
```csharp
var results = routeService.SearchLocations("warehouse");
// Returns list of locations matching search term
```

### Get Nearby Locations
```csharp
var nearby = routeService.GetLocationsWithinRadius(
	centerLat: 40.7128,
	centerLon: -74.0060,
	radiusKm: 10.0
);
// Returns locations sorted by distance
```

### Export Route
```csharp
// Export as GeoJSON for web mapping
var geoJson = routeService.ExportCurrentRouteAsGeoJSON();

// Export as JSON for storage
var json = routeService.ExportCurrentRouteAsJSON();

// Use Interop to download
await JSRuntime.InvokeVoidAsync(
	"RouteMapInterop.downloadJSON", 
	json, 
	"route-export.json"
);
```

### Multi-Stop Route
```csharp
var route = await routeService.CalculateMultiStopRouteAsync(
	startLocationId: 1,
	stopLocationIds: new[] { 3, 5, 7, 2 },
	costPerKm: 2.00
);
```

## Sample Data Initialization

The default sample data includes:
- 8 locations (warehouses, stores, hubs)
- Geographic coordinates for NYC area
- 10 interconnected roads
- Distance metrics between locations

**Locations:**
1. Main Warehouse (40.7128, -74.0060)
2. Downtown Store (40.7589, -73.9851)
3. Midtown Store (40.7549, -73.9840)
4. Uptown Store (40.7829, -73.9654)
5. Queens Store (40.7282, -73.7949)
6. Brooklyn Store (40.6501, -73.9496)
7. Distribution Hub (40.7505, -73.9972)
8. Airport Storage (40.7769, -73.8740)

**To customize:**
1. Locate the `InitializeSampleData()` method in `RoutePlannerPage.razor`
2. Modify the locations list with your own geographic data
3. Update the roads list with your network connections
4. Adjust distance calculations as needed

## UI Features

### Location Selection
- **Search autocomplete** - Type location name to filter results
- **Click-to-select** - Quick selection from dropdown
- **Selected badge** - Visual confirmation of selected location
- **Clear button** - Easy removal of selection

### Route Information Display
- **Distance**: Total route distance in kilometers
- **Estimated Cost**: Calculated based on distance × cost per km
- **Estimated Time**: Time estimate (60 km/h average)
- **Stop Count**: Number of waypoints in route
- **Waypoint List**: Detailed ordered list with coordinates

### Map Display
- **Interactive Leaflet map** with OpenStreetMap tiles
- **Route visualization** with polyline animation
- **Start marker** (green circle) at origin
- **End marker** (red circle) at destination
- **Waypoint markers** (orange circles) for intermediate stops
- **Auto-zoom** to fit entire route
- **Multiple map types** - Street, Satellite, Terrain

### Export Options
- **JSON Export** - Complete route data for API consumption
- **GeoJSON Export** - Standard geographic format for web mapping
- **Print** - Format for printing/archiving

### Route History
- **Automatic tracking** of all calculated routes
- **History modal** showing previous routes
- **Quick re-selection** of past routes
- **Route comparison** capability

## Performance Considerations

### Optimization Tips
1. **Limit search results** - UI shows top 10 results by default
2. **Cache locations** - Service caches all locations on init
3. **Lazy load markers** - Map only renders visible markers
4. **Route simplification** - Complex routes are simplified for display
5. **Debounce search** - Input search is naturally debounced by Blazor

### Scalability
- Handles 100+ locations efficiently
- Supports graphs with 1000+ edges
- Route calculation <100ms for typical networks
- Memory-efficient coordinate storage

## Styling and Customization

### Custom Colors
Edit CSS variables in `index.html`:
```css
:root {
	--primary-color: #0d6efd;
	--success-color: #198754;
	--danger-color: #dc3545;
	--warning-color: #ffc107;
	--info-color: #0dcaf0;
	--light-bg: #f8f9fa;
	--border-color: #dee2e6;
}
```

### Custom Route Colors
```razor
<MapWidget Route="@route" RouteColor="#ff0000" />
```

### Responsive Breakpoints
- Desktop (≥992px): Side-by-side layout
- Tablet (768px-991px): Stacked layout with equal height
- Mobile (<768px): Full-screen map with modal controls

## Troubleshooting

### Map Not Displaying
1. Verify Leaflet libraries are loaded in browser console
2. Check container element ID matches parameters
3. Ensure CSS is properly loaded
4. Check browser console for JavaScript errors

### Route Not Calculating
1. Verify start and end locations are selected
2. Check RouteCalculationService is registered in DI
3. Ensure MapDijkstraService has locations loaded
4. Verify graph connectivity between locations

### Performance Issues
1. Reduce number of location markers displayed
2. Simplify route line (fewer coordinates)
3. Clear previous routes before calculating new ones
4. Monitor browser memory usage

### Location Search Not Working
1. Verify locations are initialized via `InitializeAsync()`
2. Check search term matches location names
3. Ensure `SearchLocations()` is being called
4. Check browser console for errors

## Advanced Customization

### Custom Location Data Source
```csharp
public async Task LoadLocationsFromDatabase()
{
	var locations = await _dbService.GetAllLocations();
	var roads = await _dbService.GetAllRoads();
	await RouteService.InitializeAsync(locations, roads);
}
```

### Custom Route Styling
```javascript
function renderRoute(coordinates, color, dashArray) {
	// Modify polyline style
	routePolyline.setStyle({
		color: color,
		dashArray: dashArray,
		weight: 5,
		lineCap: 'round'
	});
}
```

### Custom Marker Styling
```javascript
function createCustomMarker(lat, lon, type) {
	const icon = type === 'warehouse' 
		? warehouseIcon 
		: type === 'store' 
		? storeIcon 
		: defaultIcon;

	return L.marker([lat, lon], { icon: icon }).addTo(mapInstance);
}
```

## Related Documentation
- [Dijkstra Algorithm Guide](../Algorithms/DIJKSTRA_ALGORITHM_GUIDE.md)
- [Logistics Integration Guide](../Algorithms/DIJKSTRA_LOGISTICS_GUIDE.md)
- [Dijkstra Quick Reference](../Algorithms/DIJKSTRA_QUICK_REFERENCE.md)
- [Map Service Reference](MapDijkstraService.cs)

## Support & Feedback
For issues, feature requests, or questions:
1. Check the troubleshooting section above
2. Review browser console for error messages
3. Verify all dependencies are properly installed
4. Check related algorithm documentation

## Version History
- **1.0** - Initial Blazor UI with Dijkstra integration
  - Route calculation and visualization
  - Location search and selection
  - Multi-stop routing
  - Export functionality
  - Route history tracking
