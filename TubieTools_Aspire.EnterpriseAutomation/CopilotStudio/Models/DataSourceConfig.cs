namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Data source configuration for knowledge tools.
/// </summary>
public class DataSourceConfig
{
    public string SourceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Source type (VectorDB, SQL, GraphDB, REST, FileShare, etc.)</summary>
    public string SourceType { get; set; }

    /// <summary>Connection string/endpoint</summary>
    public string ConnectionString { get; set; }

    /// <summary>Database or collection name</summary>
    public string DatabaseName { get; set; }

    /// <summary>Table/index/document name</summary>
    public string TableName { get; set; }

    /// <summary>Authentication method</summary>
    public string AuthMethod { get; set; } // ConnectionString, ManagedIdentity, API Key, OAuth

    /// <summary>Query template for structured queries</summary>
    public string QueryTemplate { get; set; }

    /// <summary>Pagination support</summary>
    public bool SupportsPagination { get; set; }

    /// <summary>Supports filtering</summary>
    public bool SupportsFiltering { get; set; }

    /// <summary>Update frequency (minutes)</summary>
    public int UpdateFrequencyMinutes { get; set; }

    /// <summary>Last synced date</summary>
    public DateTime? LastSyncedDate { get; set; }

    /// <summary>Data quality score (0-100)</summary>
    public int QualityScore { get; set; }
}
