namespace TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Represents operational metrics and monitoring for an AI agent.
/// Aligns with CAF "Manage/Operate Agents" lifecycle phase.
/// </summary>
public class OperationalMetrics
{
    public string MetricsId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Agent status (Running, Stopped, Error, Degraded)</summary>
    public string AgentStatus { get; set; }

    /// <summary>Uptime percentage (last 30 days)</summary>
    public decimal UptimePercentage { get; set; }

    /// <summary>Total invocations</summary>
    public long TotalInvocations { get; set; }

    /// <summary>Successful invocations</summary>
    public long SuccessfulInvocations { get; set; }

    /// <summary>Failed invocations</summary>
    public long FailedInvocations { get; set; }

    /// <summary>Average response time (milliseconds)</summary>
    public decimal AvgResponseTimeMs { get; set; }

    /// <summary>P95 response time (milliseconds)</summary>
    public decimal P95ResponseTimeMs { get; set; }

    /// <summary>Peak response time (milliseconds)</summary>
    public decimal PeakResponseTimeMs { get; set; }

    /// <summary>Error rate (%)</summary>
    public decimal ErrorRate { get; set; }

    /// <summary>Resource consumption tracking</summary>
    public ResourceConsumption ResourceConsumption { get; set; }

    /// <summary>Cost tracking (monthly usage costs)</summary>
    public CostTracking CostTracking { get; set; }

    /// <summary>Real-time alerts configuration</summary>
    public List<AlertConfiguration> Alerts { get; set; } = new();

    /// <summary>Recent incidents/errors</summary>
    public List<OperationalIncident> RecentIncidents { get; set; } = new();

    /// <summary>Monitoring collected metrics by time</summary>
    public List<MetricsSnapshot> MetricsHistory { get; set; } = new();

    /// <summary>Last updated timestamp</summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>Data collection period (days)</summary>
    public int CollectionPeriodDays { get; set; } = 30;
}

/// <summary>
/// Resource consumption metrics for operational tracking.
/// </summary>
public class ResourceConsumption
{
    public string ConsumptionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>CPU usage (cores or percentage)</summary>
    public string CPUUsage { get; set; }

    /// <summary>Memory usage (GB or percentage)</summary>
    public string MemoryUsage { get; set; }

    /// <summary>Storage usage (GB)</summary>
    public string StorageUsage { get; set; }

    /// <summary>Network bandwidth used (GB/month)</summary>
    public decimal NetworkBandwidthGBPerMonth { get; set; }

    /// <summary>API calls made to third-party services</summary>
    public int APICallsCount { get; set; }

    /// <summary>Database queries executed</summary>
    public long DatabaseQueriesCount { get; set; }

    /// <summary>Cache hit rate (%)</summary>
    public decimal CacheHitRate { get; set; }

    /// <summary>GPU utilization (if applicable)</summary>
    public string GPUUtilization { get; set; }

    /// <summary>Instances/replicas running</summary>
    public int RunningInstances { get; set; }

    /// <summary>Auto-scaling metrics</summary>
    public AutoScalingMetrics AutoScalingMetrics { get; set; }

    /// <summary>Last measurement date</summary>
    public DateTime MeasurementDate { get; set; }
}

/// <summary>
/// Auto-scaling metrics.
/// </summary>
public class AutoScalingMetrics
{
    /// <summary>Auto-scaling enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Minimum replicas</summary>
    public int MinReplicas { get; set; }

    /// <summary>Maximum replicas</summary>
    public int MaxReplicas { get; set; }

    /// <summary>Current replicas running</summary>
    public int CurrentReplicas { get; set; }

    /// <summary>CPU scaling threshold (%)</summary>
    public int CPUScalingThreshold { get; set; }

    /// <summary>Memory scaling threshold (%)</summary>
    public int MemoryScalingThreshold { get; set; }

    /// <summary>Scale-out events (last 30 days)</summary>
    public int ScaleOutEvents { get; set; }

    /// <summary>Scale-in events (last 30 days)</summary>
    public int ScaleInEvents { get; set; }
}

/// <summary>
/// Cost tracking for agent operations.
/// </summary>
public class CostTracking
{
    public string CostId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Compute costs (monthly)</summary>
    public decimal ComputeCostsMonthly { get; set; }

    /// <summary>Storage costs (monthly)</summary>
    public decimal StorageCostsMonthly { get; set; }

    /// <summary>API/Third-party service costs (monthly)</summary>
    public decimal APIServiceCostsMonthly { get; set; }

    /// <summary>Data transfer costs (monthly)</summary>
    public decimal DataTransferCostsMonthly { get; set; }

    /// <summary>Licensing costs (monthly)</summary>
    public decimal LicensingCostsMonthly { get; set; }

