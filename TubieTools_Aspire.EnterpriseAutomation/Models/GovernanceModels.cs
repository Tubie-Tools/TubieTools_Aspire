namespace TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Represents governance configuration for an AI agent.
/// Aligns with CAF "Govern & Secure Agents" lifecycle phase.
/// </summary>
public class GovernanceConfiguration
{
    public string GovernanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Governanced enforced (true/false)</summary>
    public bool IsEnforced { get; set; }

    /// <summary>Associated governance policies</summary>
    public List<GovernancePolicy> Policies { get; set; } = new();

    /// <summary>Approval chain and workflow</summary>
    public ApprovalWorkflow ApprovalWorkflow { get; set; }

    /// <summary>Access control configuration</summary>
    public AccessControlPolicy AccessControl { get; set; }

    /// <summary>Data governance rules</summary>
    public DataGovernancePolicy DataGovernance { get; set; }

    /// <summary>Compliance requirements tracking</summary>
    public List<ComplianceRequirement> ComplianceRequirements { get; set; } = new();

    /// <summary>Audit logging configuration</summary>
    public AuditLoggingConfiguration AuditLogging { get; set; }

    /// <summary>Bias and fairness assessment</summary>
    public BiasAndFairnessAssessment BiasAssessment { get; set; }

    /// <summary>Transparency and explainability measures</summary>
    public TransparencyConfiguration TransparencyConfig { get; set; }

    /// <summary>Change management policy for agent updates</summary>
    public ChangeManagementPolicy ChangePolicy { get; set; }

    /// <summary>Date governance was established</summary>
    public DateTime EstablishedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Next governance review date</summary>
    public DateTime NextReviewDate { get; set; }
}

/// <summary>
/// Represents a single governance policy.
/// </summary>
public class GovernancePolicy
{
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Policy name</summary>
    public string PolicyName { get; set; }

    /// <summary>Policy category (DataGovernance, AccessControl, Compliance, Security, etc.)</summary>
    public string Category { get; set; }

    /// <summary>Policy description and requirements</summary>
    public string Description { get; set; }

    /// <summary>Whether policy is mandatory or recommended</summary>
    public bool IsMandatory { get; set; }

    /// <summary>Enforcement mechanism</summary>
    public string EnforcementMechanism { get; set; }

    /// <summary>Policy version</summary>
    public string Version { get; set; }

    /// <summary>Effective date of this policy</summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>Owner of this policy</summary>
    public string PolicyOwner { get; set; }

    /// <summary>Compliance status (Compliant, NonCompliant, PartiallyCompliant, Pending)</summary>
    public string ComplianceStatus { get; set; }
}

/// <summary>
/// Represents the approval workflow for agent changes and deployments.
/// </summary>
public class ApprovalWorkflow
{
    public string WorkflowId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>List of approval steps required</summary>
    public List<ApprovalStep> ApprovalSteps { get; set; } = new();

    /// <summary>Requires business approval</summary>
    public bool RequiresBusinessApproval { get; set; }

    /// <summary>Requires security approval</summary>
    public bool RequiresSecurityApproval { get; set; }

    /// <summary>Requires compliance approval</summary>
    public bool RequiresComplianceApproval { get; set; }

    /// <summary>Requires executive sign-off</summary>
    public bool RequiresExecutiveSignOff { get; set; }

    /// <summary>Maximum approval time allowed (days)</summary>
    public int MaxApprovalDays { get; set; }

    /// <summary>Escalation contacts if approval is delayed</summary>
    public List<string> EscalationContacts { get; set; } = new();
}

/// <summary>
/// Represents a single approval step in the workflow.
/// </summary>
public class ApprovalStep
{
    public string StepId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Step order in the workflow</summary>
    public int StepOrder { get; set; }

    /// <summary>Role or person who must approve</summary>
    public string ApprovalRole { get; set; }

    /// <summary>Name of approver (if specific person assigned)</summary>
    public string ApproverName { get; set; }

    /// <summary>Email of approver</summary>
    public string ApproverEmail { get; set; }

    /// <summary>Step description</summary>
    public string Description { get; set; }

    /// <summary>Current approval status</summary>
    public ApprovalStatus Status { get; set; }

    /// <summary>Date when approved</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Approval comments or rejection reason</summary>
    public string Comments { get; set; }
}

/// <summary>
/// Approval status enumeration.
/// </summary>
public enum ApprovalStatus
{
    Draft,
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    ChangesRequested,
    ApprovedWithConditions,
    Expired
}

/// <summary>
/// Access control policy for agent access and execution.
/// </summary>
public class AccessControlPolicy
{
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Who can invoke/execute the agent (RBAC - Role-Based Access Control)</summary>
    public List<AccessRule> ExecutionRules { get; set; } = new();

    /// <summary>Who can modify the agent configuration</summary>
    public List<AccessRule> ConfigurationRules { get; set; } = new();

    /// <summary>Who can view agent outputs and results</summary>
    public List<AccessRule> ViewRules { get; set; } = new();

    /// <summary>Multi-factor authentication required</summary>
    public bool RequiresMFA { get; set; }

