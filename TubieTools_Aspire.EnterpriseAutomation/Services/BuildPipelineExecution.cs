namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Build pipeline execution details
/// </summary>
public class BuildPipelineExecution
{
    public string ExecutionId { get; set; }
    public string AgentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } // Running, Completed, Failed
    public int ProgressPercentage { get; set; }
    public List<BuildStageSummary> StageSummaries { get; set; } = new();
}

#endregion
