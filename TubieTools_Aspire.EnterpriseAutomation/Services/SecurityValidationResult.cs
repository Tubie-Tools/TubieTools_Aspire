namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Security validation results from testing
/// </summary>
public class SecurityValidationResult
{
    public string AgentId { get; set; }
    public bool PassedValidation { get; set; }
    public int CriticalVulnerabilities { get; set; }
    public int HighVulnerabilities { get; set; }
    public int MediumVulnerabilities { get; set; }
    public int LowVulnerabilities { get; set; }
    public List<Vulnerability> Vulnerabilities { get; set; } = new();
    public DateTime ValidationDate { get; set; }
}

#endregion
