namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Copilot performance metrics and monitoring.
/// </summary>
public class CopilotPerformanceMetrics
{
    public string MetricsId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Total conversations/interactions</summary>
    public long TotalInteractions { get; set; }

    /// <summary>Successful completions</summary>
    public long SuccessfulCompletions { get; set; }

    /// <summary>Failed interactions</summary>
    public long FailedInteractions { get; set; }

    /// <summary>Overall success rate (%)</summary>
    public decimal SuccessRate { get; set; }

    /// <summary>Average response time (ms)</summary>
    public decimal AvgResponseTimeMs { get; set; }

    /// <summary>P95 response time (ms)</summary>
    public decimal P95ResponseTimeMs { get; set; }

    /// <summary>Active users (last 30 days)</summary>
    public int ActiveUsers { get; set; }

    /// <summary>Average session duration (minutes)</summary>
    public decimal AvgSessionDurationMinutes { get; set; }

    /// <summary>User satisfaction score (0-5 or 0-100)</summary>
    public decimal UserSatisfactionScore { get; set; }

    /// <summary>Knowledge tool hit rate (%)</summary>
    public decimal KnowledgeHitRate { get; set; }

    /// <summary>Action tool success rate (%)</summary>
    public decimal ActionSuccessRate { get; set; }

    /// <summary>Evaluation pass rate (%)</summary>
    public decimal EvaluationPassRate { get; set; }

    /// <summary>Uptime percentage</summary>
    public decimal UptimePercentage { get; set; }

    /// <summary>Tokens used (for LLM-based copilots)</summary>
    public long TokensUsed { get; set; }

    /// <summary>Cost per interaction</summary>
    public decimal CostPerInteraction { get; set; }

    /// <summary>Token efficiency (tasks completed per token)</summary>
    public decimal TokenEfficiency { get; set; }

    /// <summary>Last updated date</summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>Measurement period (days)</summary>
    public int MeasurementPeriodDays { get; set; } = 30;
}

/// <summary>
/// Copilot deployment configuration.
/// </summary>
public class CopilotDeploymentConfig
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Current environment</summary>
    public string Environment { get; set; }

    /// <summary>Deployment strategy (BlueGreen, Canary, RollingUpdate)</summary>
    public string DeploymentStrategy { get; set; }

    /// <summary>Canary deployment percentage (if canary)</summary>
    public int CanaryPercentageUsers { get; set; }

    /// <summary>Canary duration before full rollout (hours)</summary>
    public int CanaryDurationHours { get; set; }

    /// <summary>Deployment frequency</summary>
    public string DeploymentFrequency { get; set; } // OnDemand, Daily, Weekly, Monthly

    /// <summary>Maintenance window</summary>
    public string MaintenanceWindow { get; set; } // e.g., "Sunday 2:00-4:00 AM UTC"

    /// <summary>Auto-scaling enabled</summary>
    public bool AutoScalingEnabled { get; set; }

    /// <summary>Min instances</summary>
    public int MinInstances { get; set; } = 1;

    /// <summary>Max instances</summary>
    public int MaxInstances { get; set; } = 10;

    /// <summary>Load distribution</summary>
    public string LoadDistribution { get; set; } // RoundRobin, LeastConnections, IPHash

    /// <summary>Rollback capability enabled</summary>
    public bool RollbackCapabilityEnabled { get; set; } = true;

    /// <summary>Health check configuration</summary>
    public HealthCheckConfig HealthCheck { get; set; }

    /// <summary>Zero-downtime deployment enabled</summary>
    public bool ZeroDowntimeEnabled { get; set; }

    /// <summary>Blue-green switch over timeout (minutes)</summary>
    public int SwitchoverTimeoutMinutes { get; set; } = 30;
}

/// <summary>
/// Health check configuration.
/// </summary>
public class HealthCheckConfig
{
    /// <summary>Health check enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Check interval (seconds)</summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>Timeout (seconds)</summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>Healthy threshold (consecutive passes)</summary>
    public int HealthyThreshold { get; set; } = 2;