    /// <summary>IP address restrictions</summary>
    public List<string> AllowedIPRanges { get; set; } = new();

    /// <summary>Time-based restrictions (e.g., business hours only)</summary>
    public TimeBasedRestriction TimeRestriction { get; set; }

    /// <summary>Maximum concurrent users/API calls</summary>
    public int MaxConcurrentSessions { get; set; }

    /// <summary>Rate limiting (calls per minute)</summary>
    public int RateLimitPerMinute { get; set; }
}

/// <summary>
/// Represents a single access rule.
/// </summary>
public class AccessRule
{
    public string RuleId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>User or role this rule applies to</summary>
    public string Principal { get; set; }

    /// <summary>Principal type (User, Role, Group, Service)</summary>
    public string PrincipalType { get; set; }

    /// <summary>Permission granted (Allow/Deny)</summary>
    public bool IsAllowed { get; set; }

    /// <summary>Specific actions allowed (e.g., Execute, Read, Modify, Delete)</summary>
    public List<string> AllowedActions { get; set; } = new();

    /// <summary>Resource conditions (e.g., only during business hours)</summary>
    public string Conditions { get; set; }
}

/// <summary>
/// Time-based access restrictions.
/// </summary>
public class TimeBasedRestriction
{
    /// <summary>Start time (HH:mm format)</summary>
    public string StartTime { get; set; }

    /// <summary>End time (HH:mm format)</summary>
    public string EndTime { get; set; }

    /// <summary>Allowed days of week (Monday-Friday, etc.)</summary>
    public List<string> AllowedDays { get; set; } = new();

    /// <summary>Timezone for time restrictions</summary>
    public string Timezone { get; set; }

    /// <summary>Holidays when access is restricted</summary>
    public List<DateTime> ExcludedHolidays { get; set; } = new();
}

/// <summary>
/// Data governance policy for agent data handling.
/// </summary>
public class DataGovernancePolicy
{
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Approved data sources only</summary>
    public List<string> ApprovedDataSources { get; set; } = new();

    /// <summary>Prohibited data sources</summary>
    public List<string> ProhibitedDataSources { get; set; } = new();

    /// <summary>Data retention period (days)</summary>
    public int DataRetentionDays { get; set; }

    /// <summary>Data deletion enforcement (automatic/manual)</summary>
    public string DeletionMethod { get; set; }

    /// <summary>PII handling requirements</summary>
    public PIIHandlingPolicy PIIPolicy { get; set; }

    /// <summary>Data classification levels allowed</summary>
    public List<string> AllowedDataClassifications { get; set; } = new();

    /// <summary>Anonymization/masking required fields</summary>
    public List<string> RequiresMaskingFields { get; set; } = new();

    /// <summary>Encryption requirements</summary>
    public string EncryptionRequirements { get; set; }
}

/// <summary>
/// PII (Personally Identifiable Information) handling policy.
/// </summary>
public class PIIHandlingPolicy
{
    /// <summary>Whether agent can process PII</summary>
    public bool CanProcessPII { get; set; }

    /// <summary>If allowed, which PII types (SSN, Email, Phone, Address, etc.)</summary>
    public List<string> AllowedPIITypes { get; set; } = new();

    /// <summary>Masking/tokenization strategy</summary>
    public string MaskingStrategy { get; set; }

    /// <summary>Pseudonymization requirements</summary>
    public bool RequiresPseudonymization { get; set; }

    /// <summary>GDPR Right to be Forgotten implementation</summary>
    public bool SupportsRightToBeForgotten { get; set; }

    /// <summary>Data residency requirements</summary>
    public string DataResidencyCountries { get; set; }
}

/// <summary>
/// Audit logging configuration to track agent usage and changes.
/// </summary>
public class AuditLoggingConfiguration
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Whether audit logging is enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Audit log retention period (days)</summary>
    public int LogRetentionDays { get; set; }

    /// <summary>Events to log (Execution, Configuration Change, Access, Error)</summary>
    public List<string> LoggedEvents { get; set; } = new();

    /// <summary>Log storage location</summary>
    public string LogStorageLocation { get; set; }

    /// <summary>Log encryption enabled</summary>
    public bool IsEncrypted { get; set; }

    /// <summary>Real-time alerting threshold for unusual activity</summary>
    public string AlertingThreshold { get; set; }

    /// <summary>Alert recipients</summary>
    public List<string> AlertRecipients { get; set; } = new();
}

/// <summary>
/// Bias and fairness assessment for the AI agent.
/// </summary>
public class BiasAndFairnessAssessment
{
    public string AssessmentId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Whether assessment has been performed</summary>
    public bool IsAssessed { get; set; }

    /// <summary>Date assessment was conducted</summary>
    public DateTime? AssessmentDate { get; set; }

    /// <summary>Identified bias categories (Gender, Race, Age, Socioeconomic, etc.)</summary>
    public List<string> IdentifiedBiases { get; set; } = new();

    /// <summary>Severity of bias (Critical, High, Medium, Low)</summary>
    public List<BiasItem> BiasItems { get; set; } = new();

