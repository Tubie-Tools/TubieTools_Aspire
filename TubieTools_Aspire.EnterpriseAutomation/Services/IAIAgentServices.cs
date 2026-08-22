namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Interface for AI Agent Lifecycle Management
/// Orchestrates movement through all CAF phases
/// </summary>
public interface IAIAgentLifecycleService
{
    /// <summary>
    /// Creates a new AI agent in the planning phase
    /// </summary>
    Task<AIAgent> CreateAgentAsync(AIAgent agent);

    /// <summary>
    /// Updates current lifecycle phase for an agent
    /// </summary>
    Task<AIAgent> AdvanceToPhaseAsync(string agentId, string lifecyclePhase);

    /// <summary>
    /// Gets an agent by ID with full context
    /// </summary>
    Task<AIAgent> GetAgentAsync(string agentId);

    /// <summary>
    /// Lists all agents with optional filtering by phase
    /// </summary>
    Task<IEnumerable<AIAgent>> ListAgentsAsync(string filterByPhase = null);

    /// <summary>
    /// Validates agent readiness for next phase
    /// </summary>
    Task<PhaseReadinessAssessment> AssessPhaseReadinessAsync(string agentId);

    /// <summary>
    /// Generates phase transition report
    /// </summary>
    Task<PhaseTransitionReport> GeneratePhaseTransitionReportAsync(string agentId, string targetPhase);
}

/// <summary>
/// Interface for AI Agent Planning Phase Operations
/// </summary>
public interface IAgentPlanningService
{
    /// <summary>
    /// Creates a comprehensive plan for a new agent
    /// </summary>
    Task<AgentPlan> CreateAgentPlanAsync(string agentId, AgentPlan plan);

    /// <summary>
    /// Updates an existing plan
    /// </summary>
    Task<AgentPlan> UpdateAgentPlanAsync(string agentId, AgentPlan plan);

    /// <summary>
    /// Gets the plan for an agent
    /// </summary>
    Task<AgentPlan> GetAgentPlanAsync(string agentId);

    /// <summary>
    /// Validates plan completeness
    /// </summary>
    Task<PlanValidationResult> ValidatePlanAsync(string agentId);

    /// <summary>
    /// Approves a plan for progression to next phase
    /// </summary>
    Task<bool> ApprovePlanAsync(string agentId, string approver, string comments);

    /// <summary>
    /// Performs risk assessment on the plan
    /// </summary>
    Task<List<RiskAssessment>> PerformRiskAssessmentAsync(string agentId);
}

/// <summary>
/// Interface for AI Agent Governance and Security Operations
/// </summary>
public interface IAgentGovernanceService
{
    /// <summary>
    /// Creates governance configuration for an agent
    /// </summary>
    Task<GovernanceConfiguration> CreateGovernanceAsync(string agentId, GovernanceConfiguration governance);

    /// <summary>
    /// Updates governance configuration
    /// </summary>
    Task<GovernanceConfiguration> UpdateGovernanceAsync(string agentId, GovernanceConfiguration governance);

    /// <summary>
    /// Gets governance configuration for an agent
    /// </summary>
    Task<GovernanceConfiguration> GetGovernanceAsync(string agentId);

    /// <summary>
    /// Applies governance policies to an agent
    /// </summary>
    Task<bool> ApplyPoliciesAsync(string agentId, List<GovernancePolicy> policies);

    /// <summary>
    /// Checks compliance status against all policies
    /// </summary>
    Task<ComplianceStatus> CheckComplianceStatusAsync(string agentId);

    /// <summary>
    /// Validates approval workflow completion
    /// </summary>
    Task<bool> ValidateApprovalWorkflowAsync(string agentId);

    /// <summary>
    /// Records a compliance audit
    /// </summary>
    Task<ComplianceRecord> RecordComplianceAuditAsync(string agentId, ComplianceRecord audit);
}

/// <summary>
/// Interface for AI Agent Build Operations
/// </summary>
public interface IAgentBuildService
{
    /// <summary>
    /// Creates build configuration for agent
    /// </summary>
    Task<AgentBuild> CreateBuildAsync(string agentId, AgentBuild build);

    /// <summary>
    /// Triggers build pipeline
    /// </summary>
    Task<BuildPipelineExecution> TriggerBuildAsync(string agentId);

    /// <summary>
    /// Gets build status
    /// </summary>
    Task<AgentBuild> GetBuildStatusAsync(string agentId);

    /// <summary>
    /// Records test results
    /// </summary>
    Task<bool> RecordTestResultsAsync(string agentId, TestingStrategy testResults);

    /// <summary>
    /// Records deployment
    /// </summary>
    Task<DeploymentRecord> RecordDeploymentAsync(string agentId, DeploymentRecord deployment);

