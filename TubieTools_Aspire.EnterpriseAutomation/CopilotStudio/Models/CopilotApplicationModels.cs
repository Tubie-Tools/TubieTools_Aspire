namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

using TubieTools_Aspire.EnterpriseAutomation.Models;

/// <summary>
/// Represents a Copilot application with complete governance and lifecycle.
/// Integrates with landing zones and enterprise architecture.
/// </summary>
public class CopilotApplication
{
    /// <summary>Unique identifier for the copilot</summary>
    public string CopilotId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable copilot name</summary>
    public string Name { get; set; }

    /// <summary>Detailed description and purpose</summary>
    public string Description { get; set; }

    /// <summary>Business objective of this copilot</summary>
    public string BusinessObjective { get; set; }

    /// <summary>Primary use case</summary>
    public string PrimaryUseCase { get; set; }

    /// <summary>Target audience/users</summary>
    public string TargetAudience { get; set; }

    /// <summary>Landing zone this copilot belongs to</summary>
    public string LandingZone { get; set; }

    /// <summary>Current maturity level</summary>
    public string MaturityLevel { get; set; }

    /// <summary>List of capabilities implemented</summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>Copilot model configuration</summary>
    public CopilotModelConfiguration ModelConfiguration { get; set; }

    /// <summary>Knowledge tools integrated</summary>
    public List<KnowledgeTool> KnowledgeTools { get; set; } = new();

    /// <summary>Action tools integrated</summary>
    public List<ActionTool> ActionTools { get; set; } = new();

    /// <summary>Trigger configurations</summary>
    public List<TriggerConfiguration> Triggers { get; set; } = new();

    /// <summary>Evaluation/quality check configurations</summary>
    public List<EvaluationConfiguration> Evaluations { get; set; } = new();

    /// <summary>Governance policy assigned to this copilot</summary>
    public CopilotGovernancePolicy GovernancePolicy { get; set; }

    /// <summary>Development guidelines adherence</summary>
    public GuidelinesAdherence GuidelinesAdherence { get; set; }

    /// <summary>Performance metrics and monitoring</summary>
    public CopilotPerformanceMetrics PerformanceMetrics { get; set; }

    /// <summary>Deployment configuration</summary>
    public CopilotDeploymentConfig DeploymentConfig { get; set; }

    /// <summary>Version history</summary>
    public List<CopilotVersion> VersionHistory { get; set; } = new();

    /// <summary>Current version</summary>
    public string CurrentVersion { get; set; } = "1.0.0";

    /// <summary>Owner/team</summary>
    public string Owner { get; set; }

    /// <summary>Contact email</summary>
    public string ContactEmail { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Last modified date</summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Is active</summary>
    public bool IsActive { get; set; } = true;
    public object Version { get; set; }
    public object? Id { get; set; }
}

/// <summary>
/// Copilot model configuration (underlying LLM and settings).
/// </summary>
public class CopilotModelConfiguration
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Model provider (OpenAI, Anthropic, Custom, etc.)</summary>
    public string ModelProvider { get; set; }

    /// <summary>Model name/identifier</summary>
    public string ModelName { get; set; }

    /// <summary>Model version</summary>
    public string ModelVersion { get; set; }

    /// <summary>Temperature (0-1, controls randomness)</summary>
    public decimal Temperature { get; set; } = 0.7m;

    /// <summary>Top-p value (nucleus sampling)</summary>
    public decimal TopP { get; set; } = 0.9m;

    /// <summary>Max tokens for response</summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>Frequency penalty</summary>
    public decimal FrequencyPenalty { get; set; } = 0m;

    /// <summary>Presence penalty</summary>
    public decimal PresencePenalty { get; set; } = 0m;

    /// <summary>System prompt/instructions for the model</summary>
    public string SystemPrompt { get; set; }

    /// <summary>Custom parameters specific to model</summary>
    public Dictionary<string, object> CustomParameters { get; set; } = new();

    /// <summary>Model safety settings</summary>
    public ModelSafetySettings SafetySettings { get; set; }

    /// <summary>Context window size</summary>
    public int ContextWindowSize { get; set; }

    /// <summary>Supports function calling</summary>
    public bool SupportsFunctionCalling { get; set; }
    public object Id { get; set; }
}

/// <summary>
/// Model safety settings to prevent harmful outputs.
/// </summary>
public class ModelSafetySettings
{
    /// <summary>Content filter enabled</summary>
    public bool ContentFilterEnabled { get; set; }

