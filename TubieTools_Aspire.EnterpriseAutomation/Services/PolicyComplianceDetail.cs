namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;
#region Supporting Models for Service Operations

public class PolicyComplianceDetail
{
    public string PolicyId { get; set; }
    public string PolicyName { get; set; }
    public bool IsCompliant { get; set; }
    public string NonComplianceReason { get; set; }
    public List<RemediationAction> RemediationActions { get; set; } = new();
}

#endregion
