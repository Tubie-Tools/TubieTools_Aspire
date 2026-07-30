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

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class PipelineRun
    {
        public string RunId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PipelineRunStatus
    {
        public string RunId { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public List<string> Logs { get; set; }
    }

    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
    }


}