    /// <summary>Maximum violence content threshold</summary>
    public string ViolenceThreshold { get; set; } = "High"; // Low, Medium, High

    /// <summary>Maximum sexual content threshold</summary>
    public string SexualContentThreshold { get; set; } = "High";

    /// <summary>Maximum hate speech threshold</summary>
    public string HateSpeechThreshold { get; set; } = "High";

    /// <summary>Enable jailbreak detection</summary>
    public bool EnableJailbreakDetection { get; set; } = true;

    /// <summary>Prompt injection filtering</summary>
    public bool EnablePromptInjectionFiltering { get; set; } = true;

    /// <summary>PII redaction enabled</summary>
    public bool EnablePIIRedaction { get; set; }

    /// <summary>Approved topics only</summary>
    public bool RestrictToApprovedTopics { get; set; }

    /// <summary>List of approved topics if restricted</summary>
    public List<string> ApprovedTopics { get; set; } = new();
}

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

/// <summary>
/// Tool access control configuration.
/// </summary>
public class ToolAccessControl
{
    /// <summary>Access control enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Roles allowed to use this tool</summary>
    public List<string> AllowedRoles { get; set; } = new();

    /// <summary>Users allowed to use this tool</summary>
    public List<string> AllowedUsers { get; set; } = new();

    /// <summary>User groups allowed</summary>
    public List<string> AllowedGroups { get; set; } = new();

    /// <summary>Row-level security filtering enabled</summary>
    public bool EnableRowLevelSecurity { get; set; }

    /// <summary>Data classification levels allowed to be queried</summary>
    public List<string> AllowedDataClassifications { get; set; } = new();

    /// <summary>Audit all tool usage</summary>
    public bool AuditAllUsage { get; set; } = true;

    /// <summary>Rate limit (queries per minute)</summary>
    public int RateLimitPerMinute { get; set; } = 100;
}

/// <summary>
/// Performance metrics for a knowledge tool.
/// </summary>
public class ToolPerformanceMetrics
{
    /// <summary>Average query latency (ms)</summary>
    public decimal AvgLatencyMs { get; set; }

    /// <summary>P95 latency (ms)</summary>
    public decimal P95LatencyMs { get; set; }

    /// <summary>Cache hit rate (%)</summary>
    public decimal CacheHitRate { get; set; }

    /// <summary>Query success rate (%)</summary>
    public decimal SuccessRate { get; set; }

    /// <summary>Total queries processed</summary>
    public long TotalQueriesProcessed { get; set; }

    /// <summary>Average relevance score of results</summary>
    public decimal AvgRelevanceScore { get; set; }

    /// <summary>Last measurement date</summary>
    public DateTime MeasurementDate { get; set; }
}

/// <summary>
/// Represents an Action Tool in the Copilot.
/// Used for executing tasks and modifying data.
/// </summary>
public class ActionTool
{
    public string ToolId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Tool name</summary>
    public string Name { get; set; }

    /// <summary>Tool description</summary>
    public string Description { get; set; }

    /// <summary>Action tool pattern (REST API, Database, File, Notification, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Integration configuration</summary>
    public IntegrationConfig Integration { get; set; }

    /// <summary>Action parameters and schema</summary>
    public ActionSchema Schema { get; set; }

    /// <summary>Error handling strategy</summary>
    public ErrorHandlingConfig ErrorHandling { get; set; }

    /// <summary>Retry configuration</summary>
    public RetryConfig RetryConfig { get; set; }

    /// <summary>Timeout (milliseconds)</summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>Requires approval before execution</summary>
    public bool RequiresApproval { get; set; }

    /// <summary>Approval rules</summary>
    public ApprovalRules ApprovalRules { get; set; }

    /// <summary>Access control</summary>
    public ToolAccessControl AccessControl { get; set; }

    /// <summary>Audit trail of all executions</summary>
    public bool EnableAuditTrail { get; set; } = true;

    /// <summary>Rollback capability</summary>
    public bool SupportsRollback { get; set; }

    /// <summary>Tool is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Performance metrics</summary>
    public ActionToolMetrics Metrics { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}

/// <summary>
/// Integration configuration for action tools.
/// </summary>
public class IntegrationConfig
{
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Integration framework (Power Platform, Logic Apps, Custom, etc.)</summary>
    public string Framework { get; set; }

    /// <summary>Endpoint/URL</summary>
    public string Endpoint { get; set; }

    /// <summary>HTTP method (GET, POST, PUT, DELETE, PATCH)</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>Authentication type</summary>
    public string AuthType { get; set; } // Bearer, APIKey, ManagedIdentity, BasicAuth, OAuth

