using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.Azure;
using TubieTools_Aspire.EnterpriseAutomation.AzureDevOps;
using TubieTools_Aspire.EnterpriseAutomation.KubernetesGlobal;
using TubieTools_Aspire.EnterpriseAutomation.Security;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;
using TubieTools_Aspire.EnterpriseAutomation.Terraform;

namespace TubieTools_Aspire.EnterpriseAutomation.MCP;

public class McpProtocolHandler : IMcpProtocolHandler
{
    private readonly IAzureAutomationService _azureAutomation;
    private readonly IAzureDevOpsService _azureDevOps;
    private readonly IServiceNowService _serviceNow;
    private readonly ITerraformService _terraform;
    private readonly IKubernetesService _kubernetes;
    private readonly ISecurityService _security;
    private readonly ILogger<McpProtocolHandler> _logger;

    public McpProtocolHandler(
        IAzureAutomationService azureAutomation,
        IAzureDevOpsService azureDevOps,
        IServiceNowService serviceNow,
        ITerraformService terraform,
        IKubernetesService kubernetes,
        ISecurityService security,
        ILogger<McpProtocolHandler> logger)
    {
        _azureAutomation = azureAutomation;
        _azureDevOps = azureDevOps;
        _serviceNow = serviceNow;
        _terraform = terraform;
        _kubernetes = kubernetes;
        _security = security;
        _logger = logger;
    }

    public async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        try
        {
            _logger.LogInformation($"Handling MCP request: {request.Method} - {request.Resource}");

            return (request.Method.ToLower(), request.Resource.ToLower()) switch
            {
                ("azure", "runbook") => new McpResponse
                {
                    Success = true,
                    Data = await _azureAutomation.ExecuteRunbookAsync(
                        request.Parameters["name"].ToString(),
                        (Dictionary<string, object>)request.Parameters["parameters"])
                },
                ("azuredevops", "pipeline") => new McpResponse
                {
                    Success = true,
                    Data = await _azureDevOps.TriggerPipelineAsync(
                        request.Parameters["projectId"].ToString(),
                        request.Parameters["pipelineId"].ToString())
                },
                ("servicenow", "incident") => new McpResponse
                {
                    Success = true,
                    Data = await _serviceNow.GetIncidentsAsync()
                },
                ("terraform", "plan") => new McpResponse
                {
                    Success = true,
                    Data = await _terraform.PlanAsync(
                        request.Parameters["path"].ToString(),
                        (Dictionary<string, string>)request.Parameters["variables"])
                },
                ("kubernetes", "deployments") => new McpResponse
                {
                    Success = true,
                    Data = await _kubernetes.GetDeploymentsAsync()
                },
                _ => new McpResponse { Success = false, Error = "Unknown MCP request" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"MCP request failed: {ex.Message}");
            return new McpResponse { Success = false, Error = ex.Message };
        }
    }
}