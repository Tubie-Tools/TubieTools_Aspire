namespace TubieTools_Aspire.EnterpriseAutomation.AzureDevOps
{
    public interface IAzureDevOpsService
    {
        Task<List<Project>> GetProjectsAsync();
        Task<PipelineRun> TriggerPipelineAsync(string projectId, string pipelineId);
        Task<PipelineRunStatus> GetPipelineStatusAsync(string projectId, string runId);
        Task<List<WorkItem>> GetWorkItemsAsync(string projectId, string wiql);
        Task<bool> UpdateWorkItemAsync(int workItemId, Dictionary<string, object> fields);
    }
}