    /// <summary>
    /// Validates security testing completion
    /// </summary>
    Task<SecurityValidationResult> ValidateSecurityTestingAsync(string agentId);

    /// <summary>
    /// Validates model performance against criteria
    /// </summary>
    Task<ModelPerformanceValidation> ValidateModelPerformanceAsync(string agentId);
}

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

/// <summary>
/// Interface for CAF Adoption Phase Tracking
/// </summary>
public interface ICAFAdoptionPhaseService
{
    /// <summary>
    /// Tracks organization's adoption maturity across all CAF phases
    /// </summary>
    Task<CAFAdoptionMaturity> GetAdoptionMaturityAsync();

    /// <summary>
    /// Records progress in a specific adoption phase
    /// </summary>
    Task<bool> RecordAdoptionProgressAsync(string phase, string initiative, int completionPercentage);

    /// <summary>
    /// Generates adoption roadmap
    /// </summary>
    Task<AdoptionRoadmap> GenerateAdoptionRoadmapAsync();

    /// <summary>
    /// Identifies gaps and blockers in adoption
    /// </summary>
    Task<List<AdoptionBlocker>> IdentifyAdoptionGapsAsync();
}

/// <summary>
/// Interface for AI Agent Audit and Compliance Reporting
/// </summary>
public interface IAIAgentAuditService
{
    /// <summary>
    /// Generates comprehensive audit report for an agent
    /// </summary>
    Task<AuditReport> GenerateAuditReportAsync(string agentId);

    /// <summary>
    /// Generates compliance report for regulatory requirements
    /// </summary>
    Task<RegulatoryComplianceReport> GenerateComplianceReportAsync(string agentId, string regulationName);

    /// <summary>
    /// Generates security assessment report
    /// </summary>
    Task<SecurityAssessmentReport> GenerateSecurityAssessmentAsync(string agentId);

    /// <summary>
    /// Generates bias and fairness report
    /// </summary>
    Task<BiasAndFairnessReport> GenerateBiasAndFairnessReportAsync(string agentId);

    /// <summary>
    /// Exports audit trail data
    /// </summary>
    Task<AuditTrail> ExportAuditTrailAsync(string agentId, DateTime startDate, DateTime endDate);
}

#region Supporting Models for Service Operations

/// <summary>
/// Assessment of agent readiness for next phase
/// </summary>
public class PhaseReadinessAssessment
{
    public string AgentId { get; set; }
    public string CurrentPhase { get; set; }
    public string TargetPhase { get; set; }
    public bool IsReady { get; set; }
    public List<string> CompletedRequirements { get; set; } = new();
    public List<string> MissingRequirements { get; set; } = new();
    public int ReadinessPercentage { get; set; }
    public DateTime AssessmentDate { get; set; }
}

/// <summary>
/// Report for phase transition
/// </summary>
public class PhaseTransitionReport
{
    public string AgentId { get; set; }
    public string FromPhase { get; set; }
    public string ToPhase { get; set; }
    public DateTime TransitionDate { get; set; }
    public string TransitionStatus { get; set; }
    public List<string> ObservationsAndRecommendations { get; set; } = new();
    public List<RiskAssessment> IdentifiedRisks { get; set; } = new();
}

/// <summary>
/// Results of plan validation
/// </summary>
public class PlanValidationResult
{
    public string AgentId { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationPassed { get; set; } = new();
    public List<string> ValidationFailed { get; set; } = new();
    public List<string> RecommendedImprovements { get; set; } = new();
    public int CompletionPercentage { get; set; }
}

/// <summary>
/// Compliance status check results
/// </summary>
public class ComplianceStatus
{
    public string AgentId { get; set; }
    public bool IsCompliant { get; set; }
    public List<GovernancePolicy> AppliedPolicies { get; set; } = new();
    public List<PolicyComplianceDetail> ComplianceDetails { get; set; } = new();
    public DateTime CheckDate { get; set; }
}

public class PolicyComplianceDetail
{
    public string PolicyId { get; set; }
    public string PolicyName { get; set; }
    public bool IsCompliant { get; set; }
    public string NonComplianceReason { get; set; }
    public List<RemediationAction> RemediationActions { get; set; } = new();
}

/// <summary>
/// Build pipeline execution details
/// </summary>
public class BuildPipelineExecution
{
    public string ExecutionId { get; set; }
    public string AgentId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } // Running, Completed, Failed
    public int ProgressPercentage { get; set; }
    public List<BuildStageSummary> StageSummaries { get; set; } = new();
}

public class BuildStageSummary
{
    public string StageName { get; set; }
    public string Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string LogsLocation { get; set; }
}

