namespace TubieTools_Aspire.EnterpriseAutomation.Azure;

public interface IAzureAutomationService
{
    Task<AutomationRunbookResult> ExecuteRunbookAsync(string runbookName, Dictionary<string, object> parameters);
    Task<List<AutomationAccount>> GetAccountsAsync();
    Task<AutomationJobStatus> GetJobStatusAsync(string jobId);
    Task<bool> CreateRunbookAsync(string name, string content);
    Task<bool> PublishRunbookAsync(string name);
}

public class AutomationRunbookResult
{
    public string JobId { get; set; }
    public string Status { get; set; }
    public string Output { get; set; }
    public DateTime CreatedTime { get; set; }
}

public class AutomationAccount
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ResourceGroup { get; set; }
}

public class AutomationJobStatus
{
    public string JobId { get; set; }
    public string Status { get; set; }
    public string Runbook { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Output { get; set; }
}
