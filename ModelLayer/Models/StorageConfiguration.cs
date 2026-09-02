namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Storage configuration for landing zone.
/// </summary>
public class StorageConfiguration
{
    /// <summary>Storage account type</summary>
    public string StorageType { get; set; } // Azure, AWS, GCP, On-Premises

    /// <summary>Encryption type</summary>
    public string EncryptionType { get; set; } // ServiceManaged, CustomerManagedKey, BYOK

    /// <summary>Replication strategy</summary>
    public string ReplicationStrategy { get; set; } // LRS, ZRS, GRS, GZRS

    /// <summary>Retention policies</summary>
    public List<string> RetentionPolicies { get; set; } = new();

    /// <summary>Backup configuration</summary>
    public BackupConfiguration BackupConfig { get; set; }

    /// <summary>Data classification by storage tier</summary>
    public Dictionary<string, string> DataClassificationMapping { get; set; } = new();
}