    /// <summary>Authentication details (template with placeholders)</summary>
    public string AuthTemplate { get; set; }

    /// <summary>Request body template</summary>
    public string RequestTemplate { get; set; }

    /// <summary>Response mapping template</summary>
    public string ResponseTemplate { get; set; }

    /// <summary>Content type</summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>Required headers</summary>
    public Dictionary<string, string> RequiredHeaders { get; set; } = new();

    /// <summary>Rate limiting (requests per second)</summary>
    public decimal RateLimitPerSecond { get; set; } = 10;

    /// <summary>Connection test status</summary>
    public string ConnectionStatus { get; set; } = "Unknown"; // Connected, Failed, Disabled
}

/// <summary>
/// Action schema for parameter validation.
/// </summary>
public class ActionSchema
{
    /// <summary>Input parameters</summary>
    public List<ActionParameter> InputParameters { get; set; } = new();

    /// <summary>Output response schema</summary>
    public ActionOutput OutputSchema { get; set; }

    /// <summary>Supported response formats</summary>
    public List<string> SupportedFormats { get; set; } = new();
}

/// <summary>
/// Action parameter definition.
/// </summary>
public class ActionParameter
{
    public string ParamId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Parameter name</summary>
    public string Name { get; set; }

    /// <summary>Parameter description</summary>
    public string Description { get; set; }

    /// <summary>Data type</summary>
    public string DataType { get; set; } // string, integer, boolean, object, array

    /// <summary>Is required</summary>
    public bool IsRequired { get; set; }

    /// <summary>Default value</summary>
    public string DefaultValue { get; set; }

    /// <summary>Valid values/enum</summary>
    public List<string> ValidValues { get; set; } = new();

    /// <summary>Validation regex</summary>
    public string ValidationRegex { get; set; }

    /// <summary>Example value</summary>
    public string ExampleValue { get; set; }

    /// <summary>Parameter location in request (header, body, query, path)</summary>
    public string Location { get; set; } = "body";
}

/// <summary>
/// Action output schema.
/// </summary>
public class ActionOutput
{
    public string OutputId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Success response status code</summary>
    public int SuccessStatusCode { get; set; } = 200;

    /// <summary>Response fields/properties</summary>
    public Dictionary<string, string> ResponseFields { get; set; } = new();

    /// <summary>Error response schema</summary>
    public ErrorResponseSchema ErrorSchema { get; set; }
}

public class ErrorResponseSchema
{
    /// <summary>Error code field name</summary>
    public string ErrorCodeField { get; set; }

    /// <summary>Error message field name</summary>
    public string ErrorMessageField { get; set; }

    /// <summary>Common error codes and meanings</summary>
    public Dictionary<string, string> ErrorCodeMappings { get; set; } = new();
}

/// <summary>
/// Error handling configuration.
/// </summary>
public class ErrorHandlingConfig
{
    /// <summary>Fallback response</summary>
    public string FallbackResponse { get; set; }

    /// <summary>Continue on error flag</summary>
    public bool ContinueOnError { get; set; }

    /// <summary>Log errors</summary>
    public bool LogErrors { get; set; } = true;

    /// <summary>Alert on critical errors</summary>
    public bool AlertOnCriticalErrors { get; set; }

    /// <summary>Circuits breaker enabled</summary>
    public bool EnableCircuitBreaker { get; set; }

    /// <summary>Circuit breaker failure threshold</summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>Circuit breaker recovery timeout (seconds)</summary>
    public int CircuitBreakerTimeoutSeconds { get; set; } = 60;
}

/// <summary>
/// Retry configuration for failed actions.
/// </summary>
public class RetryConfig
{
    /// <summary>Retries enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Max retry attempts</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Initial backoff (milliseconds)</summary>
    public int InitialBackoffMs { get; set; } = 1000;

    /// <summary>Max backoff (milliseconds)</summary>
    public int MaxBackoffMs { get; set; } = 30000;

    /// <summary>Backoff multiplier</summary>
    public decimal BackoffMultiplier { get; set; } = 2m;

    /// <summary>Retry on status codes</summary>
    public List<int> RetryOnStatusCodes { get; set; } = new() { 408, 429, 500, 502, 503, 504 };
}

/// <summary>
/// Approval rules for action tools.
/// </summary>
public class ApprovalRules
{
    /// <summary>Role required for approval</summary>
    public string ApprovalRole { get; set; }

    /// <summary>Condition for requiring approval (e.g., "amount > 1000")</summary>
    public string ApprovalCondition { get; set; }

