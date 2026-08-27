namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Performance metrics for a knowledge tool.
/// </summary>
public class ToolPerformanceMetrics
{
    /// <summary>Average query latency (ms)</summary>
    public decimal AvgLatencyMs { get; set; }

    /// <summary>P95 latency (ms)</summary>
    public decimal P95LatencyMs { get; set; }

    /// <summary>Cache hit rate (%)</summary>
    public decimal CacheHitRate { get; set; }

    /// <summary>Query success rate (%)</summary>
    public decimal SuccessRate { get; set; }

    /// <summary>Total queries processed</summary>
    public long TotalQueriesProcessed { get; set; }

    /// <summary>Average relevance score of results</summary>
    public decimal AvgRelevanceScore { get; set; }

    /// <summary>Last measurement date</summary>
    public DateTime MeasurementDate { get; set; }
}
