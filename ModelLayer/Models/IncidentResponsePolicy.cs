namespace ModelLayer.Models;

/// <summary>
/// Incident response and escalation policy.
/// </summary>
public class IncidentResponsePolicy
{
    /// <summary>Incident severity levels</summary>
    public List<SeverityLevel> SeverityLevels { get; set; } = new();

    /// <summary>Escalation contacts by severity</summary>
    public Dictionary<string, List<string>> EscalationContacts { get; set; } = new();

    /// <summary>Response time SLA by severity (minutes)</summary>
    public Dictionary<string, int> ResponseTimeSLAs { get; set; } = new();

    /// <summary>Resolution time SLA by severity (hours)</summary>
    public Dictionary<string, int> ResolutionTimeSLAs { get; set; } = new();

    /// <summary>Incident communication template</summary>
    public string CommunicationTemplate { get; set; }

    /// <summary>Post-incident review required</summary>
    public bool PostIncidentReviewRequired { get; set; }

    /// <summary>RCA (Root Cause Analysis) report timing</summary>
    public string RCAReportTiming { get; set; } // Same-Day, NextDay, Within3Days

    /// <summary>Incident tracking system</summary>
    public string IncidentTrackingSystem { get; set; }
}
