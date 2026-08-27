namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Represents a Trigger/Event configuration.
/// </summary>
public class TriggerConfiguration
{
    public string TriggerId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Trigger name</summary>
    public string Name { get; set; }

    /// <summary>Trigger pattern (Scheduled, Webhook, Event, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Trigger configuration specific to pattern</summary>
    public TriggerDetails TriggerDetails { get; set; }

    /// <summary>Actions to execute when triggered</summary>
    public List<string> ActionIds { get; set; } = new();

    /// <summary>Workflow to execute</summary>
    public string WorkflowId { get; set; }

    /// <summary>Context/payload schema</summary>
    public string PayloadSchema { get; set; }

    /// <summary>Conditions for execution</summary>
    public string ExecutionCondition { get; set; }

    /// <summary>Is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Retry on failure</summary>
    public bool RetryOnFailure { get; set; } = true;

    /// <summary>Max retries</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Dead letter queue for failed triggers</summary>
    public string DeadLetterQueue { get; set; }

    /// <summary>Metrics</summary>
    public TriggerMetrics Metrics { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}