    /// <summary>Total monthly cost</summary>
    public decimal TotalMonthlyCost { get; set; }

    /// <summary>Total YTD cost</summary>
    public decimal TotalYTDCost { get; set; }

    /// <summary>Budget allocated (monthly)</summary>
    public decimal MonthlyBudget { get; set; }

    /// <summary>Budget remaining this month</summary>
    public decimal BudgetRemaining { get; set; }

    /// <summary>Cost per invocation</summary>
    public decimal CostPerInvocation { get; set; }

    /// <summary>Cost trend (increasing, stable, decreasing)</summary>
    public string CostTrend { get; set; }

    /// <summary>Cost optimization recommendations</summary>
    public List<string> OptimizationRecommendations { get; set; } = new();

    /// <summary>Currency</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Last cost update date</summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Alert configuration for operational monitoring.
/// </summary>
public class AlertConfiguration
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Alert name</summary>
    public string AlertName { get; set; }

    /// <summary>Metric being monitored</summary>
    public string MetricName { get; set; }

    /// <summary>Alert threshold/condition</summary>
    public string Condition { get; set; }

    /// <summary>Alert severity (Critical, High, Medium, Low, Info)</summary>
    public string Severity { get; set; }

    /// <summary>Alert is enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Channels to notify (Email, Slack, Teams, PagerDuty, etc.)</summary>
    public List<string> NotificationChannels { get; set; } = new();

    /// <summary>Recipients (emails or user IDs)</summary>
    public List<string> Recipients { get; set; } = new();

    /// <summary>Alert cooldown period (minutes)</summary>
    public int CooldownMinutes { get; set; }

    /// <summary>Escalation policy name (if applicable)</summary>
    public string EscalationPolicy { get; set; }

    /// <summary>Escalation time (minutes before escalating)</summary>
    public int EscalationTimeMinutes { get; set; }

