namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Embedding configuration for vector-based retrieval.
/// </summary>
public class EmbeddingConfig
{
    /// <summary>Embedding model provider</summary>
    public string EmbeddingProvider { get; set; }

    /// <summary>Embedding model name</summary>
    public string EmbeddingModel { get; set; }

    /// <summary>Embedding dimension</summary>
    public int EmbeddingDimension { get; set; } = 1536;

    /// <summary>Vector store backend (Pinecone, Weaviate, Qdrant, Chroma, etc.)</summary>
    public string VectorStoreBackend { get; set; }

    /// <summary>Vector store connection</summary>
    public string VectorStoreConnection { get; set; }

    /// <summary>Re-embedding frequency (when to update embeddings)</summary>
    public string ReembeddingFrequency { get; set; } // Never, Weekly, Monthly, OnUpdate

    /// <summary>Similarity metric (cosine, euclidean, dot_product)</summary>
    public string SimilarityMetric { get; set; } = "cosine";
}
