namespace ModelLayer.Models.Action;

/// <summary>
/// Represents an Action Tool in the Copilot.
/// Used for executing tasks and modifying data.
/// </summary>
public class ActionTool
{
    public string ToolId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Tool name</summary>
    public string Name { get; set; }

    /// <summary>Tool description</summary>
    public string Description { get; set; }

    /// <summary>Action tool pattern (REST API, Database, File, Notification, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Integration configuration</summary>
    public IntegrationConfig Integration { get; set; }

    /// <summary>Action parameters and schema</summary>
    public ActionSchema Schema { get; set; }

    /// <summary>Error handling strategy</summary>
    public ErrorHandlingConfig ErrorHandling { get; set; }

    /// <summary>Retry configuration</summary>
    public RetryConfig RetryConfig { get; set; }

    /// <summary>Timeout (milliseconds)</summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>Requires approval before execution</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Approval rules</summary>
    public ApprovalRules ApprovalRules { get; set; }

    /// <summary>Access control</summary>
    public ToolAccessControl AccessControl { get; set; }

    /// <summary>Audit trail of all executions</summary>
    public bool EnableAuditTrail { get; set; } = true;

    /// <summary>Rollback capability</summary>
    public bool SupportsRollback { get; set; }

    /// <summary>Tool is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Performance metrics</summary>
    public ActionToolMetrics Metrics { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}
