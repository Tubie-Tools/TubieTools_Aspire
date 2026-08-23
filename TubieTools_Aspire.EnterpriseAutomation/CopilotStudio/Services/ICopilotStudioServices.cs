namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Services;

using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Interface for Copilot Application lifecycle management.
/// </summary>
public interface ICopilotApplicationService
{
    /// <summary>
    /// Creates a new copilot application with configuration
    /// </summary>
    Task<CopilotApplication> CreateCopilotAsync(CopilotApplication copilot);

    /// <summary>
    /// Gets a copilot by ID
    /// </summary>
    Task<CopilotApplication> GetCopilotAsync(string copilotId);

    /// <summary>
    /// Lists copilots with optional filtering
    /// </summary>
    Task<IEnumerable<CopilotApplication>> ListCopilotsByLandingZoneAsync(string landingZone);

    /// <summary>
    /// Updates copilot configuration
    /// </summary>
    Task<CopilotApplication> UpdateCopilotAsync(string copilotId, CopilotApplication copilot);

    /// <summary>
    /// Deploys copilot to environment
    /// </summary>
    Task<DeploymentResult> DeployCopilotAsync(string copilotId, string environment);

    /// <summary>
    /// Gets deployment status
    /// </summary>
    Task<DeploymentStatus> GetDeploymentStatusAsync(string copilotId, string environment);

    /// <summary>
    /// Rolls back to previous version
    /// </summary>
    Task<bool> RollbackAsync(string copilotId, string toVersion);

    /// <summary>
    /// Gets performance metrics
    /// </summary>
    Task<CopilotPerformanceMetrics> GetPerformanceMetricsAsync(string copilotId);

    /// <summary>
    /// Deactivates a copilot
    /// </summary>
    Task<bool> DeactivateCopilotAsync(string copilotId);
}

/// <summary>
/// Interface for Knowledge Tool management.
/// </summary>
public interface IKnowledgeToolService
{
    /// <summary>
    /// Adds a new knowledge tool to copilot
    /// </summary>
    Task<KnowledgeTool> AddKnowledgeToolAsync(string copilotId, KnowledgeTool tool);

    /// <summary>
    /// Gets a knowledge tool by ID
    /// </summary>
    Task<KnowledgeTool> GetKnowledgeToolAsync(string toolId);

    /// <summary>
    /// Lists knowledge tools for a copilot
    /// </summary>
    Task<IEnumerable<KnowledgeTool>> ListKnowledgeToolsAsync(string copilotId);

    /// <summary>
    /// Updates knowledge tool configuration
    /// </summary>
    Task<KnowledgeTool> UpdateKnowledgeToolAsync(string toolId, KnowledgeTool tool);

    /// <summary>
    /// Tests knowledge tool connectivity and retrieval
    /// </summary>
    Task<ToolTestResult> TestKnowledgeToolAsync(string toolId);

    /// <summary>
    /// Optimizes knowledge tool performance
    /// </summary>
    Task<OptimizationRecommendations> GetOptimizationRecommendationsAsync(string toolId);

    /// <summary>
    /// Enables/disables knowledge tool
    /// </summary>
    Task<bool> SetToolEnabledAsync(string toolId, bool enabled);

    /// <summary>
    /// Validates data source connectivity
    /// </summary>
    Task<DataSourceValidationResult> ValidateDataSourceAsync(string toolId);
}

/// <summary>
/// Interface for Action Tool management.
/// </summary>
public interface IActionToolService
{
    /// <summary>
    /// Adds a new action tool to copilot
    /// </summary>
    Task<ActionTool> AddActionToolAsync(string copilotId, ActionTool tool);

    /// <summary>
    /// Gets an action tool by ID
    /// </summary>
    Task<ActionTool> GetActionToolAsync(string toolId);

    /// <summary>
    /// Lists action tools for a copilot
    /// </summary>
    Task<IEnumerable<ActionTool>> ListActionToolsAsync(string copilotId);

    /// <summary>
    /// Updates action tool configuration
    /// </summary>
    Task<ActionTool> UpdateActionToolAsync(string toolId, ActionTool tool);

    /// <summary>
    /// Tests action tool execution
    /// </summary>
    Task<ActionTestResult> TestActionToolAsync(string toolId, object testPayload);

    /// <summary>
    /// Validates integration configuration
    /// </summary>
    Task<IntegrationValidationResult> ValidateIntegrationAsync(string toolId);

    /// <summary>
    /// Gets execution audit trail
    /// </summary>
    Task<IEnumerable<ActionExecutionRecord>> GetExecutionAuditAsync(string toolId, int days = 30);