    /// <summary>Unhealthy threshold (consecutive failures)</summary>
    public int UnhealthyThreshold { get; set; } = 3;

    /// <summary>Health check endpoint</summary>
    public string HealthCheckEndpoint { get; set; }
}

/// <summary>
/// Copilot version tracking.
/// </summary>
public class CopilotVersion
{
    public string VersionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Version number (semver)</summary>
    public string VersionNumber { get; set; }

    /// <summary>Release type (Major, Minor, Patch, Beta, RC)</summary>
    public string ReleaseType { get; set; }

    /// <summary>Release date</summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>Release notes</summary>
    public string ReleaseNotes { get; set; }

    /// <summary>Changes/features in this version</summary>
    public List<VersionChange> Changes { get; set; } = new();

    /// <summary>Breaking changes</summary>
    public List<string> BreakingChanges { get; set; } = new();

    /// <summary>Deprecations in this version</summary>
    public List<string> Deprecations { get; set; } = new();

    /// <summary>Migration guide URL</summary>
    public string MigrationGuideUrl { get; set; }

    /// <summary>Performance metrics for this version</summary>
    public CopilotPerformanceMetrics VersionMetrics { get; set; }

    /// <summary>Deployment status</summary>
    public string DeploymentStatus { get; set; } // Development, Staging, Production, Archived

    /// <summary>Rollback capability</summary>
    public string RollbackPath { get; set; } // Reference to previous version

    /// <summary>Support end date</summary>
    public DateTime? SupportEndDate { get; set; }
}

public class VersionChange
{
    public string ChangeId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } // Feature, Bugfix, Enhancement, Security
    public string Description { get; set; }
    public string ComponentAffected { get; set; } // KnowledgeTool, ActionTool, etc.
}

/// <summary>
/// Guidelines adherence tracking and compliance.
/// </summary>
public class GuidelinesAdherence
{
    public string AdherenceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Development guidelines checklist</summary>
    public List<GuidelineChecklistItem> DevelopmentChecklist { get; set; } = new();

    /// <summary>Security guidelines compliance</summary>
    public List<GuidelineChecklistItem> SecurityChecklist { get; set; } = new();

    /// <summary>Performance guidelines compliance</summary>
    public List<GuidelineChecklistItem> PerformanceChecklist { get; set; } = new();

    /// <summary>Data governance guidelines compliance</summary>
    public List<GuidelineChecklistItem> DataGovernanceChecklist { get; set; } = new();

    /// <summary>Overall adherence score (%)</summary>
    public decimal OverallAdherenceScore { get; set; }

    /// <summary>Last assessment date</summary>
    public DateTime LastAssessmentDate { get; set; }

    /// <summary>Next assessment date</summary>
    public DateTime NextAssessmentDate { get; set; }

    /// <summary>Deviations documented</summary>
    public List<GuidelineDeviation> ApprovedDeviations { get; set; } = new();

    /// <summary>Assessment notes</summary>
    public string AssessmentNotes { get; set; }
}

/// <summary>
/// Guideline checklist item.
/// </summary>
public class GuidelineChecklistItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Guideline reference</summary>
    public string GuidelineReference { get; set; }

    /// <summary>Guideline description</summary>
    public string Description { get; set; }

    /// <summary>Is compliant</summary>
    public bool IsCompliant { get; set; }

    /// <summary>Evidence of compliance</summary>
    public string ComplianceEvidence { get; set; }

    /// <summary>Remediation if not compliant</summary>
    public string NeededRemediation { get; set; }

    /// <summary>Remediation owner</summary>
    public string RemediationOwner { get; set; }

    /// <summary>Target remediation date</summary>
    public DateTime? RemediationTargetDate { get; set; }
}

/// <summary>
/// Risk/deviation from guidelines.
/// </summary>
public class GuidelineDeviation
{
    public string DeviationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Guideline being deviated from</summary>
    public string GuidelineReference { get; set; }

    /// <summary>Reason for deviation</summary>
    public string Reason { get; set; }

    /// <summary>Business justification</summary>
    public string BusinessJustification { get; set; }

