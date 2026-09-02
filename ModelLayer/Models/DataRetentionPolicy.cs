namespace ModelLayer.Models;

/// <summary>
/// Data retention policies.
/// </summary>
public class DataRetentionPolicy
{
    /// <summary>Default retention period (days)</summary>
    public int DefaultRetentionDays { get; set; }

    /// <summary>Maximum retention period (days)</summary>
    public int MaxRetentionDays { get; set; }

    /// <summary>Archive after N days</summary>
    public int ArchiveAfterDays { get; set; }

    /// <summary>Deletion method (Soft, Hard, Shred)</summary>
    public string DeletionMethod { get; set; }

    /// <summary>Deletion confirmation required</summary>
    public bool DeletionConfirmationRequired { get; set; }

    /// <summary>Backup retention</summary>
    public int BackupRetentionDays { get; set; }
}