    /// <summary>
    /// Enables/disables action tool
    /// </summary>
    Task<bool> SetToolEnabledAsync(string toolId, bool enabled);

    /// <summary>
    /// Gets action tool metrics
    /// </summary>
    Task<ActionToolMetrics> GetMetricsAsync(string toolId);
}

/// <summary>
/// Interface for Trigger configuration management.
/// </summary>
public interface ITriggerManagementService
{
    /// <summary>
    /// Creates a new trigger configuration
    /// </summary>
    Task<TriggerConfiguration> CreateTriggerAsync(TriggerConfiguration trigger, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a trigger configuration
    /// </summary>
    Task<TriggerConfiguration> GetTriggerAsync(Guid triggerId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists triggers for a copilot
    /// </summary>
    Task<IEnumerable<TriggerConfiguration>> ListTriggersAsync(string copilotId);

    /// <summary>
    /// Updates trigger configuration
    /// </summary>
    Task<TriggerConfiguration> UpdateTriggerAsync(string triggerId, TriggerConfiguration trigger);

    /// <summary>
    /// Tests trigger firing
    /// </summary>
    Task<TriggerTestResult> TestTriggerAsync(string triggerId);

    /// <summary>
    /// Gets trigger fire history
    /// </summary>
    Task<IEnumerable<TriggerFireRecord>> GetTriggerHistoryAsync(string triggerId, int days = 30);

    /// <summary>
    /// Enables/disables trigger
    /// </summary>
    Task<bool> SetTriggerEnabledAsync(string triggerId, bool enabled);

    /// <summary>
    /// Gets trigger metrics
    /// </summary>
    Task<TriggerMetrics> GetMetricsAsync(string triggerId);

    /// <summary>
    /// Checks dead letter queue for failed triggers
    /// </summary>
    Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string triggerId, int limit = 100);
}

/// <summary>
/// Interface for Evaluation management.
/// </summary>
public interface IEvaluationConfigurationService
{
    /// <summary>
    /// Creates evaluation configuration
    /// </summary>
    Task<EvaluationConfiguration> CreateEvaluationAsync(string copilotId, EvaluationConfiguration evaluation);

    /// <summary>
    /// Gets evaluation configuration
    /// </summary>
    Task<EvaluationConfiguration> GetEvaluationAsync(string evaluationId);

    /// <summary>
    /// Lists evaluations for a copilot
    /// </summary>
    Task<IEnumerable<EvaluationConfiguration>> ListEvaluationsAsync(string copilotId);

    /// <summary>
    /// Updates evaluation configuration
    /// </summary>
    Task<EvaluationConfiguration> UpdateEvaluationAsync(string evaluationId, EvaluationConfiguration evaluation);

    /// <summary>
    /// Runs evaluation manually
    /// </summary>
    Task<EvaluationResult> RunEvaluationAsync(string evaluationId);

    /// <summary>
    /// Gets evaluation results history
    /// </summary>
    Task<IEnumerable<EvaluationResult>> GetEvaluationResultsAsync(string evaluationId, int days = 30);

    /// <summary>
    /// Analyzes evaluation trends
    /// </summary>
    Task<EvaluationTrendAnalysis> AnalyzeTrendsAsync(string evaluationId);

    /// <summary>
    /// Enables/disables evaluation
    /// </summary>
    Task<bool> SetEvaluationEnabledAsync(string evaluationId, bool enabled);

    /// <summary>
    /// Gets SLA compliance for evaluation
    /// </summary>
    Task<EvaluationSLACompliance> GetSLAComplianceAsync(string evaluationId);
}

/// <summary>
/// Interface for Landing Zone management and governance.
/// </summary>
public interface ILandingZoneService
{
    /// <summary>
    /// Creates landing zone configuration
    /// </summary>
    Task<LandingZoneConfiguration> CreateLandingZoneAsync(LandingZoneConfiguration landingZone);

    /// <summary>
    /// Gets landing zone by ID
    /// </summary>
    Task<LandingZoneConfiguration> GetLandingZoneAsync(string landingZoneId);

    /// <summary>
    /// Lists all landing zones
    /// </summary>
    Task<IEnumerable<LandingZoneConfiguration>> ListLandingZonesAsync();

    /// <summary>
    /// Gets landing zone by type
    /// </summary>
    Task<IEnumerable<LandingZoneConfiguration>> GetLandingZonesByTypeAsync(string landingZoneType);