    /// <summary>Risk impact assessment</summary>
    public string RiskAssessment { get; set; }

    /// <summary>Mitigation measures</summary>
    public List<string> MitigationMeasures { get; set; } = new();

    /// <summary>Approval status</summary>
    public string ApprovalStatus { get; set; } // Pending, Approved, Rejected, Expired

    /// <summary>Approved by</summary>
    public string ApprovedBy { get; set; }

    /// <summary>Approval date</summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>Expiration date</summary>
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// Development guidelines and best practices.
/// </summary>
public class DevelopmentGuidelines
{
    public string GuidelineId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Guideline version</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>Effective date</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

    /// <summary>Knowledge tool development standards</summary>
    public KnowledgeToolGuidelines KnowledgeToolGuidelines { get; set; }

    /// <summary>Action tool development standards</summary>
    public ActionToolGuidelines ActionToolGuidelines { get; set; }

    /// <summary>Trigger configuration standards</summary>
    public TriggerGuidelines TriggerGuidelines { get; set; }

    /// <summary>Evaluation configuration standards</summary>
    public EvaluationGuidelines EvaluationGuidelines { get; set; }

    /// <summary>Testing standards</summary>
    public TestingGuidelines TestingGuidelines { get; set; }

    /// <summary>Security standards</summary>
    public SecurityGuidelines SecurityGuidelines { get; set; }

    /// <summary>Performance standards</summary>
    public PerformanceGuidelines PerformanceGuidelines { get; set; }

    /// <summary>Documentation standards</summary>
    public DocumentationGuidelines DocumentationGuidelines { get; set; }
}

/// <summary>
/// Knowledge tool development guidelines.
/// </summary>
public class KnowledgeToolGuidelines
{
    /// <summary>Minimum retrieval accuracy</summary>
    public decimal MinimumAccuracy { get; set; } = 0.85m;

    /// <summary>Maximum response latency (ms)</summary>
    public int MaxLatencyMs { get; set; } = 2000;

    /// <summary>Cache hit ratio target</summary>
    public decimal TargetCacheHitRatio { get; set; } = 0.7m;

    /// <summary>Required embedding quality score</summary>
    public int MinEmbeddingQualityScore { get; set; } = 80;

    /// <summary>Data freshness requirement (hours)</summary>
    public int MaxDataStalenessHours { get; set; } = 24;

    /// <summary>Minimum training data size</summary>
    public int MinTrainingDataSize { get; set; } = 100;

    /// <summary>Required source attribution</summary>
    public bool RequireSourceAttribution { get; set; } = true;

    /// <summary>Maximum results to return</summary>
    public int MaxResultsReturned { get; set; } = 10;
}

/// <summary>
/// Action tool development guidelines.
/// </summary>
public class ActionToolGuidelines
{
    /// <summary>Maximum execution timeout (ms)</summary>
    public int MaxTimeoutMs { get; set; } = 30000;

    /// <summary>Required retry logic</summary>
    public bool RequireRetryLogic { get; set; } = true;

    /// <summary>Required error handling</summary>
    public bool RequireErrorHandling { get; set; } = true;

    /// <summary>Required circuit breaker</summary>
    public bool RequireCircuitBreaker { get; set; } = true;

    /// <summary>Maximum failure rate acceptable (%)</summary>
    public decimal MaxFailureRate { get; set; } = 0.01m; // 1%

    /// <summary>Idempotency required</summary>
    public bool RequireIdempotency { get; set; } = true;

    /// <summary>Audit trail mandatory</summary>
    public bool RequireAuditTrail { get; set; } = true;

    /// <summary>Required for sensitive operations</summary>
    public bool RequireApprovalWorkflow { get; set; } = true;

    /// <summary>Rollback capability required</summary>
    public bool RequireRollbackCapability { get; set; } = true;

    /// <summary>Maximum concurrent requests</summary>
    public int MaxConcurrentRequests { get; set; } = 100;
}

/// <summary>
/// Trigger configuration guidelines.
/// </summary>
public class TriggerGuidelines
{
    /// <summary>Maximum trigger frequency (per minute)</summary>
    public int MaxTriggerFrequency { get; set; } = 1000;

