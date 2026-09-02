namespace ModelLayer.Models;

/// <summary>
/// Trigger metrics.
/// </summary>
public class TriggerMetrics
{
    /// <summary>Total triggers fired</summary>
    public long TotalFired { get; set; }

    /// <summary>Successful executions</summary>
    public long Succeeded { get; set; }

    /// <summary>Failed executions</summary>
    public long Failed { get; set; }

    /// <summary>Average execution time (ms)</summary>
    public decimal AvgExecutionTimeMs { get; set; }

    /// <summary>Last fired date/time</summary>
    public DateTime? LastFiredDateTime { get; set; }
}
