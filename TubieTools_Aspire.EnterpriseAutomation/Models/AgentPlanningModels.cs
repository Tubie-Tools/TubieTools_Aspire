namespace TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Represents an AI Agent within the enterprise automation framework.
/// Tracks lifecycle, governance, and operational metrics.
/// </summary>
public class AIAgent
{
    /// <summary>Unique identifier for the agent</summary>
    public string AgentId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable agent name</summary>
    public string Name { get; set; }

    /// <summary>Detailed description of agent capabilities and purpose</summary>
    public string Description { get; set; }

    /// <summary>Type of AI agent (TaskAutomation, DataAnalysis, etc.)</summary>
    public string AgentType { get; set; }

    /// <summary>Current lifecycle phase (PlanAgents, GovernAndSecureAgents, BuildAgents, OperateAgents)</summary>
    public string CurrentLifecyclePhase { get; set; }

    /// <summary>Current adoption phase (Strategy, Plan, Ready, Govern, Secure, Manage)</summary>
    public string CurrentAdoptionPhase { get; set; }

    /// <summary>Overall risk level of this agent</summary>
    public string RiskLevel { get; set; }

    /// <summary>Model being used (e.g., gpt-4, claude-3, custom)</summary>
    public string ModelName { get; set; }

    /// <summary>Version of the model</summary>
    public string ModelVersion { get; set; }

    /// <summary>Owner/team responsible for this agent</summary>
    public string Owner { get; set; }

    /// <summary>Business unit/department using this agent</summary>
    public string BusinessUnit { get; set; }

    /// <summary>Expected business outcomes and metrics</summary>
    public string ExpectedOutcomes { get; set; }

    /// <summary>Approval status</summary>
    public ApprovalStatus ApprovalStatus { get; set; }

    /// <summary>Agent planning details</summary>
    public AgentPlan AgentPlan { get; set; }

    /// <summary>Governance and security configuration</summary>
    public GovernanceConfiguration GovernanceConfig { get; set; }

    /// <summary>Build and deployment information</summary>
    public AgentBuild AgentBuild { get; set; }

    /// <summary>Operational metrics and monitoring</summary>
    public OperationalMetrics OperationalMetrics { get; set; }

    /// <summary>List of associated compliance and audit records</summary>
    public List<ComplianceRecord> ComplianceRecords { get; set; } = new();

    /// <summary>Creation timestamp</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Last modification timestamp</summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Flag indicating if agent is active</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents the planning phase of an AI agent.
/// Aligns with CAF "Plan Agents" lifecycle phase.
/// </summary>
public class AgentPlan
{
    public string PlanId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Detailed business case and justification</summary>
    public string BusinessCase { get; set; }

    /// <summary>Specific use cases and business problems addressed</summary>
    public List<string> UseCases { get; set; } = new();

    /// <summary>Required AI capabilities</summary>
    public List<string> RequiredCapabilities { get; set; } = new();

    /// <summary>Data sources required for the agent</summary>
    public List<DataSourceRequirement> DataSources { get; set; } = new();

    /// <summary>Integration points with existing systems</summary>
    public List<SystemIntegration> IntegrationPoints { get; set; } = new();

    /// <summary>Estimated timeline for deployment</summary>
    public DateTime PlannedDeploymentDate { get; set; }

    /// <summary>Estimated budget and resource requirements</summary>
    public BudgetAllocation Budget { get; set; }

    /// <summary>Key stakeholders involved</summary>
    public List<Stakeholder> Stakeholders { get; set; } = new();

    /// <summary>Identified risks and mitigation strategies during planning</summary>
    public List<RiskAssessment> IdentifiedRisks { get; set; } = new();

    /// <summary>Success criteria and KPIs</summary>
    public List<KPI> SuccessCriteria { get; set; } = new();

    /// <summary>Plan status</summary>
    public string Status { get; set; } = "Draft";

    /// <summary>Plan created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents data source requirements for an AI agent.
/// </summary>
public class DataSourceRequirement
{
    public string SourceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Name of the data source</summary>
    public string SourceName { get; set; }

    /// <summary>Type of data source (Database, API, FileShare, etc.)</summary>
    public string SourceType { get; set; }

    /// <summary>Data classification (Public, Internal, Confidential, Restricted)</summary>
    public string DataClassification { get; set; }

    /// <summary>Estimated volume of data to be processed</summary>
    public string EstimatedDataVolume { get; set; }

    /// <summary>Frequency of data access (RealTime, Daily, Weekly, etc.)</summary>
    public string AccessFrequency { get; set; }

