namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

public class BackupConfiguration
{
    public bool BackupEnabled { get; set; }
    public string BackupFrequency { get; set; }
    public int RetentionDays { get; set; }
    public string BackupStorage { get; set; }
}
