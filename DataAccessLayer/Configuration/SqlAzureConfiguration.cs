using DataAccessLayer.Data.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataAccessLayer.Configuration
{
    /// <summary>
    /// SQL Azure database configuration and setup
    /// Provides methods to configure DbContexts with SQL Azure connection strings
    /// and handle connection pooling and retry policies
    /// </summary>
    public static class SqlAzureConfiguration
    {
        /// <summary>
        /// SQL Azure connection string settings from configuration
        /// </summary>
        public class SqlAzureSettings
        {
            public string? Server { get; set; }
            public string? Database { get; set; }
            public string? UserId { get; set; }
            public string? Password { get; set; }
            public int ConnectTimeout { get; set; } = 30;
            public int CommandTimeout { get; set; } = 300;
            public bool EnableRetryOnFailure { get; set; } = true;
            public int MaxRetryCount { get; set; } = 3;
            public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
            public bool Encrypt { get; set; } = true;
            public bool TrustServerCertificate { get; set; } = false;

            /// <summary>
            /// Builds connection string from settings
            /// </summary>
            public string GetConnectionString()
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    InitialCatalog = Database,
                    UserID = UserId,
                    Password = Password,
                    ConnectRetryCount = EnableRetryOnFailure ? MaxRetryCount : 0,
                    ConnectRetryInterval = (int)MaxRetryDelay.TotalSeconds,
                    ConnectTimeout = ConnectTimeout,
                    Encrypt = Encrypt,
                    TrustServerCertificate = TrustServerCertificate
                };
                return builder.ToString();
            }
        }

        /// <summary>
        /// Configures TubieDbContext for SQL Azure
        /// </summary>
        public static IServiceCollection AddTubieDbContextSqlAzure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var sqlSettings = configuration.GetSection("SqlAzure").Get<SqlAzureSettings>()
                ?? new SqlAzureSettings();

            var connectionString = sqlSettings.GetConnectionString();

            services.AddDbContext<TubieDbContext>(options =>
            {
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(sqlSettings.CommandTimeout);

                        if (sqlSettings.EnableRetryOnFailure)
                        {
                            //sqlOptions.EnableRetryOnFailure(
                            //    maxRetryCount: sqlSettings.MaxRetryCount,
                            //    maxRetryDelayOnFirstAttempt: sqlSettings.MaxRetryDelay,
                            //    errorNumbersToAdd: null);
                        }

                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });

                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            });

            return services;
        }

        /// <summary>
        /// Configures ApplicationDbContext for SQL Azure
        /// </summary>
        public static IServiceCollection AddApplicationDbContextSqlAzure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var sqlSettings = configuration.GetSection("SqlAzure").Get<SqlAzureSettings>()
                ?? new SqlAzureSettings();

            var connectionString = sqlSettings.GetConnectionString();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(sqlSettings.CommandTimeout);

                        if (sqlSettings.EnableRetryOnFailure)
                        {
                            //sqlOptions.EnableRetryOnFailure(
                            //    maxRetryCount: sqlSettings.MaxRetryCount,
                            //    maxRetryDelayOnFirstAttempt: sqlSettings.MaxRetryDelay,
                            //    errorNumbersToAdd: null);
                        }

                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });

                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            });

            return services;
        }

        /// <summary>
        /// Configures multiple DbContexts for SQL Azure with separate databases
        /// </summary>
        public static IServiceCollection AddMultipleDbContextsSqlAzure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var defaultSettings = configuration.GetSection("SqlAzure").Get<SqlAzureSettings>()
                ?? new SqlAzureSettings();

            services.AddTubieDbContextSqlAzure(configuration);
            services.AddApplicationDbContextSqlAzure(configuration);

            return services;
        }
    }
}
