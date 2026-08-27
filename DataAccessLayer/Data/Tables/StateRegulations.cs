namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using System.Collections.Generic;

/// <summary>
/// State-specific regulations
/// </summary>
public class StateRegulations
{
    public string StateCode { get; set; } = string.Empty;
    public Dictionary<string, object> DataRetentionPolicies { get; set; } = [];
    public Dictionary<string, object> ComplianceRequirements { get; set; } = [];
    public Dictionary<string, object> SecurityStandards { get; set; } = [];
    public Dictionary<string, bool> RestrictedFeatures { get; set; } = [];
}
