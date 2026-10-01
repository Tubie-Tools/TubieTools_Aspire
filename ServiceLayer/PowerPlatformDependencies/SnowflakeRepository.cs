using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ServiceLayer.PowerPlatformDependencies
{
    public class SnowflakeRepository : ISnowflakeRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SnowflakeRepository> _logger;

        public SnowflakeRepository(HttpClient httpClient, IConfiguration configuration, ILogger<SnowflakeRepository> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> ExecuteQueryAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogInformation("Executing Snowflake query: {Query}", sqlQuery);

                var request = new
                {
                    statement = sqlQuery,
                    parameters = parameters ?? new Dictionary<string, object>()
                };

                var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/v2/statements", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<T>(responseBody, jsonOptions);

                return result ?? throw new InvalidOperationException("Failed to deserialize Snowflake response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Snowflake query: {Query}", sqlQuery);
                throw;
            }
        }

        public async Task<List<T>> ExecuteListQueryAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogInformation("Executing Snowflake list query: {Query}", sqlQuery);

                var request = new
                {
                    statement = sqlQuery,
                    parameters = parameters ?? new Dictionary<string, object>()
                };

                var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/v2/statements", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Deserialize as array
                var result = JsonSerializer.Deserialize<List<T>>(responseBody, jsonOptions);
                return result ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Snowflake list query: {Query}", sqlQuery);
                throw;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string sqlStatement, Dictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogInformation("Executing Snowflake non-query statement: {Statement}", sqlStatement);

                var request = new
                {
                    statement = sqlStatement,
                    parameters = parameters ?? new Dictionary<string, object>()
                };

                var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/v2/statements", content);
                response.EnsureSuccessStatusCode();

                return 1; // Success indicator
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Snowflake non-query statement: {Statement}", sqlStatement);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing Snowflake connection");
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Snowflake connection test failed");
                return false;
            }
        }
    }

}
