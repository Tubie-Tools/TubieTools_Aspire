namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using System.Collections.Generic;

/// <summary>
/// State-specific features
/// </summary>
public class StateFeatures
{
    public string StateCode { get; set; } = string.Empty;
    public List<string> EnabledFeatures { get; set; } = [];
    public Dictionary<string, object> FeatureConfiguration { get; set; } = [];
    public int MaxTeamMembers { get; set; } = 50;
    public int MaxApiKeys { get; set; } = 10;
    public bool RequiresMFA { get; set; } = false;
    public bool RequiresDataEncryption { get; set; } = true;
}