    /// <summary>
    /// Updates landing zone configuration
    /// </summary>
    Task<LandingZoneConfiguration> UpdateLandingZoneAsync(string landingZoneId, LandingZoneConfiguration landingZone);

    /// <summary>
    /// Validates copilot compliance with landing zone requirements
    /// </summary>
    Task<LandingZoneComplianceResult> ValidateCopilotComplianceAsync(string copilotId, string landingZoneId);

    /// <summary>
    /// Gets guardrail violations for a copilot
    /// </summary>
    Task<IEnumerable<GuardrailViolation>> GetGuardrailViolationsAsync(string copilotId);

    /// <summary>
    /// Applies landing zone policy to copilot
    /// </summary>
    Task<bool> ApplyLandingZonePolicyAsync(string copilotId, string landingZoneId);
}

/// <summary>
/// Interface for Copilot governance policy management.
/// </summary>
public interface ICopilotGovernancePolicyService
{
    /// <summary>
    /// Creates governance policy
    /// </summary>
    Task<CopilotGovernancePolicy> CreatePolicyAsync(CopilotGovernancePolicy policy);

    /// <summary>
    /// Gets governance policy
    /// </summary>
    Task<CopilotGovernancePolicy> GetPolicyAsync(string policyId);

    /// <summary>
    /// Lists policies for landing zone
    /// </summary>
    Task<IEnumerable<CopilotGovernancePolicy>> ListPoliciesByLandingZoneAsync(string landingZone);

    /// <summary>
    /// Updates governance policy
    /// </summary>
    Task<CopilotGovernancePolicy> UpdatePolicyAsync(string policyId, CopilotGovernancePolicy policy);

    /// <summary>
    /// Validates copilot against policy
    /// </summary>
    Task<PolicyComplianceResult> ValidateComplianceAsync(string copilotId, string policyId);

    /// <summary>
    /// Gets policy violations
    /// </summary>
    Task<IEnumerable<PolicyViolation>> GetViolationsAsync(string policyId);

    /// <summary>
    /// Generates compliance report
    /// </summary>
    Task<ComplianceReport> GenerateComplianceReportAsync(string policyId, DateTime startDate, DateTime endDate);
}

/// <summary>
/// Interface for Development Guidelines enforcement.
/// </summary>
public interface IDevelopmentGuidelinesService
{
    /// <summary>
    /// Gets development guidelines
    /// </summary>
    Task<DevelopmentGuidelines> GetGuidelinesAsync(string version = "latest");

    /// <summary>
    /// Creates guidelines adherence assessment
    /// </summary>
    Task<GuidelinesAdherence> AssessAdherenceAsync(string copilotId);

    /// <summary>
    /// Gets specific guideline checklist for copilot
    /// </summary>
    Task<IEnumerable<GuidelineChecklistItem>> GetChecklistAsync(string copilotId, string checklistType);

    /// <summary>
    /// Updates guideline compliance status
    /// </summary>
    Task<bool> UpdateComplianceStatusAsync(string copilotId, string guidelineId, bool compliant, string evidence);

    /// <summary>
    /// Requests deviation approval
    /// </summary>
    Task<GuidelineDeviation> RequestDeviationAsync(string copilotId, GuidelineDeviation deviation);

    /// <summary>
    /// Gets pending deviations
    /// </summary>
    Task<IEnumerable<GuidelineDeviation>> GetPendingDeviationsAsync();

    /// <summary>
    /// Approves/rejects deviation
    /// </summary>
    Task<bool> ProcessDeviationAsync(string deviationId, bool approved, string approverNotes);

    /// <summary>
    /// Gets guidelines adherence report
    /// </summary>
    Task<GuidelinesAdherenceReport> GenerateAdherenceReportAsync(string copilotId);
}

/// <summary>
/// Interface for Copilot testing and validation.
/// </summary>
public interface ICopilotTestingService
{
    /// <summary>
    /// Runs comprehensive test suite
    /// </summary>
    Task<CopilotTestResults> RunFullTestSuiteAsync(string copilotId);

    /// <summary>
    /// Runs unit tests
    /// </summary>
    Task<TestResults> RunUnitTestsAsync(string copilotId);

    /// <summary>
    /// Runs integration tests
    /// </summary>
    Task<TestResults> RunIntegrationTestsAsync(string copilotId);

    /// <summary>
    /// Runs end-to-end tests
    /// </summary>
    Task<TestResults> RunE2ETestsAsync(string copilotId);

    /// <summary>
    /// Runs performance tests
    /// </summary>
    Task<PerformanceTestResults> RunPerformanceTestsAsync(string copilotId);

