using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ServiceLayer.PowerPlatformDependencies
{
    public class PowerPlatformDataverseService : IPowerPlatformDataverseService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PowerPlatformDataverseService> _logger;
        private readonly ISnowflakeAuthService _authService;

        public PowerPlatformDataverseService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PowerPlatformDataverseService> logger,
            ISnowflakeAuthService authService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
        }

        public async Task<T> GetRecordAsync<T>(string tableName, string recordId)
        {
            try
            {
                _logger.LogInformation("Retrieving record {RecordId} from table {Table}", recordId, tableName);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"/api/data/v9.0/{tableName}({recordId})");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? throw new InvalidOperationException("Failed to deserialize response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving record from {Table}", tableName);
                throw;
            }
        }

        public async Task<List<T>> GetRecordsAsync<T>(string tableName, string fetchXml)
        {
            try
            {
                _logger.LogInformation("Retrieving records from table {Table}", tableName);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var encodedFetch = Uri.EscapeDataString(fetchXml);
                var response = await _httpClient.GetAsync($"/api/data/v9.0/{tableName}?fetchXml={encodedFetch}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var records = new List<T>();
                if (data.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        var record = JsonSerializer.Deserialize<T>(item.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (record != null) records.Add(record);
                    }
                }
                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving records from {Table}", tableName);
                throw;
            }
        }

        public async Task<string> CreateRecordAsync(string tableName, Dictionary<string, object> attributes)
        {
            try
            {
                _logger.LogInformation("Creating record in table {Table}", tableName);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonSerializer.Serialize(attributes), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/data/v9.0/{tableName}", content);
                response.EnsureSuccessStatusCode();

                var locationHeader = response.Headers.Location?.AbsolutePath;
                return locationHeader?.Split("(")[1]?.Split(")")[0] ?? throw new InvalidOperationException("Failed to extract record ID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating record in {Table}", tableName);
                throw;
            }
        }

        public async Task<bool> UpdateRecordAsync(string tableName, string recordId, Dictionary<string, object> attributes)
        {
            try
            {
                _logger.LogInformation("Updating record {RecordId} in table {Table}", recordId, tableName);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonSerializer.Serialize(attributes), System.Text.Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/data/v9.0/{tableName}({recordId})")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating record in {Table}", tableName);
                throw;
            }
        }

        public async Task<bool> DeleteRecordAsync(string tableName, string recordId)
        {
            try
            {
                _logger.LogInformation("Deleting record {RecordId} from table {Table}", recordId, tableName);

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"/api/data/v9.0/{tableName}({recordId})");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting record from {Table}", tableName);
                throw;
            }
        }
    }

}
