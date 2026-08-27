namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Security assessment report
/// </summary>
public class SecurityAssessmentReport
{
    public string AgentId { get; set; }
    public int SecurityScore { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

#endregion
