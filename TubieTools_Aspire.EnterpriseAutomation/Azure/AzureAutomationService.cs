using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Automation;
using Azure.Core;  // Add this line
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.EnterpriseAutomation.Azure;

public class AzureAutomationService : IAzureAutomationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureAutomationService> _logger;
    private readonly ArmClient _armClient;

    public AzureAutomationService(IConfiguration configuration, ILogger<AzureAutomationService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var credential = new DefaultAzureCredential();
        _armClient = new ArmClient(credential);
    }

    public async Task<AutomationRunbookResult> ExecuteRunbookAsync(string runbookName, Dictionary<string, object> parameters)
    {
        try
        {
            _logger.LogInformation($"Executing runbook: {runbookName}");

            var subscriptionId = _configuration["Azure:SubscriptionId"];
            var resourceGroup = _configuration["Azure:ResourceGroup"];
            var accountName = _configuration["Azure:AutomationAccount"];

            var resourceGroupResource = _armClient.GetResourceGroupResource(
                ResourceIdentifier.Parse($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}"));
    
            var automationAccountResource = resourceGroupResource.GetAutomationAccounts()
                .FirstOrDefaultAsync(x => x.Data.Name == accountName);

            if (automationAccountResource == null)
            {
                throw new Exception($"Automation account {accountName} not found");
            }

            // Execute runbook logic here
            return new AutomationRunbookResult
            {
                JobId = Guid.NewGuid().ToString(),
                Status = "Completed",
                Output = "Runbook executed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing runbook: {ex.Message}");
            throw;
        }
    }

    public async Task<List<AutomationAccount>> GetAccountsAsync()
    {
        var accounts = new List<AutomationAccount>();
        _logger.LogInformation("Fetching automation accounts");

        // Implementation to fetch accounts from Azure
        return accounts;
    }

    public async Task<AutomationJobStatus> GetJobStatusAsync(string jobId)
    {
        _logger.LogInformation($"Fetching job status for: {jobId}");

        return new AutomationJobStatus
        {
            JobId = jobId,
            Status = "Completed"
        };
    }

    public async Task<bool> CreateRunbookAsync(string name, string content)
    {
        _logger.LogInformation($"Creating runbook: {name}");
        return true;
    }

    public async Task<bool> PublishRunbookAsync(string name)
    {
        _logger.LogInformation($"Publishing runbook: {name}");
        return true;
    }
}