    /// <summary>
    /// Runs security tests
    /// </summary>
    Task<SecurityTestResults> RunSecurityTestsAsync(string copilotId);

    /// <summary>
    /// Gets test coverage report
    /// </summary>
    Task<CoverageReport> GetCoverageReportAsync(string copilotId);

    /// <summary>
    /// Validates against development guidelines
    /// </summary>
    Task<GuidelinesValidationResult> ValidateGuidelinesAsync(string copilotId);
}

/// <summary>
/// Interface for Copilot analytics and reporting.
/// </summary>
public interface ICopilotAnalyticsService
{
    /// <summary>
    /// Gets usage analytics
    /// </summary>
    Task<UsageAnalytics> GetUsageAnalyticsAsync(string copilotId, int days = 30);

    /// <summary>
    /// Gets user engagement metrics
    /// </summary>
    Task<UserEngagementMetrics> GetUserEngagementAsync(string copilotId, int days = 30);

    /// <summary>
    /// Gets cost analysis</summary>
    Task<CostAnalysis> GetCostAnalysisAsync(string copilotId, int months = 12);

    /// <summary>
    /// Compares copilot performance against baselines
    /// </summary>
    Task<PerformanceComparison> ComparePerformanceAsync(string copilotId, string baselineCopilotId);

    /// <summary>
    /// Gets trend analysis over time
    /// </summary>
    Task<TrendAnalysis> GetTrendAnalysisAsync(string copilotId, int days = 90);

