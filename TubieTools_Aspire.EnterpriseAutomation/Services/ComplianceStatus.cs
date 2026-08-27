namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;
#region Supporting Models for Service Operations

/// <summary>
/// Compliance status check results
/// </summary>
public class ComplianceStatus
{
    public string AgentId { get; set; }
    public bool IsCompliant { get; set; }
    public List<GovernancePolicy> AppliedPolicies { get; set; } = new();
    public List<PolicyComplianceDetail> ComplianceDetails { get; set; } = new();
    public DateTime CheckDate { get; set; }
}

#endregion