    /// <summary>Notification channels for approval</summary>
    public List<string> NotificationChannels { get; set; } = new();

    /// <summary>Approval timeout (hours)</summary>
    public int ApprovalTimeoutHours { get; set; } = 24;

    /// <summary>Escalation if not approved within timeout</summary>
    public string EscalationAction { get; set; } = "Escalate"; // Escalate, Reject, Repeat
}

/// <summary>
/// Performance metrics for an action tool.
/// </summary>
public class ActionToolMetrics
{
    /// <summary>Total executions</summary>
    public long TotalExecutions { get; set; }

    /// <summary>Successful executions</summary>
    public long SuccessfulExecutions { get; set; }

    /// <summary>Failed executions</summary>
    public long FailedExecutions { get; set; }

    /// <summary>Average execution time (ms)</summary>
    public decimal AvgExecutionTimeMs { get; set; }

    /// <summary>P95 execution time (ms)</summary>
    public decimal P95ExecutionTimeMs { get; set; }

    /// <summary>Success rate (%)</summary>
    public decimal SuccessRate { get; set; }

    /// <summary>Last measurement date</summary>
    public DateTime MeasurementDate { get; set; }
}

/// <summary>
/// Represents a Trigger/Event configuration.
/// </summary>
public class TriggerConfiguration
{
    public string TriggerId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Trigger name</summary>
    public string Name { get; set; }

    /// <summary>Trigger pattern (Scheduled, Webhook, Event, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Trigger configuration specific to pattern</summary>
    public TriggerDetails TriggerDetails { get; set; }

    /// <summary>Actions to execute when triggered</summary>
    public List<string> ActionIds { get; set; } = new();

    /// <summary>Workflow to execute</summary>
    public string WorkflowId { get; set; }

    /// <summary>Context/payload schema</summary>
    public string PayloadSchema { get; set; }

    /// <summary>Conditions for execution</summary>
    public string ExecutionCondition { get; set; }

    /// <summary>Is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Retry on failure</summary>
    public bool RetryOnFailure { get; set; } = true;

    /// <summary>Max retries</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Dead letter queue for failed triggers</summary>
    public string DeadLetterQueue { get; set; }

    /// <summary>Metrics</summary>
    public TriggerMetrics Metrics { get; set; }

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}

/// <summary>
/// Trigger-specific configuration details.
/// </summary>
public class TriggerDetails
{
    /// <summary>Schedule expression (cron format for scheduled triggers)</summary>
    public string ScheduleExpression { get; set; }

    /// <summary>Webhook endpoint</summary>
    public string WebhookUrl { get; set; }

    /// <summary>Event queue name</summary>
    public string QueueName { get; set; }

    /// <summary>Database connection for change triggers</summary>
    public string DatabaseConnection { get; set; }

    /// <summary>Database table/object being monitored</summary>
    public string MonitoredObject { get; set; }

    /// <summary>Change detection type (INSERT, UPDATE, DELETE, ALL)</summary>
    public List<string> ChangeTypes { get; set; } = new();

    /// <summary>Frequency for polling (if applicable)</summary>
    public string PollingFrequency { get; set; } // Seconds, Minutes, Hours

    /// <summary>Notification/Webhook header authentication token</summary>
    public string AuthenticationToken { get; set; }
}

/// <summary>
/// Trigger metrics.
/// </summary>
public class TriggerMetrics
{
    /// <summary>Total triggers fired</summary>
    public long TotalFired { get; set; }

    /// <summary>Successful executions</summary>
    public long Succeeded { get; set; }

    /// <summary>Failed executions</summary>
    public long Failed { get; set; }

    /// <summary>Average execution time (ms)</summary>
    public decimal AvgExecutionTimeMs { get; set; }

    /// <summary>Last fired date/time</summary>
    public DateTime? LastFiredDateTime { get; set; }
}

/// <summary>
/// Represents an Evaluation/Quality Check Configuration.
/// </summary>
public class EvaluationConfiguration
{
    public string EvaluationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Evaluation name</summary>
    public string Name { get; set; }

    /// <summary>Evaluation description</summary>
    public string Description { get; set; }

    /// <summary>Evaluation pattern (SemanticSimilarity, Compliance, Quality, Safety, etc.)</summary>
    public string Pattern { get; set; }

    /// <summary>Evaluation implementation details</summary>
    public EvaluationImplementation Implementation { get; set; }

    /// <summary>Scoring/grading model</summary>
    public ScoringModel ScoringModel { get; set; }

    /// <summary>Pass/Fail threshold</summary>
    public decimal PassThreshold { get; set; } = 0.7m;

