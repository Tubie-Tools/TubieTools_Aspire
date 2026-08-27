namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Regulatory compliance report
/// </summary>
public class RegulatoryComplianceReport
{
    public string AgentId { get; set; }
    public string RegulationName { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> RequirementsMet { get; set; } = new();
    public List<string> RequirementsNotMet { get; set; } = new();
    public List<string> MitigationsMissing { get; set; } = new();
}

#endregion
