namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.Action;

/// <summary>
/// Performance metrics for an action tool.
/// </summary>
public class ActionToolMetrics
{
    /// <summary>Total executions</summary>
    public long TotalExecutions { get; set; }

    /// <summary>Successful executions</summary>
    public long SuccessfulExecutions { get; set; }

    /// <summary>Failed executions</summary>
    public long FailedExecutions { get; set; }

    /// <summary>Average execution time (ms)</summary>
    public decimal AvgExecutionTimeMs { get; set; }

    /// <summary>P95 execution time (ms)</summary>
    public decimal P95ExecutionTimeMs { get; set; }

    /// <summary>Success rate (%)</summary>
    public decimal SuccessRate { get; set; }

    /// <summary>Last measurement date</summary>
    public DateTime MeasurementDate { get; set; }
}
