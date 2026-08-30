# TubieTools Map - Complete File Manifest & Dependencies

## Project Structure Overview

```
TubieTools_Aspire.Maps/
│
├── 📁 Services/
│   └── RouteCalculationService.cs ⭐
│       └── Injected into: RoutePlannerPage.razor, QuickRoutePlannerTemplate.razor
│
├── 📁 Pages/
│   ├── RoutePlannerPage.razor ⭐⭐⭐ (Main Application Page)
│   │   ├── Uses: RouteCalculationService
│   │   ├── Uses: IJSRuntime (for map interop)
│   │   ├── References: MapWidget component
│   │   └── Navigable at: /route-planner
│   │
│   └── QuickRoutePlannerTemplate.razor ⭐⭐ (Alternative Simple Page)
│       ├── Uses: RouteCalculationService
│       ├── Uses: IJSRuntime
│       └── Navigable at: /quick-route-planner
│
├── 📁 Components/
│   └── MapWidget.razor
│       ├── Uses: MapDijkstraService
│       ├── Uses: IJSRuntime
│       └── Reusable: Yes (can be used in other pages)
│
├── 📁 wwwroot/
│   ├── 📄 index.html
│   │   ├── Includes: Bootstrap 5.3.0
│   │   ├── Includes: Font Awesome 6.4.0
│   │   ├── Includes: Leaflet 1.9.4
│   │   ├── Includes: Custom CSS
│   │   └── Includes: Blazor framework script
│   │
│   └── 📁 js/
│       └── map-interop.js
│           ├── Dependencies: Leaflet library
│           ├── Exported: window.RouteMapInterop
│           └── Called from: MapWidget.razor, RoutePlannerPage.razor
│
├── 📁 External Dependencies (From Tests Project)
│   ├── WeightedGraph.cs (Reference or Copy)
│   ├── DijkstraAlgorithm.cs (Reference or Copy)
│   ├── MapDijkstraService.cs (Reference or Copy)
│   └── LogisticsDijkstraService.cs (Reference or Copy)
│
├── 📄 Program.cs (Main Application Configuration)
│   └── See: Program.cs.example for reference implementation
│
├── 📄 _Imports.razor (Global Using Statements)
│   └── Must include: TubieTools_Aspire.Maps.Services, TubieTools_Aspire.Tests.Algorithms
│
├── 📄 App.razor (Root Component)
│   └── Maps routes to pages defined above
│
└── 📁 Documentation/
	├── 📖 README_COMPLETE_SOLUTION.md ⭐
	│   └── Start here: Project overview and quick start
	│
	├── 📖 BLAZOR_ROUTE_PLANNER_GUIDE.md ⭐⭐
	│   └── Detailed: API reference, usage examples, troubleshooting
	│
	├── 📖 IMPLEMENTATION_SUMMARY.md ⭐⭐
	│   └── Architecture: Data flow, components, features, deployment
	│
	├── 📖 INTEGRATION_CHECKLIST.md ⭐⭐⭐
	│   └── Setup guide: 20-point checklist for integration
	│
	├── 📖 Program.cs.example
	│   └── Reference: Dependency injection configuration patterns
	│
	└── 📖 This File (FILE_MANIFEST.md)
		└── Reference: File structure and dependencies
```

---

## File Dependencies Map

### RouteCalculationService.cs
**Dependencies**:
- `TubieTools_Aspire.Tests.Algorithms.WeightedGraph`
- `TubieTools_Aspire.Tests.Algorithms.MapDijkstraService`
- `TubieTools_Aspire.Tests.Algorithms.LogisticsDijkstraService`
- `System.Collections.Generic`
- `System.Linq`
- `System.Threading.Tasks`

**Used By**:
- RoutePlannerPage.razor
- QuickRoutePlannerTemplate.razor
- (Any component requiring route calculation)

**Public Interface**:
```csharp
// Initialization
Task InitializeAsync(List<(int, string, double, double)> locations, 
					List<(int, int, double, string)> roads)

// Route Calculation
Task<MapRoute> CalculateRouteAsync(int start, int end, double costPerKm)
Task<MapRoute> CalculateMultiStopRouteAsync(int start, int[] stops, double costPerKm)

// Location Management
List<MapLocation> SearchLocations(string term)
List<MapLocation> GetAllLocations()
List<MapLocation> GetLocationsWithinRadius(double lat, double lon, double radius)
MapLocation FindNearestLocation(double lat, double lon)

// Data Access
MapRoute GetCurrentRoute()
List<MapRoute> GetRouteHistory()

// Data Export
Dictionary<string, object> ExportCurrentRouteAsJSON()
Dictionary<string, object> ExportCurrentRouteAsGeoJSON()
Dictionary<string, object> GetRouteSummary()

// State & Events
bool IsLoading { get; }
string ErrorMessage { get; }
string SuccessMessage { get; }
event Action OnRouteUpdated
event Action OnLocationsUpdated
event Action OnLoadingChanged
```