    /// <summary>Warning threshold (below pass but above critical)</summary>
    public decimal WarningThreshold { get; set; } = 0.5m;

    /// <summary>Actions on evaluation failure</summary>
    public EvaluationFailureActions FailureActions { get; set; }

    /// <summary>Monitoring and alerting</summary>
    public EvaluationMonitoring Monitoring { get; set; }

    /// <summary>Is enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Applied to phases (Planning, Testing, Production)</summary>
    public List<string> AppliedToPhases { get; set; } = new();

    /// <summary>Evaluation results history</summary>
    public List<EvaluationResult> ResultsHistory { get; set; } = new();

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object CopilotApplicationId { get; set; }
    public object? Id { get; set; }
}

/// <summary>
/// Evaluation implementation details.
/// </summary>
public class EvaluationImplementation
{
    /// <summary>Evaluation framework/library used</summary>
    public string Framework { get; set; }

    /// <summary>Model for evaluation (if using ML-based evaluation)</summary>
    public string EvaluationModel { get; set; }

    /// <summary>Reference/golden data set</summary>
    public string ReferenceDataset { get; set; }

    /// <summary>Evaluation parameters</summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>Metrics to track</summary>
    public List<string> TrackedMetrics { get; set; } = new();

    /// <summary>Query for evaluation (if database-backed)</summary>
    public string EvaluationQuery { get; set; }
}

/// <summary>
/// Scoring model for evaluation results.
/// </summary>
public class ScoringModel
{
    /// <summary>Scoring method (Numeric, BooleanPass/Fail, Weighted, Normalized)</summary>
    public string Method { get; set; } = "Numeric";

    /// <summary>Scale (0-1, 0-100, etc.)</summary>
    public string Scale { get; set; } = "0-1";

    /// <summary>Weight factors for multi-criteria evaluation</summary>
    public Dictionary<string, decimal> WeightFactors { get; set; } = new();

    /// <summary>Normalization function if applicable</summary>
    public string NormalizationFunction { get; set; }
}

/// <summary>
/// Actions to take on evaluation failure.
/// </summary>
public class EvaluationFailureActions
{
    /// <summary>Block execution</summary>
    public bool BlockExecution { get; set; }

    /// <summary>Log the failure</summary>
    public bool LogFailure { get; set; } = true;

    /// <summary>Alert/notify</summary>
    public bool SendAlert { get; set; }

    /// <summary>Alert recipients</summary>
    public List<string> AlertRecipients { get; set; } = new();

    /// <summary>Require human review</summary>
    public bool RequireHumanReview { get; set; }

    /// <summary>Fallback action/response</summary>
    public string FallbackAction { get; set; }

    /// <summary>Retry logic</summary>
    public RetryConfig RetryLogic { get; set; }
}

/// <summary>
/// Monitoring configuration for evaluations.
/// </summary>
public class EvaluationMonitoring
{
    /// <summary>Track all evaluation runs</summary>
    public bool TrackAllRuns { get; set; } = true;

    /// <summary>Evaluation run frequency for reporting</summary>
    public string ReportingFrequency { get; set; } = "Daily"; // Hourly, Daily, Weekly, Monthly

    /// <summary>Trend analysis enabled</summary>
    public bool EnableTrendAnalysis { get; set; }

    /// <summary>Anomaly detection on evaluation scores</summary>
    public bool EnableAnomalyDetection { get; set; }

    /// <summary>SLA for evaluation pass rate (%)</summary>
    public decimal SLAPassRateTarget { get; set; } = 0.95m;

    /// <summary>Custom dashboard/report configuration</summary>
    public string DashboardConfig { get; set; }
}

/// <summary>
/// Result of a single evaluation run.
/// </summary>
public class EvaluationResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Evaluation ID this result belongs to</summary>
    public string EvaluationId { get; set; }

    /// <summary>Copilot ID being evaluated</summary>
    public string CopilotId { get; set; }

    /// <summary>Evaluation timestamp</summary>
    public DateTime EvaluationTime { get; set; }

    /// <summary>Score (0-1 or 0-100 depending on scale)</summary>
    public decimal Score { get; set; }

    /// <summary>Passed threshold</summary>
    public bool Passed { get; set; }

    /// <summary>Detailed results/breakdown</summary>
    public Dictionary<string, object> DetailedResults { get; set; } = new();

    /// <summary>Issues/warnings identified</summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>Recommendations</summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>Sample data used for evaluation</summary>
    public int SampleSize { get; set; }
}
