namespace ModelLayer.Models;

/// <summary>
/// Retrieval configuration for knowledge tools.
/// </summary>
public class RetrievalConfig
{
    /// <summary>Retrieval method (semantic, lexical, hybrid)</summary>
    public string RetrievalMethod { get; set; } = "semantic";

    /// <summary>Reranking enabled</summary>
    public bool EnableReranking { get; set; }

    /// <summary>Reranker model if enabled</summary>
    public string RerankingModel { get; set; }

    /// <summary>Chunk size for splitting documents</summary>
    public int ChunkSize { get; set; } = 512;

    /// <summary>Chunk overlap</summary>
    public int ChunkOverlap { get; set; } = 50;

    /// <summary>Include metadata in results</summary>
    public bool IncludeMetadata { get; set; } = true;

    /// <summary>Deduplication enabled</summary>
    public bool EnableDeduplication { get; set; } = true;

    /// <summary>Temporal filtering (e.g., last 30 days)</summary>
    public string TemporalFilter { get; set; }

    /// <summary>Citation/source attribution</summary>
    public bool IncludeSourceAttribution { get; set; } = true;
}
