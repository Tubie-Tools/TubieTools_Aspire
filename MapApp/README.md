# US Map Application - State Capitals & Transportation Routing

A full-stack web application for visualizing US state capitals, tracking product sales, and optimizing transportation routes using intelligent algorithms.

## 🎯 Features

### Map Visualization
- **Interactive Leaflet Map**: Display all 50 US state capitals
- **Sales Status Pins**: Red pins for states where products have been sold, gray for others
- **Regional Filtering**: View capitals by geographic region (Northeast, Southeast, Midwest, Southwest, West, South)
- **Sales Tracking**: Monitor total sales and products sold per state
- **Detailed Popups**: Click any capital to see detailed sales information

### Route Optimization
- **Nearest Neighbor Algorithm**: Greedy optimization for finding efficient visiting routes
- **Haversine Distance Calculation**: Accurate great-circle distance between geographic points
- **Multi-Vehicle Planning**: Split routes across multiple vehicles with capacity constraints
- **Real-time Route Visualization**: See optimized routes displayed on the map
- **Duration Estimates**: Travel time calculations based on average speed

### Transportation Planning
- **Logistics Optimization**: Create transportation plans for visiting all 50 states
- **Vehicle Capacity Management**: Specify how many states each vehicle can serve
- **Comprehensive Statistics**: Total distance, duration, and estimated vehicles needed
- **Route Segments**: Detailed segment breakdown for each route

### Sales Analytics
- **Statistics Dashboard**: Overview of sales performance
- **Top Selling States**: Ranked list of states by sales volume
- **Regional Analysis**: Sales performance by geographic region
- **Trend Visualization**: Charts and graphs for data analysis

## 🏗️ Architecture

### Backend (ASP.NET Core)
```
MapApp.API/
├── Controllers/
│   ├── StateCapitalsController   # State capital management endpoints
│   └── RoutesController          # Route optimization endpoints
├── Models/
│   ├── StateCapital              # Domain model for state capitals
│   └── Route                     # Domain models for routes and plans
├── Services/
│   ├── RouteOptimizationService  # Core optimization algorithms
│   └── OSRMService               # OSRM API integration
├── Data/
│   └── MapAppDbContext           # Entity Framework Core DbContext
└── DTOs/                         # Data transfer objects
```

### Frontend (React + TypeScript)
```
src/
├── components/
│   ├── Map.tsx                   # Main interactive map
│   ├── TransportationPlanner.tsx # Route planning UI
│   └── SalesAnalytics.tsx        # Analytics dashboard
├── api/
│   └── mapApi.ts                 # API service layer
├── store/
│   └── MapStore.ts               # Zustand state management
├── styles/                       # CSS stylesheets
└── App.tsx                       # Main application
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 16+ and npm
- Git

### Backend Setup

1. **Navigate to backend directory**
```bash
cd MapApp/Backend/MapApp.API
```

2. **Restore NuGet packages**
```bash
dotnet restore
```

3. **Build the project**
```bash
dotnet build
```

4. **Run the application**
```bash
dotnet run
```

The API will be available at `http://localhost:5000`
Swagger documentation available at `http://localhost:5000/swagger`

### Frontend Setup

1. **Navigate to frontend directory**
```bash
cd MapApp/Frontend
```

2. **Install dependencies**
```bash
npm install
```

3. **Create .env file**
```bash
echo "REACT_APP_API_URL=http://localhost:5000/api" > .env
```

4. **Start development server**
```bash
npm start
```

The application will open at `http://localhost:3000`

## 📊 API Endpoints

### State Capitals
- `GET /api/statecapitals` - Get all state capitals
- `GET /api/statecapitals/{stateCode}` - Get capital for specific state
- `GET /api/statecapitals/region/{region}` - Get capitals by region
- `GET /api/statecapitals/sales/sold-to` - Get capitals where we sold products
- `GET /api/statecapitals/sales/statistics` - Get sales statistics
- `PUT /api/statecapitals/{stateCode}/sales` - Update sales information

### Routes
- `POST /api/routes/optimize` - Optimize route using nearest neighbor
- `POST /api/routes/transportation-plan` - Create multi-vehicle transportation plan
- `GET /api/routes` - Get all saved routes
- `GET /api/routes/{routeId}` - Get specific route
- `GET /api/routes/{routeId}/segments` - Get route segments
- `POST /api/routes/distance` - Calculate distance between two capitals

