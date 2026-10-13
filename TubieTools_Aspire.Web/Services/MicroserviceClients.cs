using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace TubieTools_Aspire.Web.Services;

/// <summary>
/// Service for calling the Public API endpoints
/// </summary>
public class PublicApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PublicApiClient> _logger;

    public PublicApiClient(HttpClient httpClient, ILogger<PublicApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CareCareProvider>> GetCareProvidersAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CareCareProvider>>("/api/careproviders");
            return result ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting care providers");
            return new();
        }
    }

    public async Task<CareCareProvider?> GetCareProviderAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CareCareProvider>($"/api/careproviders/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting care provider {Id}", id);
            return null;
        }
    }

    public async Task<ApiResponse?> CreateCareProviderAsync(CareCareProvider provider)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/careproviders", provider);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<ApiResponse>();
            }
            return new ApiResponse { Success = false, Message = "Failed to create provider" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating care provider");
            return new ApiResponse { Success = false, Message = ex.Message };
        }
    }
}

/// <summary>
/// Service for calling the Sentiment Analysis API
/// </summary>
public class SentimentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SentimentApiClient> _logger;

    public SentimentApiClient(HttpClient httpClient, ILogger<SentimentApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SentimentResult?> AnalyzeSentimentAsync(string text)
    {
        try
        {
            var request = new { text };
            var response = await _httpClient.PostAsJsonAsync("/api/sentiment/analyze", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<SentimentResult>();
            }
            _logger.LogError("Failed to analyze sentiment: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return null;
        }
    }

    public async Task<SentimentBatchResult?> AnalyzeBatchAsync(List<string> texts)
    {
        try
        {
            var request = new { texts };
            var response = await _httpClient.PostAsJsonAsync("/api/sentiment/batch", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<SentimentBatchResult>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch sentiment analysis");
            return null;
        }
    }
}

/// <summary>
/// Service for calling the Forecasting API
/// </summary>
public class ForecastingApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ForecastingApiClient> _logger;

    public ForecastingApiClient(HttpClient httpClient, ILogger<ForecastingApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ForecastResult?> GetForecastAsync(string productId, int periods = 30)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ForecastResult>(
                $"/api/forecast/{productId}?periods={periods}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting forecast for {ProductId}", productId);
            return null;
        }
    }

    public async Task<List<ForecastResult>> GetMultipleForecastsAsync(List<string> productIds)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/forecast/batch", productIds);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<List<ForecastResult>>();
                return result ?? new();
            }
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch forecasts");
            return new();
        }
    }
}

/// <summary>
/// Service for calling the Logistics/OSRM API
/// </summary>
public class LogisticsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LogisticsApiClient> _logger;

    public LogisticsApiClient(HttpClient httpClient, ILogger<LogisticsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RouteResult?> GetRouteAsync(double fromLat, double fromLon, double toLat, double toLon)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/route?fromLat={fromLat}&fromLon={fromLon}&toLat={toLat}&toLon={toLon}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<RouteResult>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route");
            return null;
        }
    }

    public async Task<DistanceMatrixResult?> GetDistanceMatrixAsync(List<Location> locations)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/distance-matrix", locations);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<DistanceMatrixResult>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distance matrix");
            return null;
        }
    }
}

/// <summary>
/// Service for calling the Enterprise Automation API
/// </summary>
public class EnterpriseAutomationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnterpriseAutomationClient> _logger;

    public EnterpriseAutomationClient(HttpClient httpClient, ILogger<EnterpriseAutomationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<AutomationWorkflow>> GetWorkflowsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AutomationWorkflow>>(
                "/api/workflows");
            return result ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflows");
            return new();
        }
    }

    public async Task<WorkflowExecutionResult?> ExecuteWorkflowAsync(string workflowId, Dictionary<string, object> parameters)
    {
        try
        {
            var request = new { workflowId, parameters };
            var response = await _httpClient.PostAsJsonAsync("/api/workflows/execute", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<WorkflowExecutionResult>();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId}", workflowId);
            return null;
        }
    }

    public async Task<WorkflowHistory?> GetWorkflowHistoryAsync(string workflowId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<WorkflowHistory>(
                $"/api/workflows/{workflowId}/history");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow history");
            return null;
        }
    }
}

// ============================================================================
// DTOs for communicating with services
// ============================================================================

public class CareCareProvider
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double? Rating { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class SentimentResult
{
    public string Text { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;  // Positive, Negative, Neutral
    public double Score { get; set; }  // 0-1 confidence
}

public class SentimentBatchResult
{
    public List<SentimentResult> Results { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

public class ForecastResult
{
    public string ProductId { get; set; } = string.Empty;
    public List<ForecastPoint> Forecast { get; set; } = new();
    public double Accuracy { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class ForecastPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public double? ConfidenceUpper { get; set; }
    public double? ConfidenceLower { get; set; }
}

public class RouteResult
{
    public double Distance { get; set; }  // meters
    public int Duration { get; set; }  // seconds
    public List<Coordinate> Polyline { get; set; } = new();
}

public class Coordinate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class Location
{
    public string Id { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class DistanceMatrixResult
{
    public double[,] Distances { get; set; } = new double[0, 0];
    public int[,] Durations { get; set; } = new int[0, 0];
}

public class AutomationWorkflow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStep> Steps { get; set; } = new();
    public Dictionary<string, WorkflowParameter> Parameters { get; set; } = new();
}

public class WorkflowStep
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class WorkflowParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
}

public class WorkflowExecutionResult
{
    public string ExecutionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;  // Running, Success, Failed
    public Dictionary<string, object> Output { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class WorkflowHistory
{
    public string WorkflowId { get; set; } = string.Empty;
    public List<WorkflowExecutionResult> Executions { get; set; } = new();
}
