namespace ModelLayer.Models;

/// <summary>
/// Audit and logging requirements.
/// </summary>
public class AuditRequirements
{
    /// <summary>Audit logging enabled</summary>
    public bool AuditLoggingEnabled { get; set; } = true;

    /// <summary>Events to log (Execution, Configuration, Access, etc.)</summary>
    public List<string> LoggedEvents { get; set; } = new();

    /// <summary>Audit log retention period (days)</summary>
    public int AuditLogRetentionDays { get; set; } = 90;

    /// <summary>Real-time log export to SIEM</summary>
    public bool RealTimeLogExport { get; set; }

    /// <summary>SIEM tool name</summary>
    public string SIEMTool { get; set; }

    /// <summary>Log immutability required</summary>
    public bool LogImmutabilityRequired { get; set; }

    /// <summary>Chain of custody for audit logs</summary>
    public bool ChainOfCustodyRequired { get; set; }

    /// <summary>Regular audit reviews</summary>
    public string AuditReviewFrequency { get; set; } // Weekly, Monthly, Quarterly

    /// <summary>Audit trail for tool modifications</summary>
    public bool ToolModificationAuditRequired { get; set; }

    /// <summary>User action audit trail</summary>
    public bool UserActionAuditRequired { get; set; }

    /// <summary>Data access audit trail</summary>
    public bool DataAccessAuditRequired { get; set; }
}
