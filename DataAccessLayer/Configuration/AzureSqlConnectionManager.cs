using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DataAccessLayer.Configuration
{
    /// <summary>
    /// Manages connections to Azure SQL Database
    /// Handles connection pooling, retry logic, and connection health checks
    /// </summary>
    public interface IAzureSqlConnectionManager
    {
        /// <summary>
        /// Gets a connection to Azure SQL Database
        /// </summary>
        Task<SqlConnection> GetConnectionAsync(string? connectionString = null);

        /// <summary>
        /// Tests the connection to Azure SQL Database
        /// </summary>
        Task<bool> TestConnectionAsync(string? connectionString = null);

        /// <summary>
        /// Gets connection pool statistics
        /// </summary>
        SqlConnectionStringBuilder GetConnectionPoolStats();

        /// <summary>
        /// Clears the connection pool
        /// </summary>
        void ClearConnectionPool();
    }

    /// <summary>
    /// Implementation of Azure SQL connection manager
    /// </summary>
    public class AzureSqlConnectionManager : IAzureSqlConnectionManager
    {
        private readonly ILogger<AzureSqlConnectionManager> _logger;
        private readonly string _connectionString;
        private readonly SqlAzureConfiguration.SqlAzureSettings _settings;

        /// <summary>
        /// Constructor
        /// </summary>
        public AzureSqlConnectionManager(
            ILogger<AzureSqlConnectionManager> logger,
            string connectionString,
            SqlAzureConfiguration.SqlAzureSettings? settings = null)
        {
            _logger = logger;
            _connectionString = connectionString;
            _settings = settings ?? new SqlAzureConfiguration.SqlAzureSettings();
        }

        /// <summary>
        /// Gets an open connection to Azure SQL Database
        /// </summary>
        public async Task<SqlConnection> GetConnectionAsync(string? connectionString = null)
        {
            var connStr = connectionString ?? _connectionString;
            var connection = new SqlConnection(connStr);

            try
            {
                _logger.LogInformation("Opening connection to Azure SQL Database...");
                await connection.OpenAsync();
                _logger.LogInformation("Successfully opened connection to Azure SQL Database");
                return connection;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Failed to open connection to Azure SQL Database. Error: {ErrorCode}", ex.Number);
                connection.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while opening connection to Azure SQL Database");
                connection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Tests the connection to Azure SQL Database
        /// </summary>
        public async Task<bool> TestConnectionAsync(string? connectionString = null)
        {
            SqlConnection? connection = null;
            try
            {
                connection = await GetConnectionAsync(connectionString);

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = _settings.CommandTimeout;

                var result = await command.ExecuteScalarAsync();
                _logger.LogInformation("Connection test successful");
                return result != null && result.Equals(1);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                _logger.LogWarning("Connection timeout. Server may be temporarily unavailable.");
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during connection test: {ErrorCode}", ex.Number);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during connection test");
                return false;
            }
            finally
            {
                connection?.Dispose();
            }
        }

        /// <summary>
        /// Gets connection pool statistics
        /// </summary>
        public SqlConnectionStringBuilder GetConnectionPoolStats()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            _logger.LogInformation("Connection Pool Stats: Min Pool Size: {MinPoolSize}, Max Pool Size: {MaxPoolSize}",
                builder.MinPoolSize, builder.MaxPoolSize);
            return builder;
        }

        /// <summary>
        /// Clears the connection pool for the current connection string
        /// </summary>
        public void ClearConnectionPool()
        {
            try
            {
                SqlConnection.ClearAllPools();
                _logger.LogInformation("Connection pool cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing connection pool");
                throw;
            }
        }
    }

    /// <summary>
    /// Extension methods for IAzureSqlConnectionManager
    /// </summary>
    public static class AzureSqlConnectionManagerExtensions
    {
        /// <summary>
        /// Executes a query and returns results
        /// </summary>
        public static async Task<List<T>> ExecuteQueryAsync<T>(
            this IAzureSqlConnectionManager manager,
            string query,
            Func<SqlDataReader, T> mapper,
            string? connectionString = null)
        {
            var results = new List<T>();
            using var connection = await manager.GetConnectionAsync(connectionString);
            using var command = connection.CreateCommand();

            command.CommandText = query;
            command.CommandType = CommandType.Text;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(mapper(reader));
            }

            return results;
        }

        /// <summary>
        /// Executes a non-query command
        /// </summary>
        public static async Task<int> ExecuteNonQueryAsync(
            this IAzureSqlConnectionManager manager,
            string query,
            string? connectionString = null)
        {
            using var connection = await manager.GetConnectionAsync(connectionString);
            using var command = connection.CreateCommand();

            command.CommandText = query;
            command.CommandType = CommandType.Text;

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a scalar query
        /// </summary>
        public static async Task<object?> ExecuteScalarAsync(
            this IAzureSqlConnectionManager manager,
            string query,
            string? connectionString = null)
        {
            using var connection = await manager.GetConnectionAsync(connectionString);
            using var command = connection.CreateCommand();

            command.CommandText = query;
            command.CommandType = CommandType.Text;

            return await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// Executes a stored procedure
        /// </summary>
        public static async Task<int> ExecuteStoredProcedureAsync(
            this IAzureSqlConnectionManager manager,
            string procedureName,
            SqlParameter[]? parameters = null,
            string? connectionString = null)
        {
            using var connection = await manager.GetConnectionAsync(connectionString);
            using var command = connection.CreateCommand();

            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }
    }
}