    /// <summary>Mitigation measures implemented</summary>
    public List<string> MitigationMeasures { get; set; } = new();

    /// <summary>Fairness metrics and thresholds</summary>
    public List<FairnessMetric> FairnessMetrics { get; set; } = new();

    /// <summary>Next bias assessment scheduled date</summary>
    public DateTime? NextAssessmentDate { get; set; }

    /// <summary>Assessor name and credentials</summary>
    public string Assessor { get; set; }
}

/// <summary>
/// Represents a single identified bias.
/// </summary>
public class BiasItem
{
    public string BiasId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Type of bias identified</summary>
    public string BiasType { get; set; }

    /// <summary>Detailed description</summary>
    public string Description { get; set; }

    /// <summary>Severity level</summary>
    public string Severity { get; set; }

    /// <summary>Affected groups</summary>
    public List<string> AffectedGroups { get; set; } = new();

    /// <summary>Percentage impact (if measurable)</summary>
    public decimal ImpactPercentage { get; set; }
}

/// <summary>
/// Fairness metric for ensuring equitable outcomes.
/// </summary>
public class FairnessMetric
{
    public string MetricId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Metric name (e.g., Demographic Parity, Equalized Odds)</summary>
    public string Name { get; set; }

    /// <summary>Target fairness threshold (0-1)</summary>
    public decimal TargetThreshold { get; set; }

    /// <summary>Current measured value</summary>
    public decimal CurrentValue { get; set; }

    /// <summary>Data as of when measurement was taken</summary>
    public DateTime MeasurementDate { get; set; }
}

/// <summary>
/// Transparency and explainability configuration.
/// </summary>
public class TransparencyConfiguration
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Whether explainability is required</summary>
    public bool RequiresExplainability { get; set; }

    /// <summary>Explainability methods used (FeatureImportance, LIME, SHAP, etc.)</summary>
    public List<string> ExplainabilityMethods { get; set; } = new();

    /// <summary>User-facing transparency information</summary>
    public string TransparencyStatement { get; set; }

    /// <summary>Disclosure of AI involvement in decision-making</summary>
    public bool DisclosesAIInvolvement { get; set; }

    /// <summary>Confidence score threshold for results disclosure</summary>
    public decimal MinConfidenceForDisclosure { get; set; }

    /// <summary>Right to appeal or challenge decisions</summary>
    public bool AllowsAppeal { get; set; }

    /// <summary>Appeal process description</summary>
    public string AppealProcess { get; set; }
}

/// <summary>
/// Change management policy for agent updates and modifications.
/// </summary>
public class ChangeManagementPolicy
{
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Change approval required</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Types of changes requiring approval (Major, Minor, Patch)</summary>
    public List<string> ChangeTypesRequiringApproval { get; set; } = new();

    /// <summary>Testing required before deployment</summary>
    public bool RequiresTesting { get; set; }

    /// <summary>Test coverage minimum percentage</summary>
    public int MinimumTestCoverage { get; set; }

    /// <summary>Rollback capability required</summary>
    public bool RequiresRollbackCapability { get; set; }

    /// <summary>Staging/pre-production testing required</summary>
    public bool RequiresStagingDeployment { get; set; }

    /// <summary>Canary deployment percentage</summary>
    public int CanaryDeploymentPercentage { get; set; }

    /// <summary>Deployment window (e.g., maintenance windows only)</summary>
    public string DeploymentWindow { get; set; }

    /// <summary>Communication required to stakeholders</summary>
    public bool RequiresStakeholderNotification { get; set; }
}

/// <summary>
/// Compliance requirement for regulated environments.
/// </summary>
public class ComplianceRequirement
{
    public string RequirementId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Regulation name (GDPR, HIPAA, SOC2, etc.)</summary>
    public string RegulationName { get; set; }

    /// <summary>Specific article/section from regulation</summary>
    public string SpecificRequirement { get; set; }

    /// <summary>Compliance status</summary>
    public string ComplianceStatus { get; set; }

    /// <summary>Controls implemented to meet requirement</summary>
    public List<string> ControlsImplemented { get; set; } = new();

    /// <summary>Evidence of compliance</summary>
    public List<ComplianceEvidence> Evidence { get; set; } = new();

    /// <summary>Last compliance verification date</summary>
    public DateTime? LastVerificationDate { get; set; }

    /// <summary>Next verification due date</summary>
    public DateTime NextVerificationDue { get; set; }
}

/// <summary>
/// Evidence of compliance control implementation.
/// </summary>
public class ComplianceEvidence
{
    public string EvidenceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Type of evidence (Document, Audit, Test, etc.)</summary>
    public string EvidenceType { get; set; }

    /// <summary>Description of evidence</summary>
    public string Description { get; set; }

    /// <summary>Reference to document/audit ID</summary>
    public string Reference { get; set; }

    /// <summary>Date evidence was collected</summary>
    public DateTime CollectionDate { get; set; }

    /// <summary>Expiration date (if applicable)</summary>
    public DateTime? ExpirationDate { get; set; }
}
