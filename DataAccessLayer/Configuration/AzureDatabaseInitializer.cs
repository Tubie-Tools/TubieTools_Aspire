using DataAccessLayer.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Configuration
{
    /// <summary>
    /// Handles database initialization and migration for Azure SQL
    /// </summary>
    public interface IAzureDatabaseInitializer
    {
        /// <summary>
        /// Initializes database with migrations
        /// </summary>
        Task InitializeDatabaseAsync<T>(T context) where T : DbContext;

        /// <summary>
        /// Seeds initial data
        /// </summary>
        Task SeedDataAsync<T>(T context) where T : DbContext;

        /// <summary>
        /// Applies pending migrations
        /// </summary>
        Task ApplyMigrationsAsync<T>(T context) where T : DbContext;

        /// <summary>
        /// Checks if database is accessible
        /// </summary>
        Task<bool> IsDatabaseAccessibleAsync<T>(T context) where T : DbContext;
    }

    /// <summary>
    /// Implementation of database initializer for Azure SQL
    /// </summary>
    public class AzureDatabaseInitializer : IAzureDatabaseInitializer
    {
        private readonly ILogger<AzureDatabaseInitializer> _logger;

        public AzureDatabaseInitializer(ILogger<AzureDatabaseInitializer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes the database with migrations and seed data
        /// </summary>
        public async Task InitializeDatabaseAsync<T>(T context) where T : DbContext
        {
            try
            {
                _logger.LogInformation("Initializing Azure SQL database for {ContextName}...", typeof(T).Name);

                // Check if database is accessible
                if (!await IsDatabaseAccessibleAsync(context))
                {
                    _logger.LogWarning("Database is not accessible. Attempting to create...");
                    await context.Database.EnsureCreatedAsync();
                }

                // Apply pending migrations
                await ApplyMigrationsAsync(context);

                // Seed initial data
                await SeedDataAsync(context);

                _logger.LogInformation("Database initialization completed for {ContextName}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database for {ContextName}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Applies pending migrations to the database
        /// </summary>
        public async Task ApplyMigrationsAsync<T>(T context) where T : DbContext
        {
            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations to {ContextName}...", 
                        pendingMigrations.Count(), typeof(T).Name);

                    await context.Database.MigrateAsync();

                    _logger.LogInformation("Migrations applied successfully for {ContextName}", typeof(T).Name);
                }
                else
                {
                    _logger.LogInformation("No pending migrations for {ContextName}", typeof(T).Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying migrations for {ContextName}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Seeds initial data to the database
        /// </summary>
        public async Task SeedDataAsync<T>(T context) where T : DbContext
        {
            try
            {
                _logger.LogInformation("Seeding data for {ContextName}...", typeof(T).Name);

                // ApplicationDbContext seeding
                if (typeof(T).Name == nameof(ApplicationDbContext))
                {
                    await SeedApplicationDbContextAsync((ApplicationDbContext)(object)context);
                }

                // TubieDbContext seeding
                if (typeof(T).Name == nameof(TubieDbContext))
                {
                    await SeedTubieDbContextAsync((TubieDbContext)(object)context);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Data seeding completed for {ContextName}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data for {ContextName}", typeof(T).Name);
                // Don't throw - seeding failures shouldn't prevent startup
            }
        }

        /// <summary>
        /// Checks if the database is accessible and responding
        /// </summary>
        public async Task<bool> IsDatabaseAccessibleAsync<T>(T context) where T : DbContext
        {
            try
            {
                _logger.LogInformation("Testing connection to {ContextName} database...", typeof(T).Name);
                var canConnect = await context.Database.CanConnectAsync();

                if (canConnect)
                {
                    _logger.LogInformation("Database connection successful for {ContextName}", typeof(T).Name);
                }
                else
                {
                    _logger.LogWarning("Database connection failed for {ContextName}", typeof(T).Name);
                }

                return canConnect;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error testing database connection for {ContextName}", typeof(T).Name);
                return false;
            }
        }

        /// <summary>
        /// Seeds initial data for ApplicationDbContext (Identity roles, users, etc.)
        /// </summary>
        private async Task SeedApplicationDbContextAsync(ApplicationDbContext context)
        {
            try
            {
                _logger.LogInformation("Seeding ApplicationDbContext with default identity roles");

                // Check if seed data already exists
                if (await context.Roles.AnyAsync())
                {
                    _logger.LogInformation("ApplicationDbContext already contains roles, skipping seed");
                    return;
                }

                // Roles will be seeded automatically by OnModelCreating
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding ApplicationDbContext");
                throw;
            }
        }

        /// <summary>
        /// Seeds initial data for TubieDbContext (sample data, if needed)
        /// </summary>
        private async Task SeedTubieDbContextAsync(TubieDbContext context)
        {
            try
            {
                _logger.LogInformation("Checking TubieDbContext seed data status");

                // Check if seed data already exists
                if (await context.CareProviders.AnyAsync())
                {
                    _logger.LogInformation("TubieDbContext already contains care provider data, skipping seed");
                    return;
                }

                // Add any default/test data as needed
                _logger.LogInformation("No care provider data found, skipping additional seeding");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding TubieDbContext");
                throw;
            }
        }
    }
}