    /// <summary>
    /// Generates comprehensive analytics report
    /// </summary>
    Task<AnalyticsReport> GenerateAnalyticsReportAsync(string copilotId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets user satisfaction trends
    /// </summary>
    Task<SatisfactionTrends> GetSatisfactionTrendsAsync(string copilotId, int days = 90);

    /// <summary>
    /// Identifies optimization opportunities
    /// </summary>
    Task<List<OptimizationOpportunity>> IdentifyOptimizationOpportunitiesAsync(string copilotId);
}

#region Supporting Models for Service Operations

/// <summary>
/// Deployment result information.
/// </summary>
public class DeploymentResult
{
    public string DeploymentId { get; set; }
    public bool Success { get; set; }
    public string Environment { get; set; }
    public DateTime DeploymentTime { get; set; }
    public string Version { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public string DeploymentLogsUrl { get; set; }
}

public class DeploymentStatus
{
    public string Environment { get; set; }
    public string Status { get; set; } // InProgress, Completed, Failed, Rollback
    public int ProgressPercentage { get; set; }
    public string CurrentStage { get; set; }
    public DateTime? ExpectedCompletion { get; set; }
}

public class ToolTestResult
{
    public bool Success { get; set; }
    public decimal ResponseTime { get; set; }
    public string SampleData { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class OptimizationRecommendations
{
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, string> ExpectedImpact { get; set; } = new();
}

public class DataSourceValidationResult
{
    public bool IsValid { get; set; }
    public string ConnectionStatus { get; set; }
    public int RecordCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<string> ValidationIssues { get; set; } = new();
}

public class ActionTestResult
{
    public bool Success { get; set; }
    public decimal ExecutionTime { get; set; }
    public object ResponseData { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class IntegrationValidationResult
{
    public bool IsValid { get; set; }
    public string IntegrationStatus { get; set; }
    public string Endpoint { get; set; }
    public int HealthCheckResponse { get; set; }
    public List<string> ConfigurationIssues { get; set; } = new();
}

public class ActionExecutionRecord
{
    public string ExecutionId { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string Status { get; set; }
    public decimal ExecutionTimeMs { get; set; }
    public object InputData { get; set; }
    public object OutputData { get; set; }
    public string InitiatedBy { get; set; }
}

public class TriggerTestResult
{
    public bool Success { get; set; }
    public decimal ExecutionTime { get; set; }
    public string TriggeredAction { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class TriggerFireRecord
{
    public string FireId { get; set; }
    public DateTime FiredTime { get; set; }
    public string Status { get; set; }
    public decimal ExecutionTimeMs { get; set; }
    public string ActionsTriggered { get; set; }
}

public class DeadLetterMessage
{
    public string MessageId { get; set; }
    public DateTime ReceivedTime { get; set; }
    public string ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public object MessageContent { get; set; }
}

public class EvaluationTrendAnalysis
{
    public decimal AverageTrend { get; set; }
    public decimal StandardDeviation { get; set; }
    public string TrendDirection { get; set; } // Improving, Degrading, Stable
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}

public class EvaluationSLACompliance
{
    public decimal PassRatePercentage { get; set; }
    public decimal SLATarget { get; set; }
    public bool MetSLA { get; set; }
    public int DaysSinceBreach { get; set; }
}

public class LandingZoneComplianceResult
{
    public string CopilotId { get; set; }
    public string LandingZoneId { get; set; }
    public bool IsCompliant { get; set; }
    public int ComplianceScore { get; set; }
    public List<ComplianceIssue> Issues { get; set; } = new();
}

public class ComplianceIssue
{
    public string Category { get; set; }
    public string Issue { get; set; }
    public string Severity { get; set; }
    public string Recommendation { get; set; }
}

public class GuardrailViolation
{
    public string ViolationId { get; set; }
    public string GuardrailName { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }
    public DateTime DetectedTime { get; set; }
}

public class PolicyComplianceResult
{
    public bool IsCompliant { get; set; }
    public List<string> ComplianceAreas { get; set; } = new();
    public List<string> NonComplianceAreas { get; set; } = new();
}

public class PolicyViolation
{
    public string CopilotId { get; set; }
    public string ViolatedRequirement { get; set; }
    public string Description { get; set; }
    public DateTime DetectedTime { get; set; }
}

public class ComplianceReport
{
    public string PolicyId { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal ComplianceScore { get; set; }
    public List<string> Findings { get; set; } = new();
}

public class GuidelinesAdherenceReport
{
    public string CopilotId { get; set; }
    public decimal OverallScore { get; set; }
    public List<CategoryScore> CategoryScores { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class CategoryScore
{
    public string Category { get; set; }
    public decimal Score { get; set; }
}

public class CopilotTestResults
{
    public string CopilotId { get; set; }
    public bool OverallPass { get; set; }
    public TestResults UnitTests { get; set; }
    public TestResults IntegrationTests { get; set; }
    public TestResults E2ETests { get; set; }
    public PerformanceTestResults PerformanceTests { get; set; }
    public SecurityTestResults SecurityTests { get; set; }
}

public class TestResults
{
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public decimal PassRate { get; set; }
    public List<FailedTest> Failures { get; set; } = new();
}

public class FailedTest
{
    public string TestName { get; set; }
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }
}

public class PerformanceTestResults
{
    public decimal AverageLatencyMs { get; set; }
    public decimal P95LatencyMs { get; set; }
    public int ThroughputRPS { get; set; }
    public bool PassedThresholds { get; set; }
}

public class SecurityTestResults
{
    public int VulnerabilitiesFound { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public bool PassedSecurityTests { get; set; }
}

public class CoverageReport
{
    public decimal CodeCoverage { get; set; }
    public decimal LineCoverage { get; set; }
    public decimal BranchCoverage { get; set; }
}

public class GuidelinesValidationResult
{
    public bool PassedValidation { get; set; }
    public List<string> PassedGuidelines { get; set; } = new();
    public List<string> FailedGuidelines { get; set; } = new();
}

// Analytics Models
public class UsageAnalytics
{
    public int TotalInteractions { get; set; }
    public int DailyActiveUsers { get; set; }
    public decimal AverageSessionDuration { get; set; }
}

public class UserEngagementMetrics
{
    public decimal RetentionRate { get; set; }
    public int RecurringUsers { get; set; }
    public decimal AverageSatisfactionScore { get; set; }
}

public class CostAnalysis
{
    public decimal TotalCost { get; set; }
    public decimal CostPerUser { get; set; }
    public decimal CostTrend { get; set; }
}

public class PerformanceComparison
{
    public string CopilotId { get; set; }
    public string BaselineCopilotId { get; set; }
    public Dictionary<string, decimal> MetricsComparison { get; set; } = new();
}

public class TrendAnalysis
{
    public List<TrendDataPoint> Trends { get; set; } = new();
    public string OverallDirection { get; set; }
}

public class AnalyticsReport
{
    public string CopilotId { get; set; }
    public UsageAnalytics Usage { get; set; }
    public UserEngagementMetrics Engagement { get; set; }
    public CostAnalysis CostData { get; set; }
}

public class SatisfactionTrends
{
    public decimal AverageScore { get; set; }
    public List<TrendDataPoint> ScoreTrends { get; set; } = new();
}

public class OptimizationOpportunity
{
    public string Opportunity { get; set; }
    public string Impact { get; set; }
    public string Effort { get; set; }
    public decimal ExpectedROI { get; set; }
}

#endregion