    /// <summary>Runbook reference (documentation for resolving alert)</summary>
    public string RunbookReference { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Operational incident record.
/// </summary>
public class OperationalIncident
{
    public string IncidentId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Incident type (Error, Performance, Security, Data, etc.)</summary>
    public string IncidentType { get; set; }

    /// <summary>Severity level</summary>
    public string Severity { get; set; }

    /// <summary>Incident title</summary>
    public string Title { get; set; }

    /// <summary>Incident description</summary>
    public string Description { get; set; }

    /// <summary>Error message/stack trace</summary>
    public string ErrorDetails { get; set; }

    /// <summary>Affected users/systems</summary>
    public string AffectedScope { get; set; }

    /// <summary>Root cause analysis (after resolution)</summary>
    public string RootCauseAnalysis { get; set; }

    /// <summary>Incident status (Open, InProgress, Resolved, Closed)</summary>
    public string Status { get; set; }

    /// <summary>Incident detected date/time</summary>
    public DateTime DetectedDateTime { get; set; }

    /// <summary>Incident resolved date/time</summary>
    public DateTime? ResolvedDateTime { get; set; }

    /// <summary>Time to resolution (minutes)</summary>
    public int? TimeToResolutionMinutes { get; set; }

    /// <summary>Assigned to (engineer/team)</summary>
    public string AssignedTo { get; set; }

    /// <summary>Resolution steps taken</summary>
    public string ResolutionSteps { get; set; }

    /// <summary>Prevention measures to prevent recurrence</summary>
    public string PreventionMeasures { get; set; }

    /// <summary>Related change/deployment</summary>
    public string RelatedChangeId { get; set; }

    /// <summary>Post-mortem/blameless retrospective URL</summary>
    public string PostMortemUrl { get; set; }
}

/// <summary>
/// Point-in-time metrics snapshot.
/// </summary>
public class MetricsSnapshot
{
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Snapshot timestamp</summary>
    public DateTime SnapshotTime { get; set; }

    /// <summary>Invocation count in period</summary>
    public long InvocationCount { get; set; }

    /// <summary>Success count in period</summary>
    public long SuccessCount { get; set; }

    /// <summary>Error count in period</summary>
    public long ErrorCount { get; set; }

    /// <summary>Average response time in period (ms)</summary>
    public decimal AvgResponseTimeMs { get; set; }

    /// <summary>Error rate in period (%)</summary>
    public decimal ErrorRate { get; set; }

    /// <summary>CPU usage in period (%)</summary>
    public decimal CPUPercentage { get; set; }

    /// <summary>Memory usage in period (%)</summary>
    public decimal MemoryPercentage { get; set; }

    /// <summary>Cost incurred in period</summary>
    public decimal CostIncurred { get; set; }
}

/// <summary>
/// Compliance record for audit and regulatory tracking.
/// </summary>
public class ComplianceRecord
{
    public string RecordId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Audit type (Scheduled, Incident, Regulatory, Internal)</summary>
    public string AuditType { get; set; }

    /// <summary>Audit date</summary>
    public DateTime AuditDate { get; set; }

    /// <summary>Auditor name/team</summary>
    public string Auditor { get; set; }

    /// <summary>Compliance framework (GDPR, HIPAA, SOC2, ISO27001, etc.)</summary>
    public string ComplianceFramework { get; set; }

    /// <summary>Audit findings</summary>
    public List<string> Findings { get; set; } = new();

    /// <summary>Non-compliances identified</summary>
    public List<string> NonCompliances { get; set; } = new();

    /// <summary>Audit result (Compliant, NonCompliant, PartiallyCompliant)</summary>
    public string Result { get; set; }

    /// <summary>Remediation actions required</summary>
    public List<RemediationAction> RemediationActions { get; set; } = new();

    /// <summary>Data processed (volume, classification)</summary>
    public string DataProcessedSummary { get; set; }

    /// <summary>Incidents during audit period</summary>
    public int IncidentsDuringPeriod { get; set; }

    /// <summary>Audit report location/URL</summary>
    public string AuditReportLocation { get; set; }

    /// <summary>Next audit date</summary>
    public DateTime NextAuditDate { get; set; }
}

/// <summary>
/// Remediation action for non-compliance.
/// </summary>
public class RemediationAction
{
    public string ActionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Non-compliance this addresses</summary>
    public string NonComplianceItem { get; set; }

    /// <summary>Action description</summary>
    public string Description { get; set; }

    /// <summary>Assigned to</summary>
    public string AssignedTo { get; set; }

    /// <summary>Due date for remediation</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Status (Open, InProgress, Completed, Overdue)</summary>
    public string Status { get; set; }

    /// <summary>Completion date</summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>Evidence of remediation</summary>
    public string EvidenceLocation { get; set; }

    /// <summary>Verification status (Verified, Pending, Rejected)</summary>
    public string VerificationStatus { get; set; }
}

/// <summary>
/// Model performance monitoring and drift detection.
/// </summary>
public class ModelMonitoring
{
    public string MonitoringId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Production model version</summary>
    public string ModelVersion { get; set; }

    /// <summary>Latest performance metrics</summary>
    public ModelPerformanceMetrics LatestPerformance { get; set; }

    /// <summary>Performance comparison with baseline</summary>
    public ModelPerformanceComparison PerformanceComparison { get; set; }

    /// <summary>Data drift detection results</summary>
    public DataDriftDetection DataDrift { get; set; }

    /// <summary>Concept drift detection results</summary>
    public ConceptDriftDetection ConceptDrift { get; set; }

    /// <summary>Feature importance over time</summary>
    public List<FeatureImportanceSnapshot> FeatureImportance { get; set; } = new();

    /// <summary>Model retraining recommendations</summary>
    public List<RetrainingRecommendation> RetrainingRecommendations { get; set; } = new();

    /// <summary>Last monitoring update date</summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Model performance metrics.
/// </summary>
public class ModelPerformanceMetrics
{
    public string MetricsId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Measurement date</summary>
    public DateTime MeasurementDate { get; set; }

    /// <summary>Accuracy metric</summary>
    public decimal Accuracy { get; set; }

    /// <summary>Precision metric</summary>
    public decimal Precision { get; set; }

    /// <summary>Recall metric</summary>
    public decimal Recall { get; set; }

    /// <summary>F1 score</summary>
    public decimal F1Score { get; set; }

    /// <summary>Specific metrics for use case</summary>
    public Dictionary<string, decimal> CustomMetrics { get; set; } = new();

    /// <summary>Sample size used for measurement</summary>
    public int SampleSize { get; set; }
}

/// <summary>
/// Comparison of current performance against baseline.
/// </summary>
public class ModelPerformanceComparison
{
    /// <summary>Baseline model version</summary>
    public string BaselineVersion { get; set; }

    /// <summary>Baseline metrics</summary>
    public ModelPerformanceMetrics BaselineMetrics { get; set; }

    /// <summary>Current metrics</summary>
    public ModelPerformanceMetrics CurrentMetrics { get; set; }

    /// <summary>Performance degradation detected</summary>
    public bool DegradationDetected { get; set; }

    /// <summary>Metrics that degraded</summary>
    public List<string> DegradedMetrics { get; set; } = new();

    /// <summary>Acceptable deviation threshold (%)</summary>
    public decimal AcceptableDeviation { get; set; }

    /// <summary>Retraining recommended</summary>
    public bool RetrainingRecommended { get; set; }
}

/// <summary>
/// Data drift detection results.
/// </summary>
public class DataDriftDetection
{
    /// <summary>Detection date</summary>
    public DateTime DetectionDate { get; set; }

    /// <summary>Drift detected</summary>
    public bool DriftDetected { get; set; }

    /// <summary>Drift severity (High, Medium, Low)</summary>
    public string Severity { get; set; }

    /// <summary>Features showing drift</summary>
    public List<FeatureDrift> FeaturesDrifting { get; set; } = new();

    /// <summary>Statistical test used (KL Divergence, Kolmogorov-Smirnov, etc.)</summary>
    public string TestMethod { get; set; }

    /// <summary>P-value from statistical test</summary>
    public decimal PValue { get; set; }

    /// <summary>Threshold used for detection</summary>
    public decimal Threshold { get; set; }

    /// <summary>Days until retraining recommended</summary>
    public int DaysUntilRetrain { get; set; }
}

/// <summary>
/// Feature drift information.
/// </summary>
public class FeatureDrift
{
    /// <summary>Feature name</summary>
    public string FeatureName { get; set; }

    /// <summary>Drift magnitude</summary>
    public decimal DriftMagnitude { get; set; }

    /// <summary>Statistical test p-value</summary>
    public decimal PValue { get; set; }

    /// <summary>Drift direction (Increasing, Decreasing, Volatility)</summary>
    public string DriftDirection { get; set; }
}

/// <summary>
/// Concept drift detection results.
/// </summary>
public class ConceptDriftDetection
{
    /// <summary>Detection date</summary>
    public DateTime DetectionDate { get; set; }

    /// <summary>Concept drift detected</summary>
    public bool DriftDetected { get; set; }

    /// <summary>Drift severity</summary>
    public string Severity { get; set; }

    /// <summary>Confidence level of detection (0-1)</summary>
    public decimal ConfidenceLevel { get; set; }

    /// <summary>Estimated impact on model performance</summary>
    public string EstimatedImpact { get; set; }

    /// <summary>Recommended action</summary>
    public string RecommendedAction { get; set; }
}

/// <summary>
/// Feature importance snapshot over time.
/// </summary>
public class FeatureImportanceSnapshot
{
    /// <summary>Snapshot date</summary>
    public DateTime SnapshotDate { get; set; }

    /// <summary>Feature name to importance score mapping</summary>
    public Dictionary<string, decimal> FeatureImportances { get; set; } = new();

    /// <summary>Top 10 most important features</summary>
    public List<string> TopFeatures { get; set; } = new();
}

/// <summary>
/// Recommendation to retrain the model.
/// </summary>
public class RetrainingRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Recommendation date</summary>
    public DateTime RecommendationDate { get; set; }

    /// <summary>Reason for retraining (DriftDetected, PerformanceDegradation, PolicyChange, etc.)</summary>
    public string Reason { get; set; }

    /// <summary>Urgency level</summary>
    public string Urgency { get; set; }

    /// <summary>Expected benefit from retraining</summary>
    public string ExpectedBenefit { get; set; }

    /// <summary>Estimated retraining effort (hours)</summary>
    public int EstimatedEffortHours { get; set; }

    /// <summary>Estimated cost</summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>Recommendation status (Pending, Approved, InProgress, Completed, Rejected)</summary>
    public string Status { get; set; }

    /// <summary>Approval date (if approved)</summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>Retraining start date (if in progress/completed)</summary>
    public DateTime? RetrainingStartDate { get; set; }

    /// <summary>Retraining completion date</summary>
    public DateTime? RetrainingCompletionDate { get; set; }
}

/// <summary>
/// Maintenance window record.
/// </summary>
public class MaintenanceWindow
{
    public string WindowId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Maintenance type (Patching, Update, Configuration, Cleanup)</summary>
    public string MaintenanceType { get; set; }

    /// <summary>Scheduled start time</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>Scheduled end time</summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>Maintenance description</summary>
    public string Description { get; set; }

    /// <summary>Expected impact on service</summary>
    public string ExpectedImpact { get; set; }

    /// <summary>Rollback plan</summary>
    public string RollbackPlan { get; set; }

    /// <summary>Status (Scheduled, InProgress, Completed, Postponed, Cancelled)</summary>
    public string Status { get; set; }

    /// <summary>Actual start time</summary>
    public DateTime? ActualStart { get; set; }

    /// <summary>Actual end time</summary>
    public DateTime? ActualEnd { get; set; }

    /// <summary>Maintenance performed by</summary>
    public string PerformedBy { get; set; }

    /// <summary>Approver of maintenance</summary>
    public string Approver { get; set; }
}