### RoutePlannerPage.razor
**Dependencies**:
- `TubieTools_Aspire.Maps.Services.RouteCalculationService` (injected)
- `Microsoft.JSInterop.IJSRuntime` (injected)
- `MapWidget` component (local)
- `map-interop.js` (via JSRuntime)

**Features**:
- Split-view layout with controls & map
- Location search with autocomplete
- Route calculation & visualization
- Route information display
- Waypoint listing
- Export options
- Route history modal

**Navigable URL**: `/route-planner`

**Sample Data**: 8 locations + 10 roads (NYC area)

### QuickRoutePlannerTemplate.razor
**Dependencies**:
- `TubieTools_Aspire.Maps.Services.RouteCalculationService` (injected)
- `Microsoft.JSInterop.IJSRuntime` (injected)

**Features**:
- Simplified UI with dropdowns
- Quick route calculation
- Inline result display
- Route history table
- Re-usable routes

**Navigable URL**: `/quick-route-planner`

**Minimal & Template-Focused**: Good for learning or quick implementation

### MapWidget.razor
**Dependencies**:
- `TubieTools_Aspire.Tests.Algorithms.MapDijkstraService.MapRoute`
- `TubieTools_Aspire.Tests.Algorithms.MapDijkstraService.MapLocation`
- `Microsoft.JSInterop.IJSRuntime`
- `map-interop.js` (via JSRuntime)

**Parameters**:
- `ContainerId` (string, default: "mapContainer")
- `Width` (string, default: "100%")
- `Height` (string, default: "500px")
- `Style` (string, optional)
- `Route` (MapRoute, optional)
- `Locations` (List<MapLocation>, optional)
- `RouteColor` (string, default: "#0d6efd")
- `ShowLocationMarkers` (bool, default: true)
- `MapType` (string, default: "street")
- `OnMapClicked` (EventCallback)

**Methods**:
```csharp
Task ClearMap()
Task SetCenter(double lat, double lon, int zoom)
Task AddRadiusCircle(double lat, double lon, double radiusMeters, string color)
Task ExportAsImage()
```

**Reusable**: Can be embedded in any page

### map-interop.js
**Dependencies**:
- Leaflet 1.9.4 (L object)
- No external dependencies beyond Leaflet

**Global Export**:
```javascript
window.RouteMapInterop = {
	initializeMap,
	renderRoute,
	addLocationMarkers,
	clearMarkers,
	clearRoute,
	getMapCenter,
	setMapCenter,
	getMapBounds,
	addRadiusCircle,
	downloadJSON,
	exportMapAsImage,
	setMapType,
	measureDistance,
	addGeoJSONLayer,
	getRouteAsGeoJSON
}
```

**Called From**:
- MapWidget.razor (via JSRuntime)
- RoutePlannerPage.razor (via JSRuntime)
- QuickRoutePlannerTemplate.razor (via JSRuntime)

### index.html
**Includes**:
- Bootstrap 5.3.0 CSS
- Font Awesome 6.4.0 CSS
- Leaflet 1.9.4 CSS & JS
- Custom CSS (embedded)
- Blazor Framework JS
- map-interop.js

**Structure**:
- Meta tags for responsiveness
- CSS variables for theming
- Responsive grid system
- Print styles
- Loading spinner styles
- Animation styles

### Program.cs
**Must Configure**:
```csharp
// Add these services:
builder.Services.AddScoped<WeightedGraph>();
builder.Services.AddScoped<DijkstraAlgorithm>();
builder.Services.AddScoped<LogisticsDijkstraService>();
builder.Services.AddScoped<MapDijkstraService>();
builder.Services.AddScoped<RouteCalculationService>();

// Add Razor components:
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Add HTTP client:
builder.Services.AddHttpClient();
```

**Optional Configurations**:
- Authentication
- Authorization
- CORS
- Session state
- Logging
- Database
- Custom initialization

### _Imports.razor
**Must Include**:
```razor
@using TubieTools_Aspire.Maps.Services
@using TubieTools_Aspire.Tests.Algorithms
@using TubieTools_Aspire.Maps.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
```

