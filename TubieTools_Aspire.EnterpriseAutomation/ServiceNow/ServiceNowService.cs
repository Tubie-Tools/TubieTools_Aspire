using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow;

public class ServiceNowService : IServiceNowService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceNowService> _logger;

    public ServiceNowService(HttpClient httpClient, IConfiguration configuration, ILogger<ServiceNowService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var instance = _configuration["ServiceNow:Instance"];
        _httpClient.BaseAddress = new Uri($"https://{instance}.service-now.com/api/now");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["ServiceNow:Token"]}");
    }

    public async Task<List<Incident>> GetIncidentsAsync(string query = "")
    {
        try
        {
            _logger.LogInformation("Fetching incidents from ServiceNow");

            var response = await _httpClient.GetAsync($"/table/incident?sysparm_query={query}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ServiceNowResponse<Incident>>();
            return result?.Result ?? new List<Incident>();
            }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching incidents: {ex.Message}");
            throw;
        }
    }

    public async Task<Incident> GetIncidentAsync(string incidentNumber)
    {
        _logger.LogInformation($"Fetching incident: {incidentNumber}");
        return new Incident { Number = incidentNumber };
    }

    public async Task<Incident> CreateIncidentAsync(CreateIncidentRequest request)
    {
        _logger.LogInformation($"Creating incident: {request.Title}");

        var response = await _httpClient.PostAsJsonAsync("/table/incident", request);
        response.EnsureSuccessStatusCode();

        return new Incident { Number = Guid.NewGuid().ToString() };
    }

    public async Task<bool> UpdateIncidentAsync(string incidentNumber, UpdateIncidentRequest request)
    {
        _logger.LogInformation($"Updating incident: {incidentNumber}");
        return true;
    }

    public async Task<List<ChangeRequest>> GetChangeRequestsAsync()
    {
        _logger.LogInformation("Fetching change requests");
        return new List<ChangeRequest>();
    }

    public async Task<ChangeRequest> CreateChangeRequestAsync(CreateChangeRequest request)
    {
        _logger.LogInformation($"Creating change request: {request.Title}");
        return new ChangeRequest { Id = Guid.NewGuid().ToString() };
    }

    public async Task<bool> ApproveChangeAsync(string changeId)
    {
        _logger.LogInformation($"Approving change: {changeId}");
        return true;
    }
}

public class ServiceNowResponse<T>
{
    public List<T> Result { get; set; } = new();
}

