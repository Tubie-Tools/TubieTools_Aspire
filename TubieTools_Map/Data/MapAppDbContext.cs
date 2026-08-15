using Microsoft.EntityFrameworkCore;
using TubieTools_Map.Data.Models;

namespace TubieTools_Map.Data;

public class MapAppDbContext : DbContext
{
    public MapAppDbContext(DbContextOptions<MapAppDbContext> options) : base(options) { }

    public DbSet<MapRoute> Routes => Set<MapRoute>();
    public DbSet<RouteSegment> RouteSegments => Set<RouteSegment>();
    public DbSet<RouteRedirection> RouteRedirections => Set<RouteRedirection>();
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Route relationships
        modelBuilder.Entity<RouteSegment>()
            .HasOne(rs => rs.Route)
            .WithMany(r => r.Segments)
            .HasForeignKey(rs => rs.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RouteRedirection>()
            .HasOne(rr => rr.OriginalRoute)
            .WithMany(r => r.Redirections)
            .HasForeignKey(rr => rr.OriginalRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RouteRedirection>()
            .HasOne(rr => rr.AlternativeRoute)
            .WithMany()
            .HasForeignKey(rr => rr.AlternativeRouteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        modelBuilder.Entity<MapRoute>().HasIndex(r => r.CreatedBy);
        modelBuilder.Entity<MapRoute>().HasIndex(r => r.Status);
        modelBuilder.Entity<Account>().HasIndex(a => a.Email).IsUnique();
        modelBuilder.Entity<Account>().HasIndex(a => a.EntraObjectId);
    }

    public static async Task SeedAsync(MapAppDbContext context)
    {
        if (!await context.Accounts.AnyAsync())
        {
            var adminAccount = new Account
            {
                Email = "admin@tubietools.com",
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true
            };

            context.Accounts.Add(adminAccount);
            await context.SaveChangesAsync();
        }
    }
}