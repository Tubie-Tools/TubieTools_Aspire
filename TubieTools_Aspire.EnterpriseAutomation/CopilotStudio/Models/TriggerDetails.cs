namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Trigger-specific configuration details.
/// </summary>
public class TriggerDetails
{
    /// <summary>Schedule expression (cron format for scheduled triggers)</summary>
    public string ScheduleExpression { get; set; }

    /// <summary>Webhook endpoint</summary>
    public string WebhookUrl { get; set; }

    /// <summary>Event queue name</summary>
    public string QueueName { get; set; }

    /// <summary>Database connection for change triggers</summary>
    public string DatabaseConnection { get; set; }

    /// <summary>Database table/object being monitored</summary>
    public string MonitoredObject { get; set; }

    /// <summary>Change detection type (INSERT, UPDATE, DELETE, ALL)</summary>
    public List<string> ChangeTypes { get; set; } = new();

    /// <summary>Frequency for polling (if applicable)</summary>
    public string PollingFrequency { get; set; } // Seconds, Minutes, Hours

    /// <summary>Notification/Webhook header authentication token</summary>
    public string AuthenticationToken { get; set; }
}
