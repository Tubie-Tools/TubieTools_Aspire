using Microsoft.AspNetCore.Mvc;
using TubieTools_Aspire.EnterpriseAutomation.Azure;
using TubieTools_Aspire.EnterpriseAutomation.KubernetesGlobal;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;
using TubieTools_Aspire.EnterpriseAutomation.Terraform;

namespace TubieTools_Aspire.EnterpriseAutomation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutomationController : ControllerBase
{
    private readonly IAzureAutomationService _azureAutomation;
    private readonly IServiceNowService _serviceNow;
    private readonly ITerraformService _terraform;
    private readonly IKubernetesService _kubernetes;
    private readonly ILogger<AutomationController> _logger;

    public AutomationController(
        IAzureAutomationService azureAutomation,
        IServiceNowService serviceNow,
        ITerraformService terraform,
        IKubernetesService kubernetes,
        ILogger<AutomationController> logger)
    {
        _azureAutomation = azureAutomation;
        _serviceNow = serviceNow;
        _terraform = terraform;
        _kubernetes = kubernetes;
        _logger = logger;
    }

    [HttpPost("azure/runbook")]
    public async Task<IActionResult> ExecuteRunbook([FromBody] ExecuteRunbookRequest request)
    {
        var result = await _azureAutomation.ExecuteRunbookAsync(request.RunbookName, request.Parameters);
        return Ok(result);
    }

    [HttpPost("servicenow/incident")]
    public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        var incident = await _serviceNow.CreateIncidentAsync(request);
        return Ok(incident);
    }

    [HttpPost("terraform/plan")]
    public async Task<IActionResult> TerraformPlan([FromBody] TerraformPlanRequest request)
    {
        var plan = await _terraform.PlanAsync(request.WorkspacePath, request.Variables);
        return Ok(plan);
    }

    [HttpGet("kubernetes/deployments")]
    public async Task<IActionResult> GetDeployments([FromQuery] string ns = "default")
    {
        var deployments = await _kubernetes.GetDeploymentsAsync(ns);
        return Ok(deployments);
    }
}

public class ExecuteRunbookRequest
{
    public string RunbookName { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

public class TerraformPlanRequest
{
    public string WorkspacePath { get; set; }
    public Dictionary<string, string> Variables { get; set; }
}