    /// <summary>Compliance implications (GDPR, HIPAA, SOC2, etc.)</summary>
    public List<string> ComplianceRequirements { get; set; } = new();

    /// <summary>Data retention policy</summary>
    public string RetentionPolicy { get; set; }

    /// <summary>Sensitivity/PII concerns</summary>
    public bool ContainsPII { get; set; }

    /// <summary>Data masking/anonymization required</summary>
    public bool RequiresMasking { get; set; }
}

/// <summary>
/// Represents system integration points.
/// </summary>
public class SystemIntegration
{
    public string IntegrationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>System name to integrate with</summary>
    public string SystemName { get; set; }

    /// <summary>Type of integration (REST, GraphQL, Queue, RPC, etc.)</summary>
    public string IntegrationType { get; set; }

    /// <summary>Integration endpoint or connection string</summary>
    public string Endpoint { get; set; }

    /// <summary>Authentication method required</summary>
    public string AuthenticationMethod { get; set; }

    /// <summary>Expected throughput (requests/second, messages/minute, etc.)</summary>
    public string ExpectedThroughput { get; set; }

    /// <summary>SLA requirements</summary>
    public string SLARequirement { get; set; }
}

/// <summary>
/// Budget and resource allocation for an agent.
/// </summary>
public class BudgetAllocation
{
    public string BudgetId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Initial development budget</summary>
    public decimal DevelopmentBudget { get; set; }

    /// <summary>Annual operational budget</summary>
    public decimal OperationalBudget { get; set; }

    /// <summary>Infrastructure costs (compute, storage, API calls)</summary>
    public decimal InfrastructureCosts { get; set; }

    /// <summary>FTE (Full-Time Equivalent) resources needed</summary>
    public decimal RequiredFTEs { get; set; }

    /// <summary>Required skills (ML Engineers, DevOps, SecurityEngineers, etc.)</summary>
    public List<string> RequiredSkills { get; set; } = new();

    /// <summary>Training budget for team</summary>
    public decimal TrainingBudget { get; set; }

    /// <summary>Currency of budget</summary>
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Represents a stakeholder involved in the agent project.
/// </summary>
public class Stakeholder
{
    public string StakeholderId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Stakeholder name</summary>
    public string Name { get; set; }

    /// <summary>Role/title</summary>
    public string Role { get; set; }

    /// <summary>Department/business unit</summary>
    public string Department { get; set; }

    /// <summary>Level of influence (Executive, Manager, Contributor)</summary>
    public string InfluenceLevel { get; set; }

    /// <summary>Interest level (High, Medium, Low)</summary>
    public string InterestLevel { get; set; }

    /// <summary>Contact email</summary>
    public string Email { get; set; }

    /// <summary>Responsibilities in the project</summary>
    public List<string> Responsibilities { get; set; } = new();
}

/// <summary>
/// Represents a risk assessment during planning.
/// </summary>
public class RiskAssessment
{
    public string RiskId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Risk description</summary>
    public string Description { get; set; }

    /// <summary>Risk category (Technical, Business, Compliance, Security, Operational)</summary>
    public string Category { get; set; }

    /// <summary>Probability of occurrence (High, Medium, Low)</summary>
    public string Probability { get; set; }

    /// <summary>Impact if risk occurs (Critical, High, Medium, Low)</summary>
    public string Impact { get; set; }

    /// <summary>Mitigation strategy</summary>
    public string MitigationStrategy { get; set; }

    /// <summary>Owner responsible for this risk</summary>
    public string Owner { get; set; }

    /// <summary>Status (Active, Mitigated, Closed)</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Date risk was identified</summary>
    public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Key Performance Indicator for agent success.
/// </summary>
public class KPI
{
    public string KPIId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>KPI name</summary>
    public string Name { get; set; }

    /// <summary>Detailed description</summary>
    public string Description { get; set; }

    /// <summary>Target value to achieve</summary>
    public string TargetValue { get; set; }

    /// <summary>Baseline value before AI agent deployment</summary>
    public string BaselineValue { get; set; }

    /// <summary>Unit of measurement</summary>
    public string Unit { get; set; }

    /// <summary>How KPI is measured/calculated</summary>
    public string MeasurementMethod { get; set; }

    /// <summary>Frequency of measurement</summary>
    public string MeasurementFrequency { get; set; }

    /// <summary>Owner responsible for tracking this KPI</summary>
    public string Owner { get; set; }

    /// <summary>Business impact of this KPI (HighImpact, MediumImpact, LowImpact)</summary>
    public string BusinessImpact { get; set; }
}
