namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Interface for AI Agent Operational Management
/// </summary>
public interface IAgentOperationsService
{
    /// <summary>
    /// Records operational metrics
    /// </summary>
    Task<bool> RecordOperationalMetricsAsync(string agentId, MetricsSnapshot metrics);

    /// <summary>
    /// Gets current operational status
    /// </summary>
    Task<OperationalMetrics> GetOperationalMetricsAsync(string agentId);

    /// <summary>
    /// Reports an operational incident
    /// </summary>
    Task<OperationalIncident> ReportIncidentAsync(string agentId, OperationalIncident incident);

    /// <summary>
    /// Updates incident resolution
    /// </summary>
    Task<OperationalIncident> ResolveIncidentAsync(string incidentId, string resolution);

    /// <summary>
    /// Records model performance monitoring
    /// </summary>
    Task<bool> RecordModelMonitoringAsync(string agentId, ModelMonitoring monitoring);

    /// <summary>
    /// Detects model drift and recommends retraining
    /// </summary>
    Task<RetrainingRecommendation> DetectDriftAndRecommendRetrainingAsync(string agentId);

    /// <summary>
    /// Schedules maintenance window
    /// </summary>
    Task<MaintenanceWindow> ScheduleMaintenanceAsync(string agentId, MaintenanceWindow window);

    /// <summary>
    /// Gets SLA compliance report
    /// </summary>
    Task<SLAComplianceReport> GetSLAComplianceReportAsync(string agentId, int days = 30);
}
