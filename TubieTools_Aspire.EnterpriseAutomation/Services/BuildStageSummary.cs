namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

public class BuildStageSummary
{
    public string StageName { get; set; }
    public string Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string LogsLocation { get; set; }
}

#endregion