---

## External Algorithm Files

### WeightedGraph.cs (From Tests Project)
**Purpose**: Graph data structure foundation
**Key Classes**:
- `GraphVertex` - Node with ID, label, coordinates, metadata
- `WeightedEdge` - Connection between vertices
- `WeightedGraph` - Graph container

**Usage in Solution**:
- Holds location and road data
- Passed to DijkstraAlgorithm
- Referenced by MapDijkstraService

### DijkstraAlgorithm.cs (From Tests Project)
**Purpose**: Shortest path computation
**Key Methods**:
- `FindShortestPath(start, end)` - Compute optimal route
- `FindShortestPathsToMultipleDestinations(start, ends)` - Multi-destination
- `ComputeShortestPathTree(start)` - Complete tree
- `FindPathsUpToDistance(start, maxDistance)` - Bounded search
- `ComputeAllPairsShortestPaths()` - Floyd-Warshall

**Usage in Solution**:
- Core algorithm for route calculation
- Called by MapDijkstraService
- Cached by RouteCalculationService

### MapDijkstraService.cs (From Tests Project)
**Purpose**: Geographic adapter for routing
**Key Classes**:
- `MapLocation` - Location with coordinates
- `MapRoute` - Route with waypoints and metadata

**Key Methods**:
- `AddLocation(id, name, lat, lon)` - Register location
- `AddRoad(from, to, distance)` - Register connection
- `FindOptimalRoute(start, end, costPerKm)` - Calculate route
- `FindMultiStopRoute(start, stops, costPerKm)` - Optimize multi-stop
- `ExportAsGeoJSON(route)` - Convert to standard format
- `ExportAsPolyline(route)` - Encode for maps API
- `GetLocationsWithinRadius(lat, lon, radius)` - Spatial query
- `FindNearestLocation(lat, lon)` - Proximity search

**Usage in Solution**:
- Provides geographic abstractions
- Used by RouteCalculationService
- Data fed to map visualization

### LogisticsDijkstraService.cs (From Tests Project)
**Purpose**: Logistics-specific routing
**Key Methods**:
- `FindDeliveryRoute(start, stops)` - Delivery optimization
- `FindOptimalMultiStopRoute(start, stops, costPerKm)` - Multi-stop
- `ClusterDeliveriesIntoRoutes(stops, maxPerRoute)` - Batching
- `CalculateRouteCost(route, costPerKm)` - Cost computation
- `ValidateRoute(route)` - Route verification

**Usage in Solution**:
- Supports multi-stop route optimization
- Called by RouteCalculationService.CalculateMultiStopRouteAsync()
- Used for delivery use cases

---

## Data Flow

### Simple Route Calculation Flow
```
RoutePlannerPage.razor
  ↓ (User clicks Calculate)
RouteCalculationService.CalculateRouteAsync()
  ↓
MapDijkstraService.FindOptimalRoute()
  ↓
DijkstraAlgorithm.FindShortestPath()
  ↓ (Returns path)
MapRoute object created with waypoints
  ↓
RoutePlannerPage displays:
  - Route information
  - Waypoints list
  - Passes to MapWidget
	↓
	MapWidget calls map-interop.js
	  ↓
	  renderRoute(coordinates, color)
		↓
		Leaflet renders polyline + markers
```

### Multi-Stop Calculation Flow
```
User selects multiple stops
  ↓
RouteCalculationService.CalculateMultiStopRouteAsync()
  ↓
LogisticsDijkstraService.FindOptimalMultiStopRoute()
  ↓
[Calculate optimal order using Dijkstra]
  ↓
MapRoute with ordered waypoints
  ↓
Display and visualize
```

### Location Search Flow
```
User types in search box
  ↓
SearchLocations(searchTerm)
  ↓
MapDijkstraService.SearchLocations()
  ↓
Filter cached locations
  ↓
Return matching results
  ↓
RoutePlannerPage displays dropdown
  ↓
User selects location
  ↓
Update selectedStartLocation
```

---

## Library Dependencies

### Runtime Libraries
| Library | Version | Purpose | Source |
|---------|---------|---------|--------|
| Bootstrap | 5.3.0 | CSS Framework | CDN |
| Font Awesome | 6.4.0 | Icons | CDN |
| Leaflet | 1.9.4 | Map Library | CDN |
| Leaflet Image | 0.5.0 | Map Export | CDN |
| OpenStreetMap | Latest | Map Tiles | CDN |
| Blazor Web | Latest | Framework | NuGet |

