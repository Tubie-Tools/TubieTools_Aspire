using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;

namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow.Tools
{
    /// <summary>
    /// Tool for creating incidents in ServiceNow
    /// </summary>
    public class CreateIncidentTool : ICreateIncidentTool
    {
        private readonly IServiceNowService _servicenowService;
        private readonly ILogger<CreateIncidentTool> _logger;

        public string Name => "create_incident";
        public string Description => "Creates a new incident in ServiceNow with title, description, priority, and category";

        public CreateIncidentTool(IServiceNowService servicenowService, ILogger<CreateIncidentTool> logger)
        {
            _servicenowService = servicenowService;
            _logger = logger;
        }

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
        {
            try
            {
                _logger.LogInformation("Creating incident with parameters: {Parameters}", string.Join(", ", parameters.Keys));

                // Extract parameters
                var title = parameters.ContainsKey("title") ? parameters["title"].ToString() : throw new ArgumentException("title is required");
                var description = parameters.ContainsKey("description") ? parameters["description"].ToString() : throw new ArgumentException("description is required");
                var priority = parameters.ContainsKey("priority") ? parameters["priority"].ToString() : "3"; // Default to Medium
                var category = parameters.ContainsKey("category") ? parameters["category"].ToString() : "General";

                // Create the incident request
                var request = new CreateIncidentRequest
                {
                    Title = title,
                    Description = description,
                    Priority = priority,
                    Category = category
                };

                // Call the service
                var incident = await _servicenowService.CreateIncidentAsync(request);

                // Return success result
                return new CreateIncidentResult
                {
                    Success = true,
                    IncidentNumber = incident.Number,
                    Message = $"Incident {incident.Number} created successfully",
                    CreatedIncident = incident
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident");
                return new CreateIncidentResult
                {
                    Success = false,
                    Message = $"Error creating incident: {ex.Message}"
                };
            }
        }
    }
}
