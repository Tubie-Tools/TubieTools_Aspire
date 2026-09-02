using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Copilot Governance Policy - landing zone compliance and controls.
/// </summary>
[Table("CopilotGovernancePolicies")]
public class CopilotGovernancePolicy
{
    [Key]
    [StringLength(36)]
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(100)]
    public string LandingZone { get; set; }

    [Required]
    [StringLength(255)]
    public string PolicyName { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    // Data residency requirements (JSON)
    public string DataResidency { get; set; }

    // Security requirements (JSON)
    public string SecurityRequirements { get; set; }

    // Compliance requirements (JSON array)
    public string ComplianceRequirements { get; set; }

    // Data handling policies (JSON)
    public string DataHandling { get; set; }

    // Model governance (JSON)
    public string ModelGovernance { get; set; }

    // Audit requirements (JSON)
    public string AuditRequirements { get; set; }

    // Cost management policies (JSON)
    public string CostManagement { get; set; }

    // Incident response policy (JSON)
    public string IncidentResponse { get; set; }

    [StringLength(50)]
    public string EnforcementMode { get; set; } = "Strict";

    public bool RequiresAttestation { get; set; }

    public DateTime LastReviewDate { get; set; }

    public DateTime NextReviewDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<CopilotApplication> CopilotApplications { get; set; } = new List<CopilotApplication>();
}
