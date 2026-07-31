using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;

namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow.Tools
{
    /// <summary>
    /// Tool for searching incidents in ServiceNow
    /// </summary>
    public class SearchIncidentTool : ISearchIncidentTool
    {
        private readonly IServiceNowService _servicenowService;
        private readonly ILogger<SearchIncidentTool> _logger;

        public string Name => "search_incident";
        public string Description => "Searches for incidents in ServiceNow based on query parameters (number, title, state, priority, etc.)";

        public SearchIncidentTool(IServiceNowService servicenowService, ILogger<SearchIncidentTool> logger)
        {
            _servicenowService = servicenowService;
            _logger = logger;
        }

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
        {
            try
            {
                _logger.LogInformation("Searching incidents with parameters: {Parameters}", string.Join(", ", parameters.Keys));

                // Build query string based on parameters
                var queryParts = new List<string>();

                if (parameters.ContainsKey("number"))
                {
                    queryParts.Add($"numberSTARTSWITH{parameters["number"]}");
                }

                if (parameters.ContainsKey("title"))
                {
                    queryParts.Add($"short_descriptionLIKE{parameters["title"]}");
                }

                if (parameters.ContainsKey("state"))
                {
                    queryParts.Add($"stateEQ{parameters["state"]}");
                }

                if (parameters.ContainsKey("priority"))
                {
                    queryParts.Add($"priorityEQ{parameters["priority"]}");
                }

                if (parameters.ContainsKey("assigned_to"))
                {
                    queryParts.Add($"assigned_toEQ{parameters["assigned_to"]}");
                }

                // Combine query parts with AND operator
                var query = string.Join("^", queryParts);

                // Call the service
                var incidents = await _servicenowService.GetIncidentsAsync(query);

                // Return success result
                return new SearchIncidentResult
                {
                    Success = true,
                    TotalCount = incidents.Count,
                    Incidents = incidents,
                    Message = $"Found {incidents.Count} incident(s)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching incidents");
                return new SearchIncidentResult
                {
                    Success = false,
                    Message = $"Error searching incidents: {ex.Message}"
                };
            }
        }
    }
}
