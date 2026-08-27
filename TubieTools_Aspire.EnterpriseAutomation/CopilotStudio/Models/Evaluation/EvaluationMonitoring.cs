namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.Evaluation;

/// <summary>
/// Monitoring configuration for evaluations.
/// </summary>
public class EvaluationMonitoring
{
    /// <summary>Track all evaluation runs</summary>
    public bool TrackAllRuns { get; set; } = true;

    /// <summary>Evaluation run frequency for reporting</summary>
    public string ReportingFrequency { get; set; } = "Daily"; // Hourly, Daily, Weekly, Monthly

    /// <summary>Trend analysis enabled</summary>
    public bool EnableTrendAnalysis { get; set; }

    /// <summary>Anomaly detection on evaluation scores</summary>
    public bool EnableAnomalyDetection { get; set; }

    /// <summary>SLA for evaluation pass rate (%)</summary>
    public decimal SLAPassRateTarget { get; set; } = 0.95m;

    /// <summary>Custom dashboard/report configuration</summary>
    public string DashboardConfig { get; set; }
}