    /// <summary>Scheduled trigger minimum interval (minutes)</summary>
    public int MinScheduleInterval { get; set; } = 5;

    /// <summary>Event-based trigger max latency (seconds)</summary>
    public int MaxEventLatency { get; set; } = 60;

    /// <summary>Webhook timeout (seconds)</summary>
    public int WebhookTimeoutSeconds { get; set; } = 30;

    /// <summary>Webhook retry attempts</summary>
    public int WebhookMaxRetries { get; set; } = 3;

    /// <summary>Dead letter queue required</summary>
    public bool RequireDeadLetterQueue { get; set; } = true;

    /// <summary>Monitoring and alerting required</summary>
    public bool RequireMonitoring { get; set; } = true;

    /// <summary>Trigger audit logging required</summary>
    public bool RequireAuditLogging { get; set; } = true;
}

/// <summary>
/// Evaluation configuration guidelines.
/// </summary>
public class EvaluationGuidelines
{
    /// <summary>Minimum evaluation coverage (%)</summary>
    public decimal MinimumCoverage { get; set; } = 0.80m;

    /// <summary>Evaluation sample size minimum</summary>
    public int MinSampleSize { get; set; } = 100;

    /// <summary>Maximum evaluation latency (ms)</summary>
    public int MaxEvaluationLatency { get; set; } = 1000;

    /// <summary>Evaluation frequency minimum</summary>
    public string MinEvaluationFrequency { get; set; } = "Daily"; // Hourly, Daily, Weekly

    /// <summary>Pass threshold minimum</summary>
    public decimal MinPassThreshold { get; set; } = 0.70m;

    /// <summary>Warning threshold required</summary>
    public bool RequireWarningThreshold { get; set; } = true;

    /// <summary>Failed evaluation alerting required</summary>
    public bool RequireAlertingOnFailure { get; set; } = true;

    /// <summary>Evaluation result audit trail</summary>
    public bool RequireResultsAuditTrail { get; set; } = true;

    /// <summary>A/B testing recommended</summary>
    public bool RecommendABTesting { get; set; } = true;
}

/// <summary>
/// Testing guidelines for copilots.
/// </summary>
public class TestingGuidelines
{
    /// <summary>Unit test coverage required (%)</summary>
    public int MinUnitTestCoverage { get; set; } = 80;

    /// <summary>Integration test required</summary>
    public bool RequireIntegrationTesting { get; set; } = true;

    /// <summary>End-to-end test required</summary>
    public bool RequireE2ETesting { get; set; } = true;

    /// <summary>User acceptance test required</summary>
    public bool RequireUAT { get; set; } = true;

    /// <summary>Security testing required</summary>
    public bool RequireSecurityTesting { get; set; } = true;

    /// <summary>Performance testing required</summary>
    public bool RequirePerformanceTesting { get; set; } = true;

    /// <summary>Stress testing required</summary>
    public bool RequireStressTesting { get; set; } = false;

    /// <summary>Regression testing required</summary>
    public bool RequireRegressionTesting { get; set; } = true;

    /// <summary>Test automation level</summary>
    public string TestAutomationLevel { get; set; } = "High"; // High, Medium, Low

    /// <summary>Test data requirements</summary>
    public TestDataRequirements TestDataReqs { get; set; }
}

public class TestDataRequirements
{
    public int MinPositiveTestCases { get; set; } = 50;
    public int MinNegativeTestCases { get; set; } = 50;
    public int MinEdgeCaseTestCases { get; set; } = 25;
    public bool RequirePIITestData { get; set; }
    public bool RequireAnonymizedTestData { get; set; } = true;
    public int TestDataRefreshFrequencyDays { get; set; } = 30;
}

/// <summary>
/// Security guidelines for copilot development.
/// </summary>
public class SecurityGuidelines
{
    /// <summary>Code review required before deployment</summary>
    public bool RequireCodeReview { get; set; } = true;

