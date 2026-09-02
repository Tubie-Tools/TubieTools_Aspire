namespace ModelLayer.Models;

/// <summary>
/// Approval rules for action tools.
/// </summary>
public class ApprovalRules
{
    /// <summary>Role required for approval</summary>
    public string ApprovalRole { get; set; }

    /// <summary>Condition for requiring approval (e.g., "amount > 1000")</summary>
    public string ApprovalCondition { get; set; }

    /// <summary>Notification channels for approval</summary>
    public List<string> NotificationChannels { get; set; } = new();

    /// <summary>Approval timeout (hours)</summary>
    public int ApprovalTimeoutHours { get; set; } = 24;

    /// <summary>Escalation if not approved within timeout</summary>
    public string EscalationAction { get; set; } = "Escalate"; // Escalate, Reject, Repeat
}
