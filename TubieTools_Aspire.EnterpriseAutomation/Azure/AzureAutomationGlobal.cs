namespace TubieTools_Aspire.EnterpriseAutomation.Azure;

public interface IAzureAutomationService
{
    Task<AutomationRunbookResult> ExecuteRunbookAsync(string runbookName, Dictionary<string, object> parameters);
    Task<List<AutomationAccount>> GetAccountsAsync();
    Task<AutomationJobStatus> GetJobStatusAsync(string jobId);
    Task<bool> CreateRunbookAsync(string name, string content);
    Task<bool> PublishRunbookAsync(string name);
}
