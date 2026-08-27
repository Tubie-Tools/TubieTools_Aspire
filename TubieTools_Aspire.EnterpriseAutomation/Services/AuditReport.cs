namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Comprehensive audit report
/// </summary>
public class AuditReport
{
    public string AgentId { get; set; }
    public DateTime AuditDate { get; set; }
    public string Auditor { get; set; }
    public string OverallAssessment { get; set; }
    public List<string> FindingsSummary { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

#endregion
