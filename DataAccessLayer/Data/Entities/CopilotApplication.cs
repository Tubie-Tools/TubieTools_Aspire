using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Copilot Application - persisted in CopilotStudioDbContext.
/// Represents a complete copilot deployment with governance and lifecycle management.
/// </summary>
[Table("CopilotApplications")]
public class CopilotApplication
{
    [Key]
    [StringLength(36)]
    public string CopilotId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [StringLength(500)]
    public string BusinessObjective { get; set; }

    [StringLength(500)]
    public string PrimaryUseCase { get; set; }

    [StringLength(500)]
    public string TargetAudience { get; set; }

    [Required]
    [StringLength(100)]
    public string LandingZone { get; set; }

    [StringLength(100)]
    public string MaturityLevel { get; set; }

    [StringLength(2000)]
    public string Capabilities { get; set; } // JSON serialized

    [StringLength(36)]
    public string ModelConfigurationId { get; set; }

    [StringLength(36)]
    public string GovernancePolicyId { get; set; }

    [StringLength(500)]
    public string GuidelinesAdherence { get; set; }

    [StringLength(36)]
    public string PerformanceMetricsId { get; set; }

    [StringLength(36)]
    public string DeploymentConfigId { get; set; }

    [StringLength(50)]
    public string CurrentVersion { get; set; } = "1.0.0";

    [StringLength(255)]
    public string Owner { get; set; }

    [StringLength(255)]
    [EmailAddress]
    public string ContactEmail { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey(nameof(ModelConfigurationId))]
    public virtual CopilotModelConfiguration ModelConfiguration { get; set; }

    [ForeignKey(nameof(GovernancePolicyId))]
    public virtual CopilotGovernancePolicy GovernancePolicy { get; set; }

    [ForeignKey(nameof(PerformanceMetricsId))]
    public virtual CopilotPerformanceMetrics PerformanceMetrics { get; set; }

    [ForeignKey(nameof(DeploymentConfigId))]
    public virtual CopilotDeploymentConfig DeploymentConfig { get; set; }

    public virtual ICollection<KnowledgeTool> KnowledgeTools { get; set; } = new List<KnowledgeTool>();
    public virtual ICollection<CopilotVersion> VersionHistory { get; set; } = new List<CopilotVersion>();
}
