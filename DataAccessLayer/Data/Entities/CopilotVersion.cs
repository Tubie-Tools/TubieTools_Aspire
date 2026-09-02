using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Copilot Version - version history and release notes.
/// </summary>
[Table("CopilotVersions")]
public class CopilotVersion
{
    [Key]
    [StringLength(36)]
    public string VersionId { get; set; } = Guid.NewGuid().ToString();

    [StringLength(36)]
    public string CopilotId { get; set; }

    [Required]
    [StringLength(50)]
    public string VersionNumber { get; set; }

    [StringLength(2000)]
    public string ReleaseNotes { get; set; }

    [Required]
    public DateTime ReleaseDate { get; set; }

    // Changes list (JSON array of VersionChange objects)
    public List<string> Changes { get; set; }

    // Breaking changes (JSON array of strings)
    public List<string> BreakingChanges { get; set; }

    // Deprecations (JSON array of strings)
    public List<string> Deprecations { get; set; }

    // Database migration required
    public bool RequiresMigration { get; set; }

    // Backward compatible
    public bool IsBackwardCompatible { get; set; }

    [StringLength(100)]
    public string PrereleaseName { get; set; }

    public bool IsPrerelease { get; set; }

    // Release candidate flag
    public bool IsReleaseCandidate { get; set; }

    // Deployment instructions (JSON)
    public string DeploymentInstructions { get; set; }

    // Rollback instructions (JSON)
    public string RollbackInstructions { get; set; }

    [StringLength(255)]
    public string ReleasedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation property
    [ForeignKey(nameof(CopilotId))]
    public virtual CopilotApplication CopilotApplication { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// Supporting class for version changes.
/// </summary>
public class VersionChange
{
    public string ChangeId { get; set; }
    public string Category { get; set; } // Feature, BugFix, Performance, Security, etc.
    public string Description { get; set; }
    public string ImpactLevel { get; set; } // Critical, High, Medium, Low
    public DateTime ModifiedDate { get; set; }
}