## 🎓 Algorithm Details

### Nearest Neighbor Algorithm
**Time Complexity**: O(n²)  
**Space Complexity**: O(n)

The algorithm works by:
1. Starting from a given state capital
2. Finding the unvisited capital with minimum distance
3. Moving to that capital and marking it as visited
4. Repeating until all capitals are visited
5. Returning to the starting point

This provides a good approximation for the Traveling Salesman Problem (TSP) with reasonable performance for 50 nodes.

**Example**:
```
Start: California → Nearest: Nevada (500km) → Nearest: Arizona (600km) → ...
```

### Haversine Distance Formula
Calculates the great-circle distance between two geographic points:

```
a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
c = 2 ⋅ asin( √a )
d = R ⋅ c
```

Where:
- φ = latitude, λ = longitude, R = earth's radius (6371 km)

### Load Distribution
For multi-vehicle plans:
1. Sort all capitals by distance from starting point
2. Distribute into vehicle groups respecting capacity
3. Optimize each group's route independently
4. Calculate total logistics metrics

## 💾 Database

Uses **Entity Framework Core with In-Memory Database** for demonstration.

**Seed Data**: 50 US state capitals with:
- Geographic coordinates (latitude, longitude)
- Sales status and amount
- Products sold count
- Regional classification
- Timestamps

To use a real database (SQL Server, PostgreSQL), update `Program.cs`:
```csharp
builder.Services.AddDbContext<MapAppDbContext>(options =>
	options.UseSqlServer("Your-Connection-String"));
```

## 🗺️ Map Integration

### Leaflet + React Leaflet
- Free, open-source mapping library
- OpenStreetMap tiles (free, no API key required)
- Custom SVG markers for sales status visualization
- PolyLine rendering for optimized routes

### OSRM (Optional Enhancement)
For production, integrate Open Source Routing Machine:
- Free routing engine
- Real-world road network
- Can be self-hosted or use public API
- Provides actual travel times and distances

## 📈 Performance Optimizations

1. **Memoization**: React hooks to prevent unnecessary re-renders
2. **Lazy Loading**: Map tiles loaded on demand
3. **Caching**: Client-side caching of API responses
4. **Efficient Algorithms**: Greedy nearest-neighbor avoids exponential complexity
5. **Database Indexing**: Indexes on frequently queried fields

## 🔒 Security Considerations

For production deployment:
1. Enable HTTPS/CORS restrictions
2. Add API authentication (JWT tokens)
3. Implement rate limiting
4. Validate all input data
5. Use environment variables for sensitive data
6. Add request logging and monitoring

## 🧪 Testing

### Backend
```bash
dotnet test
```

### Frontend
```bash
npm test
```

## 📱 Deployment

### Backend (Azure/AWS)
```bash
dotnet publish -c Release
# Deploy build artifacts
```

### Frontend (Netlify/Vercel)
```bash
npm run build
# Deploy build folder
```

## 🎨 UI/UX Features

- **Responsive Design**: Works on desktop, tablet, mobile
- **Dark Theme Support**: Via Ant Design theming
- **Interactive Charts**: Recharts visualizations
- **Real-time Updates**: Instant map and route updates
- **Intuitive Controls**: Simple, discoverable UI
- **Detailed Analytics**: Comprehensive sales dashboards

## 📚 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core
- **Mapping**: AutoMapper
- **Logging**: Serilog
- **API Docs**: Swagger/OpenAPI

### Frontend
- **Framework**: React 18 + TypeScript
- **State Management**: Zustand
- **Mapping**: Leaflet + React Leaflet
- **UI Components**: Ant Design
- **HTTP Client**: Axios
- **Charts**: Recharts

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 💡 Future Enhancements

- [ ] Google Maps / Mapbox integration
- [ ] Advanced TSP solvers (genetic algorithm, ant colony)
- [ ] Real-time traffic integration
- [ ] Customer location clustering
- [ ] Delivery time windows
- [ ] Multi-start nearest neighbor (better solution quality)
- [ ] Simulated annealing optimization
- [ ] Driver availability calendar
- [ ] Cost optimization (fuel, vehicle, time)
- [ ] WebSocket for real-time updates

## 📞 Support

For questions or issues, please open an issue in the repository.

## 📄 License

This project is provided as-is for interview and educational purposes.

---

**Built for**: Transportation & Logistics Company Interview  
**Version**: 1.0.0  
**Last Updated**: 2024