### NuGet Packages Required
```xml
<!-- Implicit via .NET -->
- Microsoft.AspNetCore.Components
- Microsoft.AspNetCore.Components.Web
- Microsoft.JSInterop
- System.Collections.Generic
- System.Linq
- System.Threading.Tasks
```

---

## Configuration Files

### appsettings.json (Standard ASP.NET Core)
Standard configuration—no special entries required for route planner unless adding:
- Database connection strings
- Third-party API keys
- Custom settings

### launchSettings.json (Debug Configuration)
Defines development server URLs and profiles.

---

## Asset Files

### CSS Files
- `index.html` - Contains complete inline CSS
- Bootstrap 5.3.0 CDN - Framework styles
- Font Awesome CDN - Icon styles
- Custom CSS variables in `:root`

### JavaScript Files
- `wwwroot/js/map-interop.js` - Custom map bridge (~400 lines)
- Blazor framework (`_framework/blazor.web.js`)
- Bootstrap JS (for modals, dropdowns, etc.)
- Leaflet JS (for mapping)

### Image Assets
- Font Awesome icons (loaded from CDN)
- OpenStreetMap tiles (loaded from CDN)
- No custom images required

---

## Documentation Files

All documentation files are included in the project root:

1. **README_COMPLETE_SOLUTION.md** ⭐ START HERE
   - Project overview
   - Quick start guide
   - Feature summary
   - Next steps

2. **BLAZOR_ROUTE_PLANNER_GUIDE.md**
   - Detailed API reference
   - Component documentation
   - Setup instructions
   - Usage examples
   - Troubleshooting

3. **IMPLEMENTATION_SUMMARY.md**
   - Architecture overview
   - Deliverables breakdown
   - Data flow diagrams
   - Performance info
   - File structure

4. **INTEGRATION_CHECKLIST.md**
   - Step-by-step setup
   - 20-point verification checklist
   - Testing procedures
   - Production readiness

5. **Program.cs.example**
   - Dependency injection setup
   - Configuration patterns
   - Advanced examples

6. **FILE_MANIFEST.md** (This File)
   - Complete file reference
   - Dependency map
   - Data flow documentation

---

## Quick Reference: What Each File Does

| File | Size | Purpose | Critical? |
|------|------|---------|-----------|
| RouteCalculationService.cs | 350 | Route orchestration | YES |
| RoutePlannerPage.razor | 520 | Main UI | YES |
| QuickRoutePlannerTemplate.razor | 280 | Alternate UI | NO |
| MapWidget.razor | 100 | Map component | NO |
| map-interop.js | 400 | JS bridge | YES |
| index.html | 450 | HTML layout | YES |
| Program.cs.example | 150 | Config reference | YES |
| Documentation | 2000+ | Guides & reference | YES |

---

## Deployment Checklist

Before deploying to production:

- [ ] All files copied to correct locations
- [ ] Program.cs configured with all services
- [ ] _Imports.razor updated with using statements
- [ ] NavMenu.razor updated with route links
- [ ] All external libraries available (CDNs accessible)
- [ ] Custom CSS loads correctly
- [ ] JavaScript interop works
- [ ] Map displays and functions
- [ ] Route calculation executes
- [ ] No console errors in browser
- [ ] Responsive design tested on mobile/tablet
- [ ] Performance acceptable
- [ ] Security review completed
- [ ] Documentation available to team
- [ ] Error handling tested
- [ ] Production environment variables set

---

## Support & Troubleshooting Reference

If files are missing, check:
1. All files from "Complete File List" section above
2. External algorithms copied from Tests project
3. Folder structure matches recommendations
4. Using statements in _Imports.razor correct
5. Program.cs configured correctly

If functionality doesn't work:
1. Check browser console (F12) for errors
2. Verify all external libraries loaded (Network tab)
3. Ensure JSRuntime calls reference correct function names
4. Check RouteCalculationService is registered in DI
5. Verify map container div exists in HTML

---

## Summary

This manifest documents:

✅ Complete file structure
✅ All dependencies and relationships
✅ Data flow architecture
✅ External algorithm usage
✅ Configuration requirements
✅ Library dependencies
✅ Documentation structure
✅ Deployment preparation

**For step-by-step integration, refer to INTEGRATION_CHECKLIST.md**

**For detailed information, refer to BLAZOR_ROUTE_PLANNER_GUIDE.md**
