using MapApp.API.Models;
using MapApp.API.Models.TMS;
using Microsoft.EntityFrameworkCore;

namespace MapApp.API.Data;

public class MapAppDbContext : DbContext
{
    public MapAppDbContext(DbContextOptions<MapAppDbContext> options) : base(options)
    {
    }

    public DbSet<StateCapital> StateCapitals { get; set; } = null!;
    public DbSet<RouteSegment> RouteSegments { get; set; } = null!;
    public DbSet<OptimizedRoute> OptimizedRoutes { get; set; } = null!;
    public DbSet<TransportationPlan> TransportationPlans { get; set; } = null!;

    // TMS Entities (Schneider International)
    public DbSet<Shipment> Shipments { get; set; } = null!;
    public DbSet<ShipmentEvent> ShipmentEvents { get; set; } = null!;
    public DbSet<BillingRecord> BillingRecords { get; set; } = null!;
    public DbSet<Truck> Trucks { get; set; } = null!;
    public DbSet<Driver> Drivers { get; set; } = null!;
    public DbSet<RouteFactor> RouteFactors { get; set; } = null!;
    public DbSet<FuelMetrics> FuelMetrics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // State capitals configuration
        modelBuilder.Entity<StateCapital>()
            .HasKey(s => s.StateCode);

        modelBuilder.Entity<StateCapital>()
            .HasIndex(s => s.HasSoldProducts);

        modelBuilder.Entity<StateCapital>()
            .HasIndex(s => s.Region);

        // Route segments configuration
        modelBuilder.Entity<RouteSegment>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<RouteSegment>()
            .HasIndex(r => new { r.FromState, r.ToState })
            .IsUnique();

        // Optimized routes configuration
        modelBuilder.Entity<OptimizedRoute>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<OptimizedRoute>()
            .HasMany(r => r.RouteSegments)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        // Transportation plans configuration
        modelBuilder.Entity<TransportationPlan>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<TransportationPlan>()
            .HasMany(t => t.Routes)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        // ========================================
        // TMS Entity Configurations
        // ========================================

        // Shipment Configuration
        modelBuilder.Entity<Shipment>()
            .HasKey(s => s.ShipmentId);

        modelBuilder.Entity<Shipment>()
            .HasMany<ShipmentEvent>()
            .WithOne()
            .HasForeignKey("ShipmentId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Shipment>()
            .HasIndex(s => s.Status);

        modelBuilder.Entity<Shipment>()
            .HasIndex(s => new { s.OriginState, s.DestinationState });

        modelBuilder.Entity<Shipment>()
            .HasIndex(s => s.BillingStatus);

        // ShipmentEvent Configuration
        modelBuilder.Entity<ShipmentEvent>()
            .HasKey(e => e.EventId);

        modelBuilder.Entity<ShipmentEvent>()
            .HasIndex(e => e.EventType);

        // Truck Configuration
        modelBuilder.Entity<Truck>()
            .HasKey(t => t.TruckId);

        modelBuilder.Entity<Truck>()
            .HasIndex(t => t.Status);

        // Driver Configuration
        modelBuilder.Entity<Driver>()
            .HasKey(d => d.DriverId);

        modelBuilder.Entity<Driver>()
            .HasIndex(d => d.Status);

        // RouteFactor Configuration
        modelBuilder.Entity<RouteFactor>()
            .HasKey(f => f.FactorId);

        modelBuilder.Entity<RouteFactor>()
            .HasIndex(f => new { f.AffectedState, f.FactorType });

        // BillingRecord Configuration
        modelBuilder.Entity<BillingRecord>()
            .HasKey(b => b.BillingRecordId);

        modelBuilder.Entity<BillingRecord>()
            .HasIndex(b => b.ShipmentId);

        modelBuilder.Entity<BillingRecord>()
            .HasIndex(b => new { b.BillingDate, b.BillingStatus });

        // FuelMetrics Configuration
        modelBuilder.Entity<FuelMetrics>()
            .HasKey(f => f.MetricsId);

        modelBuilder.Entity<FuelMetrics>()
            .HasIndex(f => f.TruckId);

        // Seed initial state capitals data
        SeedStateCapitals(modelBuilder);
    }

