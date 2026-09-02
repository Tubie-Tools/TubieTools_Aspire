namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Disaster recovery configuration.
/// </summary>
public class DRConfiguration
{
    /// <summary>Recovery time objective (hours)</summary>
    public int RTOHours { get; set; }

    /// <summary>Recovery point objective (minutes)</summary>
    public int RPOMinutes { get; set; }

    /// <summary>DR region</summary>
    public string DRRegion { get; set; }

    /// <summary>Backup frequency</summary>
    public string BackupFrequency { get; set; }

    /// <summary>Failover automation enabled</summary>
    public bool FailoverAutomationEnabled { get; set; }

    /// <summary>DR testing frequency</summary>
    public string DRTestingFrequency { get; set; }
}
