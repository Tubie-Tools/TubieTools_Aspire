namespace ModelLayer.Models;

using ModelLayer.Models.Action;
using ModelLayer.Models.Evaluation;

/// <summary>
/// Represents a Copilot application with complete governance and lifecycle.
/// Integrates with landing zones and enterprise architecture.
/// </summary>
public class CopilotApplication
{
    /// <summary>Unique identifier for the copilot</summary>
    public string CopilotId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable copilot name</summary>
    public string Name { get; set; }

    /// <summary>Detailed description and purpose</summary>
    public string Description { get; set; }

    /// <summary>Business objective of this copilot</summary>
    public string BusinessObjective { get; set; }

    /// <summary>Primary use case</summary>
    public string PrimaryUseCase { get; set; }

    /// <summary>Target audience/users</summary>
    public string TargetAudience { get; set; }

    /// <summary>Landing zone this copilot belongs to</summary>
    public string LandingZone { get; set; }

    /// <summary>Current maturity level</summary>
    public string MaturityLevel { get; set; }

    /// <summary>List of capabilities implemented</summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>Copilot model configuration</summary>
    public CopilotModelConfiguration ModelConfiguration { get; set; }

    /// <summary>Knowledge tools integrated</summary>
    public List<KnowledgeTool> KnowledgeTools { get; set; } = new();

    /// <summary>Action tools integrated</summary>
    public List<ActionTool> ActionTools { get; set; } = new();

    /// <summary>Trigger configurations</summary>
    public List<TriggerConfiguration> Triggers { get; set; } = new();

    /// <summary>Evaluation/quality check configurations</summary>
    public List<EvaluationConfiguration> Evaluations { get; set; } = new();

    /// <summary>Governance policy assigned to this copilot</summary>
    public CopilotGovernancePolicy GovernancePolicy { get; set; }

    /// <summary>Development guidelines adherence</summary>
    public GuidelinesAdherence GuidelinesAdherence { get; set; }

    /// <summary>Performance metrics and monitoring</summary>
    public CopilotPerformanceMetrics PerformanceMetrics { get; set; }

    /// <summary>Deployment configuration</summary>
    public CopilotDeploymentConfig DeploymentConfig { get; set; }

    /// <summary>Version history</summary>
    public List<CopilotVersion> VersionHistory { get; set; } = new();

    /// <summary>Current version</summary>
    public string CurrentVersion { get; set; } = "1.0.0";

    /// <summary>Owner/team</summary>
    public string Owner { get; set; }

    /// <summary>Contact email</summary>
    public string ContactEmail { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Last modified date</summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Is active</summary>
    public bool IsActive { get; set; } = true;
    public object Version { get; set; }
    public object? Id { get; set; }
}
