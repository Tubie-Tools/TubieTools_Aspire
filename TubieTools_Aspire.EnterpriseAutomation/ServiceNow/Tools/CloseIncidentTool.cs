using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;

namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow.Tools
{
    /// <summary>
    /// Tool for closing incidents in ServiceNow
    /// </summary>
    public class CloseIncidentTool : ICloseIncidentTool
    {
        private readonly IServiceNowService _servicenowService;
        private readonly ILogger<CloseIncidentTool> _logger;

        public string Name => "close_incident";
        public string Description => "Closes an incident in ServiceNow by incident number with optional closure notes";

        public CloseIncidentTool(IServiceNowService servicenowService, ILogger<CloseIncidentTool> logger)
        {
            _servicenowService = servicenowService;
            _logger = logger;
        }

        public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
        {
            try
            {
                _logger.LogInformation("Closing incident with parameters: {Parameters}", string.Join(", ", parameters.Keys));

                // Extract parameters
                var incidentNumber = parameters.ContainsKey("incident_number") 
                    ? parameters["incident_number"].ToString() 
                    : throw new ArgumentException("incident_number is required");

                var closureNotes = parameters.ContainsKey("closure_notes") 
                    ? parameters["closure_notes"].ToString() 
                    : "Incident resolved";

                // Create the update request
                var request = new UpdateIncidentRequest
                {
                    State = "resolved",
                    WorkNotes = closureNotes,
                    AssignedTo = parameters.ContainsKey("assigned_to") ? parameters["assigned_to"].ToString() : null
                };

                // Call the service to update the incident
                var success = await _servicenowService.UpdateIncidentAsync(incidentNumber, request);

                if (success)
                {
                    _logger.LogInformation("Incident {IncidentNumber} closed successfully", incidentNumber);
                }

                // Return result
                return new CloseIncidentResult
                {
                    Success = success,
                    IncidentNumber = incidentNumber,
                    ClosureNotes = closureNotes,
                    Message = success 
                        ? $"Incident {incidentNumber} closed successfully" 
                        : $"Failed to close incident {incidentNumber}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing incident");
                return new CloseIncidentResult
                {
                    Success = false,
                    Message = $"Error closing incident: {ex.Message}"
                };
            }
        }
    }
}
