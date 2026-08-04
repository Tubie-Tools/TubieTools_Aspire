using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using TubieTools_Aspire.Security.Authorization;
using TubieTools_Aspire.Security.Models;

namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow;

public class ServiceNowService : IServiceNowService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceNowService> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServiceNowService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ServiceNowService> logger,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;

        var instance = _configuration["ServiceNow:Instance"];
        _httpClient.BaseAddress = new Uri($"https://{instance}.service-now.com/api/now");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["ServiceNow:Token"]}");
    }

    public async Task<List<Incident>> GetIncidentsAsync(string query = "")
    {
        try
        {
            // Check authorization for read operation
            var user = _httpContextAccessor.HttpContext?.User;
            var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowRead);

            if (!isAuthorized)
            {
                _logger.LogWarning("User {User} denied access to ServiceNow read operation",
                    _authorizationService.GetUserPrincipalName(user) ?? "unknown");
                throw new UnauthorizedAccessException("You do not have permission to read incidents from ServiceNow");
            }

            _logger.LogInformation("User {User} fetching incidents from ServiceNow with query: {Query}",
                _authorizationService.GetUserPrincipalName(user) ?? "unknown", query);

            var response = await _httpClient.GetAsync($"/table/incident?sysparm_query={query}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ServiceNowResponse<Incident>>();
            return result?.Result ?? new List<Incident>();
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching incidents: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Incident> GetIncidentAsync(string incidentNumber)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowRead);

        if (!isAuthorized)
        {
            _logger.LogWarning("User {User} denied access to ServiceNow read operation",
                _authorizationService.GetUserPrincipalName(user) ?? "unknown");
            throw new UnauthorizedAccessException("You do not have permission to read incidents");
        }

        _logger.LogInformation($"User {_authorizationService.GetUserPrincipalName(user)} fetching incident: {incidentNumber}");
        return new Incident { Number = incidentNumber };
    }

    public async Task<Incident> CreateIncidentAsync(CreateIncidentRequest request)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowCreate);

        if (!isAuthorized)
        {
            _logger.LogWarning("User {User} denied access to ServiceNow create operation",
                _authorizationService.GetUserPrincipalName(user) ?? "unknown");
            throw new UnauthorizedAccessException("You do not have permission to create incidents in ServiceNow");
        }

        _logger.LogInformation("User {User} creating incident: {Title}",
            _authorizationService.GetUserPrincipalName(user) ?? "unknown",
            request.Title);

        var response = await _httpClient.PostAsJsonAsync("/table/incident", request);
        response.EnsureSuccessStatusCode();

        return new Incident { Number = Guid.NewGuid().ToString() };
    }

    public async Task<bool> UpdateIncidentAsync(string incidentNumber, UpdateIncidentRequest request)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowUpdate);

        if (!isAuthorized)
        {
            _logger.LogWarning("User {User} denied access to ServiceNow update operation",
                _authorizationService.GetUserPrincipalName(user) ?? "unknown");
            throw new UnauthorizedAccessException("You do not have permission to update incidents in ServiceNow");
        }

        _logger.LogInformation("User {User} updating incident: {IncidentNumber}",
            _authorizationService.GetUserPrincipalName(user) ?? "unknown",
            incidentNumber);

        return true;
    }

    public async Task<List<ChangeRequest>> GetChangeRequestsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowRead);

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to read change requests");
        }

        _logger.LogInformation("User {User} fetching change requests",
            _authorizationService.GetUserPrincipalName(user) ?? "unknown");

        return new List<ChangeRequest>();
    }

    public async Task<ChangeRequest> CreateChangeRequestAsync(CreateChangeRequest request)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowCreate);

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to create change requests");
        }

        _logger.LogInformation("User {User} creating change request: {Title}",
            _authorizationService.GetUserPrincipalName(user) ?? "unknown",
            request.Title);

        return new ChangeRequest { Id = Guid.NewGuid().ToString() };
    }

    public async Task<bool> ApproveChangeAsync(string changeId)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAuthorized = await _authorizationService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowAdmin);

        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to approve changes");
        }

        _logger.LogInformation("User {User} approving change: {ChangeId}",
            _authorizationService.GetUserPrincipalName(user) ?? "unknown",
            changeId);

        return true;
    }
}

public class ServiceNowResponse<T>
{
    public List<T> Result { get; set; } = new();
}