/// <summary>
/// Security validation results from testing
/// </summary>
public class SecurityValidationResult
{
    public string AgentId { get; set; }
    public bool PassedValidation { get; set; }
    public int CriticalVulnerabilities { get; set; }
    public int HighVulnerabilities { get; set; }
    public int MediumVulnerabilities { get; set; }
    public int LowVulnerabilities { get; set; }
    public List<Vulnerability> Vulnerabilities { get; set; } = new();
    public DateTime ValidationDate { get; set; }
}

public class Vulnerability
{
    public string Title { get; set; }
    public string Severity { get; set; }
    public string Description { get; set; }
    public string RemediationAdvice { get; set; }
    public string CVEId { get; set; }
}

/// <summary>
/// Model performance validation against success criteria
/// </summary>
public class ModelPerformanceValidation
{
    public string AgentId { get; set; }
    public bool PassedValidation { get; set; }
    public List<KPIValidationResult> KPIResults { get; set; } = new();
    public int PassedCriteria { get; set; }
    public int TotalCriteria { get; set; }
    public DateTime ValidationDate { get; set; }
}

public class KPIValidationResult
{
    public string KPIName { get; set; }
    public string TargetValue { get; set; }
    public string ActualValue { get; set; }
    public bool Met { get; set; }
    public string Unit { get; set; }
}

/// <summary>
/// SLA compliance report
/// </summary>
public class SLAComplianceReport
{
    public string AgentId { get; set; }
    public int ReportingPeriodDays { get; set; }
    public decimal UptimePercentage { get; set; }
    public decimal UptimeSLATarget { get; set; }
    public bool MetUptimeSLA { get; set; }
    public decimal ErrorRatePercentage { get; set; }
    public decimal ErrorRateSLATarget { get; set; }
    public bool MetErrorRateSLA { get; set; }
    public decimal AverageLatencyMs { get; set; }
    public decimal LatencySLATargetMs { get; set; }
    public bool MetLatencySLA { get; set; }
    public List<string> SLABreaches { get; set; } = new();
}

/// <summary>
/// CAF Adoption maturity across all phases
/// </summary>
public class CAFAdoptionMaturity
{
    public int StrategyMaturity { get; set; } // 0-100%
    public int PlanMaturity { get; set; }
    public int ReadyMaturity { get; set; }
    public int GovernMaturity { get; set; }
    public int SecureMaturity { get; set; }
    public int ManageMaturity { get; set; }
    public int OverallMaturity { get; set; } // Average
    public DateTime AssessmentDate { get; set; }
}

/// <summary>
/// AI Adoption roadmap with milestones
/// </summary>
public class AdoptionRoadmap
{
    public Dictionary<string, List<Milestone>> PhasesMilestones { get; set; } = new();
    public DateTime ProjectedCompletionDate { get; set; }
    public List<string> KeySuccesFactors { get; set; } = new();
    public List<string> CriticalDependencies { get; set; } = new();
}

public class Milestone
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime TargetDate { get; set; }
    public int Progress { get; set; }
    public string Owner { get; set; }
}

/// <summary>
/// Blockers preventing adoption progress
/// </summary>
public class AdoptionBlocker
{
    public string Phase { get; set; }
    public string BlockerDescription { get; set; }
    public string Impact { get; set; }
    public string RecommendedResolution { get; set; }
    public string AssignedTo { get; set; }
    public DateTime IdentifiedDate { get; set; }
}

/// <summary>
/// Comprehensive audit report
/// </summary>
public class AuditReport
{
    public string AgentId { get; set; }
    public DateTime AuditDate { get; set; }
    public string Auditor { get; set; }
    public string OverallAssessment { get; set; }
    public List<string> FindingsSummary { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Regulatory compliance report
/// </summary>
public class RegulatoryComplianceReport
{
    public string AgentId { get; set; }
    public string RegulationName { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> RequirementsMet { get; set; } = new();
    public List<string> RequirementsNotMet { get; set; } = new();
    public List<string> MitigationsMissing { get; set; } = new();
}

/// <summary>
/// Security assessment report
/// </summary>
public class SecurityAssessmentReport
{
    public string AgentId { get; set; }
    public int SecurityScore { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public List<string> KeyFindings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Bias and fairness assessment report
/// </summary>
public class BiasAndFairnessReport
{
    public string AgentId { get; set; }
    public bool BiasesIdentified { get; set; }
    public List<string> IdentifiedBiases { get; set; } = new();
    public List<BiasItem> BiasDetails { get; set; } = new();
    public List<string> MitigationMeasures { get; set; } = new();
    public int FairnessScore { get; set; }
}

/// <summary>
/// Audit trail export
/// </summary>
public class AuditTrail
{
    public string AgentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<AuditEvent> Events { get; set; } = new();
}

public class AuditEvent
{
    public DateTime EventTime { get; set; }
    public string EventType { get; set; }
    public string Actor { get; set; }
    public string Action { get; set; }
    public string ResourceAffected { get; set; }
    public string Details { get; set; }
}

#endregion
