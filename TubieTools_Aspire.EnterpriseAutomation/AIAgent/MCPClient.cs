using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow.Tools;

namespace TubieTools_Aspire.EnterpriseAutomation.AIAgent
{
    /// <summary>
    /// MCP Client implementation for invoking tools
    /// </summary>
    public class MCPClient : IMCPClient
    {
        private readonly IServiceNowToolsFactory _toolsFactory;
        private readonly ILogger<MCPClient> _logger;
        private List<AIChatTool> _availableTools;

        public bool IsConnected { get; private set; } = true;

        public MCPClient(IServiceNowToolsFactory toolsFactory, ILogger<MCPClient> logger)
        {
            _toolsFactory = toolsFactory;
            _logger = logger;
            _availableTools = new List<AIChatTool>();
            InitializeTools();
        }

        private void InitializeTools()
        {
            try
            {
                _logger.LogInformation("Initializing MCP Client with ServiceNow tools");

                var tools = _toolsFactory.GetAllTools();

                foreach (var tool in tools)
                {
                    _availableTools.Add(new AIChatTool
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        Parameters = GetToolParameters(tool.Name)
                    });
                }

                _logger.LogInformation($"MCP Client initialized with {_availableTools.Count} tools");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MCP Client");
                IsConnected = false;
            }
        }

        public async Task<object> InvokeToolAsync(string toolName, Dictionary<string, object> parameters)
        {
            try
            {
                _logger.LogInformation($"Invoking MCP tool: {toolName} with parameters: {string.Join(", ", parameters.Keys)}");

                return toolName.ToLower() switch
                {
                    "create_incident" => await _toolsFactory.GetCreateIncidentTool().ExecuteAsync(parameters),
                    "search_incident" => await _toolsFactory.GetSearchIncidentTool().ExecuteAsync(parameters),
                    "close_incident" => await _toolsFactory.GetCloseIncidentTool().ExecuteAsync(parameters),
                    _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error invoking tool {toolName}");
                throw;
            }
        }

        public async Task<List<AIChatTool>> GetAvailableToolsAsync()
        {
            return await Task.FromResult(_availableTools);
        }

        private Dictionary<string, object> GetToolParameters(string toolName)
        {
            return toolName switch
            {
                "create_incident" => new Dictionary<string, object>
                {
                    { "title", "string (required)" },
                    { "description", "string (required)" },
                    { "priority", "string (optional, default: '3')" },
                    { "category", "string (optional, default: 'General')" }
                },
                "search_incident" => new Dictionary<string, object>
                {
                    { "number", "string (optional)" },
                    { "title", "string (optional)" },
                    { "state", "string (optional)" },
                    { "priority", "string (optional)" },
                    { "assigned_to", "string (optional)" }
                },
                "close_incident" => new Dictionary<string, object>
                {
                    { "incident_number", "string (required)" },
                    { "closure_notes", "string (optional)" },
                    { "assigned_to", "string (optional)" }
                },
                _ => new Dictionary<string, object>()
            };
        }
    }
}