    /// <summary>Minimum reviewers</summary>
    public int MinimumReviewers { get; set; } = 2;

    /// <summary>Security review required</summary>
    public bool RequireSecurityReview { get; set; } = true;

    /// <summary>SAST (Static Application Security Testing) required</summary>
    public bool RequireSAST { get; set; } = true;

    /// <summary>Dependency scanning required</summary>
    public bool RequireDependencyScanning { get; set; } = true;

    /// <summary>Secrets scanning required</summary>
    public bool RequireSecretsScanning { get; set; } = true;

    /// <summary>Penetration testing for production</summary>
    public bool RequirePenetrationTesting { get; set; } = true;

    /// <summary>Encryption of data in transit</summary>
    public string EncryptionInTransitStandard { get; set; } = "TLS 1.2+";

    /// <summary>Encryption at rest required</summary>
    public bool RequireEncryptionAtRest { get; set; } = true;

    /// <summary>MFA for admin access</summary>
    public bool RequireMFAForAdmins { get; set; } = true;

    /// <summary>Rate limiting required</summary>
    public bool RequireRateLimiting { get; set; } = true;

    /// <summary>Input validation required</summary>
    public bool RequireInputValidation { get; set; } = true;

    /// <summary>Prompt injection filtering</summary>
    public bool RequirePromptInjectionFiltering { get; set; } = true;
}

/// <summary>
/// Performance guidelines for copilot development.
/// </summary>
public class PerformanceGuidelines
{
    /// <summary>Maximum response time (ms)</summary>
    public int MaxResponseTime { get; set; } = 3000;

    /// <summary>Target availability (%)</summary>
    public decimal TargetAvailability { get; set; } = 0.99m; // 99%

    /// <summary>Throughput requirement (requests/second)</summary>
    public int MinThroughput { get; set; } = 100;

    /// <summary>Concurrent users support</summary>
    public int TargetConcurrentUsers { get; set; } = 1000;

    /// <summary>Memory limit per instance (MB)</summary>
    public int MemoryLimitMB { get; set; } = 2048;

    /// <summary>CPU limit per instance (cores)</summary>
    public decimal CPULimitCores { get; set; } = 2.0m;

    /// <summary>Caching recommended</summary>
    public bool RecommendCaching { get; set; } = true;

    /// <summary>CDN recommended for static content</summary>
    public bool RecommendCDN { get; set; } = true;

    /// <summary>Database query optimization required</summary>
    public bool RequireQueryOptimization { get; set; } = true;

    /// <summary>Maximum database connections</summary>
    public int MaxDBConnections { get; set; } = 100;

    /// <summary>Connection pooling required</summary>
    public bool RequireConnectionPooling { get; set; } = true;
}

/// <summary>
/// Documentation guidelines for copilot development.
/// </summary>
public class DocumentationGuidelines
{
    /// <summary>README file required</summary>
    public bool RequireREADME { get; set; } = true;

    /// <summary>API documentation required</summary>
    public bool RequireAPIDocumentation { get; set; } = true;

    /// <summary>Code comments required</summary>
    public bool RequireCodeComments { get; set; } = true;

    /// <summary>Minimum code comment ratio (%)</summary>
    public int MinCommentRatio { get; set; } = 20;

    /// <summary>Architecture documentation required</summary>
    public bool RequireArchitectureDoc { get; set; } = true;

    /// <summary>Runbook/Operational guide required</summary>
    public bool RequireRunbook { get; set; } = true;

    /// <summary>Deployment guide required</summary>
    public bool RequireDeploymentGuide { get; set; } = true;

    /// <summary>Troubleshooting guide required</summary>
    public bool RequireTroubleshootingGuide { get; set; } = true;

    /// <summary>Change log required</summary>
    public bool RequireChangelog { get; set; } = true;

    /// <summary>Examples/samples required</summary>
    public bool RequireExamples { get; set; } = true;

    /// <summary>Video tutorials recommended</summary>
    public bool RecommendVideoTutorials { get; set; } = false;
}
