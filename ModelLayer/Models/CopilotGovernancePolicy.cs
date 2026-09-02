namespace ModelLayer.Models;

/// <summary>
/// Copilot governance policy aligned with landing zones and enterprise standards.
/// </summary>
public class CopilotGovernancePolicy
{
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Landing zone this policy applies to</summary>
    public string LandingZone { get; set; }

    /// <summary>Policy name</summary>
    public string PolicyName { get; set; }

    /// <summary>Policy description</summary>
    public string Description { get; set; }

    /// <summary>Data residency requirements</summary>
    public DataResidencyRequirements DataResidency { get; set; }

    /// <summary>Security requirements</summary>
    public SecurityRequirements SecurityRequirements { get; set; }

    /// <summary>Compliance requirements</summary>
    public List<ComplianceRequirementPolicy> ComplianceRequirements { get; set; } = new();

    /// <summary>Data handling policies</summary>
    public DataHandlingPolicy DataHandling { get; set; }

    /// <summary>Model and AI governance</summary>
    public ModelGovernance ModelGovernance { get; set; }

    /// <summary>Audit and logging requirements</summary>
    public AuditRequirements AuditRequirements { get; set; }

    /// <summary>Cost management policies</summary>
    public CostManagementPolicy CostManagement { get; set; }

    /// <summary>Escalation and incident response</summary>
    public IncidentResponsePolicy IncidentResponse { get; set; }

    /// <summary>Policy enforcement mode (Strict, Moderate, Advisory)</summary>
    public string EnforcementMode { get; set; } = "Strict";

    /// <summary>Attestation requirement</summary>
    public bool RequiresAttestation { get; set; }

    /// <summary>Last review date</summary>
    public DateTime LastReviewDate { get; set; }

    /// <summary>Next review date</summary>
    public DateTime NextReviewDate { get; set; }
    public object? Id { get; set; }
    public object Name { get; set; }
    public object Version { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public bool IsActive { get; set; }
}
