namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Cache configuration to optimize retrieval performance.
/// </summary>
public class CacheConfig
{
    /// <summary>Caching enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Cache provider (Redis, AppCache, CosmosDB, etc.)</summary>
    public string CacheProvider { get; set; }

    /// <summary>Cache TTL (minutes)</summary>
    public int CacheTTLMinutes { get; set; } = 60;

    /// <summary>Cache eviction policy</summary>
    public string EvictionPolicy { get; set; } = "LRU"; // LRU, LFU, FIFO

    /// <summary>Maximum cache size (MB)</summary>
    public int MaxCacheSizeMB { get; set; } = 500;

    /// <summary>Cache hit ratio target (%)</summary>
    public decimal TargetHitRatio { get; set; } = 0.7m;
}
