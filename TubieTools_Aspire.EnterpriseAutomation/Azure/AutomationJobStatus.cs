namespace TubieTools_Aspire.EnterpriseAutomation.Azure;

public class AutomationJobStatus
{
    public string JobId { get; set; }
    public string Status { get; set; }
    public string Runbook { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Output { get; set; }
}