    private static void SeedStateCapitals(ModelBuilder modelBuilder)
    {
        var stateCapitals = new[]
        {
            new StateCapital { StateCode = "AL", StateName = "Alabama", CapitalName = "Montgomery", Latitude = 32.3668, Longitude = -86.2934, Region = "Southeast", HasSoldProducts = true, TotalSalesAmount = 45000, ProductsSold = 120 },
            new StateCapital { StateCode = "AK", StateName = "Alaska", CapitalName = "Juneau", Latitude = 58.3019, Longitude = -134.4197, Region = "West", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "AZ", StateName = "Arizona", CapitalName = "Phoenix", Latitude = 33.4484, Longitude = -112.0742, Region = "Southwest", HasSoldProducts = true, TotalSalesAmount = 89000, ProductsSold = 234 },
            new StateCapital { StateCode = "AR", StateName = "Arkansas", CapitalName = "Little Rock", Latitude = 34.7465, Longitude = -92.2896, Region = "South", HasSoldProducts = true, TotalSalesAmount = 32000, ProductsSold = 85 },
            new StateCapital { StateCode = "CA", StateName = "California", CapitalName = "Sacramento", Latitude = 38.5816, Longitude = -121.4944, Region = "West", HasSoldProducts = true, TotalSalesAmount = 156000, ProductsSold = 412 },
            new StateCapital { StateCode = "CO", StateName = "Colorado", CapitalName = "Denver", Latitude = 39.7392, Longitude = -104.9903, Region = "West", HasSoldProducts = true, TotalSalesAmount = 67000, ProductsSold = 178 },
            new StateCapital { StateCode = "CT", StateName = "Connecticut", CapitalName = "Hartford", Latitude = 41.7658, Longitude = -72.6734, Region = "Northeast", HasSoldProducts = true, TotalSalesAmount = 52000, ProductsSold = 138 },
            new StateCapital { StateCode = "DE", StateName = "Delaware", CapitalName = "Dover", Latitude = 39.1582, Longitude = -75.5244, Region = "Northeast", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "FL", StateName = "Florida", CapitalName = "Tallahassee", Latitude = 30.4383, Longitude = -84.2807, Region = "Southeast", HasSoldProducts = true, TotalSalesAmount = 123000, ProductsSold = 326 },
            new StateCapital { StateCode = "GA", StateName = "Georgia", CapitalName = "Atlanta", Latitude = 33.7490, Longitude = -84.3880, Region = "Southeast", HasSoldProducts = true, TotalSalesAmount = 98000, ProductsSold = 260 },
            new StateCapital { StateCode = "HI", StateName = "Hawaii", CapitalName = "Honolulu", Latitude = 21.3099, Longitude = -157.8581, Region = "West", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "ID", StateName = "Idaho", CapitalName = "Boise", Latitude = 43.6150, Longitude = -116.2023, Region = "West", HasSoldProducts = true, TotalSalesAmount = 28000, ProductsSold = 74 },
            new StateCapital { StateCode = "IL", StateName = "Illinois", CapitalName = "Springfield", Latitude = 39.7817, Longitude = -89.6501, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 87000, ProductsSold = 231 },
            new StateCapital { StateCode = "IN", StateName = "Indiana", CapitalName = "Indianapolis", Latitude = 39.7684, Longitude = -86.1581, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 55000, ProductsSold = 146 },
            new StateCapital { StateCode = "IA", StateName = "Iowa", CapitalName = "Des Moines", Latitude = 41.5868, Longitude = -93.6250, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 41000, ProductsSold = 109 },
            new StateCapital { StateCode = "KS", StateName = "Kansas", CapitalName = "Topeka", Latitude = 39.0473, Longitude = -95.6752, Region = "Midwest", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "KY", StateName = "Kentucky", CapitalName = "Frankfort", Latitude = 38.1975, Longitude = -84.8733, Region = "South", HasSoldProducts = true, TotalSalesAmount = 38000, ProductsSold = 101 },
            new StateCapital { StateCode = "LA", StateName = "Louisiana", CapitalName = "Baton Rouge", Latitude = 30.4515, Longitude = -91.1871, Region = "South", HasSoldProducts = true, TotalSalesAmount = 51000, ProductsSold = 135 },
            new StateCapital { StateCode = "ME", StateName = "Maine", CapitalName = "Augusta", Latitude = 44.3106, Longitude = -69.7795, Region = "Northeast", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "MD", StateName = "Maryland", CapitalName = "Annapolis", Latitude = 38.9784, Longitude = -76.4922, Region = "Northeast", HasSoldProducts = true, TotalSalesAmount = 63000, ProductsSold = 167 },
            new StateCapital { StateCode = "MA", StateName = "Massachusetts", CapitalName = "Boston", Latitude = 42.3601, Longitude = -71.0589, Region = "Northeast", HasSoldProducts = true, TotalSalesAmount = 76000, ProductsSold = 201 },
            new StateCapital { StateCode = "MI", StateName = "Michigan", CapitalName = "Lansing", Latitude = 42.7335, Longitude = -84.5467, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 72000, ProductsSold = 191 },
            new StateCapital { StateCode = "MN", StateName = "Minnesota", CapitalName = "Saint Paul", Latitude = 44.9465, Longitude = -93.0900, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 65000, ProductsSold = 172 },
            new StateCapital { StateCode = "MS", StateName = "Mississippi", CapitalName = "Jackson", Latitude = 32.2988, Longitude = -90.1848, Region = "South", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "MO", StateName = "Missouri", CapitalName = "Jefferson City", Latitude = 38.5767, Longitude = -92.1735, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 59000, ProductsSold = 156 },
            new StateCapital { StateCode = "MT", StateName = "Montana", CapitalName = "Helena", Latitude = 46.5891, Longitude = -112.0391, Region = "West", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "NE", StateName = "Nebraska", CapitalName = "Lincoln", Latitude = 40.8258, Longitude = -96.6852, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 33000, ProductsSold = 87 },
            new StateCapital { StateCode = "NV", StateName = "Nevada", CapitalName = "Carson City", Latitude = 39.1638, Longitude = -119.7674, Region = "West", HasSoldProducts = true, TotalSalesAmount = 44000, ProductsSold = 116 },
            new StateCapital { StateCode = "NH", StateName = "New Hampshire", CapitalName = "Concord", Latitude = 43.2081, Longitude = -71.5376, Region = "Northeast", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "NJ", StateName = "New Jersey", CapitalName = "Trenton", Latitude = 40.2206, Longitude = -74.7597, Region = "Northeast", HasSoldProducts = true, TotalSalesAmount = 81000, ProductsSold = 215 },
            new StateCapital { StateCode = "NM", StateName = "New Mexico", CapitalName = "Santa Fe", Latitude = 35.0853, Longitude = -106.6056, Region = "Southwest", HasSoldProducts = true, TotalSalesAmount = 27000, ProductsSold = 71 },
            new StateCapital { StateCode = "NY", StateName = "New York", CapitalName = "Albany", Latitude = 42.6526, Longitude = -73.7562, Region = "Northeast", HasSoldProducts = true, TotalSalesAmount = 105000, ProductsSold = 278 },
            new StateCapital { StateCode = "NC", StateName = "North Carolina", CapitalName = "Raleigh", Latitude = 35.7796, Longitude = -78.6382, Region = "Southeast", HasSoldProducts = true, TotalSalesAmount = 92000, ProductsSold = 244 },
            new StateCapital { StateCode = "ND", StateName = "North Dakota", CapitalName = "Bismarck", Latitude = 46.8083, Longitude = -100.7837, Region = "Midwest", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "OH", StateName = "Ohio", CapitalName = "Columbus", Latitude = 39.9612, Longitude = -82.9988, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 86000, ProductsSold = 228 },
            new StateCapital { StateCode = "OK", StateName = "Oklahoma", CapitalName = "Oklahoma City", Latitude = 35.4676, Longitude = -97.5164, Region = "South", HasSoldProducts = true, TotalSalesAmount = 48000, ProductsSold = 127 },
            new StateCapital { StateCode = "OR", StateName = "Oregon", CapitalName = "Salem", Latitude = 44.9410, Longitude = -123.0351, Region = "West", HasSoldProducts = true, TotalSalesAmount = 58000, ProductsSold = 154 },
            new StateCapital { StateCode = "PA", StateName = "Pennsylvania", CapitalName = "Harrisburg", Latitude = 40.2732, Longitude = -76.8867, Region = "Northeast", HasSoldProducts = true, TotalSalesAmount = 95000, ProductsSold = 252 },
            new StateCapital { StateCode = "RI", StateName = "Rhode Island", CapitalName = "Providence", Latitude = 41.8240, Longitude = -71.4128, Region = "Northeast", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "SC", StateName = "South Carolina", CapitalName = "Columbia", Latitude = 34.0007, Longitude = -81.0348, Region = "Southeast", HasSoldProducts = true, TotalSalesAmount = 54000, ProductsSold = 143 },
            new StateCapital { StateCode = "SD", StateName = "South Dakota", CapitalName = "Pierre", Latitude = 44.3683, Longitude = -100.3364, Region = "Midwest", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "TN", StateName = "Tennessee", CapitalName = "Nashville", Latitude = 36.1627, Longitude = -86.7816, Region = "South", HasSoldProducts = true, TotalSalesAmount = 71000, ProductsSold = 188 },
            new StateCapital { StateCode = "TX", StateName = "Texas", CapitalName = "Austin", Latitude = 30.2672, Longitude = -97.7431, Region = "South", HasSoldProducts = true, TotalSalesAmount = 142000, ProductsSold = 376 },
            new StateCapital { StateCode = "UT", StateName = "Utah", CapitalName = "Salt Lake City", Latitude = 40.7608, Longitude = -111.8910, Region = "West", HasSoldProducts = true, TotalSalesAmount = 61000, ProductsSold = 161 },
            new StateCapital { StateCode = "VT", StateName = "Vermont", CapitalName = "Montpelier", Latitude = 44.2601, Longitude = -72.5754, Region = "Northeast", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "VA", StateName = "Virginia", CapitalName = "Richmond", Latitude = 37.5407, Longitude = -77.4360, Region = "Southeast", HasSoldProducts = true, TotalSalesAmount = 84000, ProductsSold = 223 },
            new StateCapital { StateCode = "WA", StateName = "Washington", CapitalName = "Olympia", Latitude = 47.0379, Longitude = -122.9007, Region = "West", HasSoldProducts = true, TotalSalesAmount = 79000, ProductsSold = 209 },
            new StateCapital { StateCode = "WV", StateName = "West Virginia", CapitalName = "Charleston", Latitude = 38.3498, Longitude = -81.6326, Region = "Southeast", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 },
            new StateCapital { StateCode = "WI", StateName = "Wisconsin", CapitalName = "Madison", Latitude = 43.0731, Longitude = -89.4012, Region = "Midwest", HasSoldProducts = true, TotalSalesAmount = 68000, ProductsSold = 180 },
            new StateCapital { StateCode = "WY", StateName = "Wyoming", CapitalName = "Cheyenne", Latitude = 41.1400, Longitude = -104.8202, Region = "West", HasSoldProducts = false, TotalSalesAmount = 0, ProductsSold = 0 }
        };

        modelBuilder.Entity<StateCapital>().HasData(stateCapitals);
    }
}
