namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Monitoring and logging configuration.
/// </summary>
public class MonitoringConfiguration
{
    /// <summary>Monitoring platform</summary>
    public string MonitoringPlatform { get; set; }

    /// <summary>Log aggregation enabled</summary>
    public bool LogAggregationEnabled { get; set; }

    /// <summary>Metrics collection frequency</summary>
    public string MetricsFrequency { get; set; }

    /// <summary>Alert thresholds</summary>
    public List<AlertThreshold> AlertThresholds { get; set; } = new();

    /// <summary>Incident response automation</summary>
    public bool IncidentAutomationEnabled { get; set; }
}
