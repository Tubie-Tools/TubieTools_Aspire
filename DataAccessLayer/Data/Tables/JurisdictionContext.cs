namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

/// <summary>
/// Current jurisdiction context
/// </summary>
public class JurisdictionContext
{
    public string JurisdictionId { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string JurisdictionName { get; set; } = string.Empty;
    public StateRegulations? Regulations { get; set; }
    public StateFeatures? Features { get; set; }
    public string DatabaseSchema { get; set; } = string.Empty;
    public string ConnectionStringName { get; set; } = string.Empty;
}