using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Knowledge Tool - retrieval and context sources.
/// </summary>
[Table("KnowledgeTools")]
public class KnowledgeTool
{
    [Key]
    [StringLength(36)]
    public string ToolId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [StringLength(100)]
    public string Pattern { get; set; }

    // Foreign key to CopilotApplication
    [StringLength(36)]
    public string CopilotApplicationId { get; set; }

    // Data source configuration (JSON)
    public string DataSourceConfig { get; set; }

    // Retrieval configuration (JSON)
    public string RetrievalConfig { get; set; }

    // Embedding configuration (JSON)
    public string EmbeddingConfig { get; set; }

    public int ContextWindowSize { get; set; } = 2000;

    [Range(0, 1)]
    public decimal RelevanceThreshold { get; set; } = 0.7m;

    public int MaxResults { get; set; } = 5;

    // Cache configuration (JSON)
    public string CacheConfig { get; set; }

    // Access control configuration (JSON)
    public string AccessControl { get; set; }

    [StringLength(50)]
    public string FreshnessRequirement { get; set; }

    public bool IsEnabled { get; set; } = true;

    // Performance metrics (JSON)
    public string PerformanceMetrics { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(CopilotApplicationId))]
    public virtual CopilotApplication CopilotApplication { get; set; }
    public string Type { get; set; }
    public bool IsActive { get; set; }
}
