namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Represents a Knowledge Tool in the Copilot.
/// Used for retrieval of information and context.
/// </summary>
public class KnowledgeTool
{
    public string ToolId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Tool name</summary>
    public string Name { get; set; }

    /// <summary>Tool description</summary>
    public string Description { get; set; }

    /// <summary>Knowledge tool pattern (VectorSearch, RAG, StructuredQuery, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Data source type/location</summary>
    public DataSourceConfig DataSource { get; set; }

    /// <summary>Search/retrieval configuration</summary>
    public RetrievalConfig RetrievalConfig { get; set; }

    /// <summary>Embedding configuration (if using vector search)</summary>
    public EmbeddingConfig EmbeddingConfig { get; set; }

    /// <summary>Context window size for this tool</summary>
    public int ContextWindowSize { get; set; } = 2000;

    /// <summary>Minimum relevance threshold for results</summary>
    public decimal RelevanceThreshold { get; set; } = 0.7m;

    /// <summary>Maximum results to return</summary>
    public int MaxResults { get; set; } = 5;

    /// <summary>Cache configuration to optimize performance</summary>
    public CacheConfig CacheConfig { get; set; }

    /// <summary>Access control for this tool</summary>
    public ToolAccessControl AccessControl { get; set; }

    /// <summary>Freshness/update frequency requirements</summary>
    public string FreshnessRequirement { get; set; } // RealTime, Daily, Weekly, Monthly

    /// <summary>Tool is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Performance metrics</summary>
    public ToolPerformanceMetrics PerformanceMetrics { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}
