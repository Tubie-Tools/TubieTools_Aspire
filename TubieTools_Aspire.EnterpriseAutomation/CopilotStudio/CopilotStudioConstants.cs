namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio;

/// <summary>
/// Constants for Copilot Studio and Foundry development patterns and guidelines.
/// Aligns with cloud landing zones and enterprise architecture.
/// </summary>
public static class CopilotStudioConstants
{
    /// <summary>
    /// Copilot Studio Tool Classifications
    /// </summary>
    public static class ToolTypes
    {
        /// <summary>Knowledge retrieval tools - Search, RAG, documentation lookup</summary>
        public const string KnowledgeTools = "KnowledgeTools";

        /// <summary>Action execution tools - API calls, data manipulation, process execution</summary>
        public const string ActionTools = "ActionTools";

        /// <summary>Trigger/Event-based tools - Webhooks, schedulers, event listeners</summary>
        public const string TriggerTools = "TriggerTools";

        /// <summary>Evaluation/Validation tools - Quality checks, compliance validation</summary>
        public const string EvaluationTools = "EvaluationTools";

        /// <summary>Orchestration tools - Multi-step workflows, conditional logic</summary>
        public const string OrchestrationTools = "OrchestrationTools";

        public static IEnumerable<string> AllToolTypes => new[]
        {
            KnowledgeTools, ActionTools, TriggerTools, EvaluationTools, OrchestrationTools
        };
    }

    /// <summary>
    /// Knowledge Tool Patterns
    /// </summary>
    public static class KnowledgeToolPatterns
    {
        /// <summary>Vector database search (Semantic search, embedding-based)</summary>
        public const string VectorSearch = "VectorSearch";

        /// <summary>Retrieval Augmented Generation for context-aware responses</summary>
        public const string RAG = "RAG";

        /// <summary>SQL database queries for structured data</summary>
        public const string StructuredQuery = "StructuredQuery";

        /// <summary>File/document search and retrieval</summary>
        public const string DocumentSearch = "DocumentSearch";

        /// <summary>Graph database queries for relationship traversal</summary>
        public const string GraphQuery = "GraphQuery";

        /// <summary>Multi-source aggregation/federation</summary>
        public const string FederatedSearch = "FederatedSearch";

        public static IEnumerable<string> AllPatterns => new[]
        {
            VectorSearch, RAG, StructuredQuery, DocumentSearch, GraphQuery, FederatedSearch
        };
    }

    /// <summary>
    /// Action Tool Patterns
    /// </summary>
    public static class ActionToolPatterns
    {
        /// <summary>REST API calls with rate limiting and retry logic</summary>
        public const string RESTAPICall = "RESTAPICall";

        /// <summary>Database CRUD operations</summary>
        public const string DatabaseOperation = "DatabaseOperation";

        /// <summary>File operations (read, write, delete)</summary>
        public const string FileOperation = "FileOperation";

        /// <summary>Email/notification sending</summary>
        public const string NotificationAction = "NotificationAction";

        /// <summary>Data transformation and enrichment</summary>
        public const string DataTransformation = "DataTransformation";

        /// <summary>Business process invocation</summary>
        public const string ProcessInvocation = "ProcessInvocation";

        /// <summary>Third-party service integration (Dynamics, Salesforce, etc.)</summary>
        public const string ThirdPartyIntegration = "ThirdPartyIntegration";

        /// <summary>Machine learning model invocation</summary>
        public const string MLModelInvocation = "MLModelInvocation";

        public static IEnumerable<string> AllPatterns => new[]
        {
            RESTAPICall, DatabaseOperation, FileOperation, NotificationAction,
            DataTransformation, ProcessInvocation, ThirdPartyIntegration, MLModelInvocation
        };
    }

    /// <summary>
    /// Trigger/Event Patterns
    /// </summary>
    public static class TriggerPatterns
    {
        /// <summary>Scheduled/time-based triggers (cron expressions)</summary>
        public const string ScheduledTrigger = "ScheduledTrigger";

        /// <summary>Event-based triggers from message queues</summary>
        public const string EventQueueTrigger = "EventQueueTrigger";

        /// <summary>Webhook/HTTP endpoint triggers</summary>
        public const string WebhookTrigger = "WebhookTrigger";

        /// <summary>Database change triggers (row-level, CDC)</summary>
        public const string DatabaseChangeTrigger = "DatabaseChangeTrigger";

        /// <summary>Manual/user-initiated triggers</summary>
        public const string ManualTrigger = "ManualTrigger";

        /// <summary>Conditional triggers based on state/metrics</summary>
        public const string ConditionalTrigger = "ConditionalTrigger";

        /// <summary>Integration platform triggers (Logic Apps, Power Automate)</summary>
        public const string IntegrationPlatformTrigger = "IntegrationPlatformTrigger";

        public static IEnumerable<string> AllPatterns => new[]
        {
            ScheduledTrigger, EventQueueTrigger, WebhookTrigger, DatabaseChangeTrigger,
            ManualTrigger, ConditionalTrigger, IntegrationPlatformTrigger
        };
    }

    /// <summary>
    /// Evaluation/Quality Check Patterns
    /// </summary>
    public static class EvaluationPatterns
    {
        /// <summary>Semantic similarity evaluation for relevance</summary>
        public const string SemanticSimilarity = "SemanticSimilarity";

        /// <summary>Compliance validation against policies/rules</summary>
        public const string ComplianceValidation = "ComplianceValidation";

        /// <summary>Data quality checks (completeness, accuracy, format)</summary>
        public const string DataQualityCheck = "DataQualityCheck";

        /// <summary>Security/safety evaluation of responses</summary>
        public const string SafetyEvaluation = "SafetyEvaluation";

        /// <summary>Hallucination detection in LLM responses</summary>
        public const string HallucinationDetection = "HallucinationDetection";

        /// <summary>Factual grounding evaluation</summary>
        public const string FactualGrounding = "FactualGrounding";

        /// <summary>User feedback-based evaluation</summary>
        public const string UserFeedbackEvaluation = "UserFeedbackEvaluation";

        /// <summary>Latency and performance evaluation</summary>
        public const string PerformanceEvaluation = "PerformanceEvaluation";

        public static IEnumerable<string> AllPatterns => new[]
        {
            SemanticSimilarity, ComplianceValidation, DataQualityCheck, SafetyEvaluation,
            HallucinationDetection, FactualGrounding, UserFeedbackEvaluation, PerformanceEvaluation
        };
    }

    /// <summary>
    /// Cloud Landing Zones
    /// </summary>
    public static class LandingZones
    {
        /// <summary>Corporate landing zone for regulated/sensitive workloads</summary>
        public const string Corporate = "Corporate";

        /// <summary>Online landing zone for internet-facing applications</summary>
        public const string Online = "Online";

        /// <summary>Sandbox/experimentation landing zone</summary>
        public const string Sandbox = "Sandbox";

        /// <summary>Data landing zone for analytics and reporting</summary>
        public const string DataLandingZone = "DataLandingZone";

        /// <summary>AI/ML landing zone for model development and operations</summary>
        public const string AIMLLandingZone = "AIMLLandingZone";

        public static IEnumerable<string> AllLandingZones => new[]
        {
            Corporate, Online, Sandbox, DataLandingZone, AIMLLandingZone
        };
    }

    /// <summary>
    /// Deployment Environments
    /// </summary>
    public static class DeploymentEnvironments
    {
        public const string Development = "Development";
        public const string Testing = "Testing";
        public const string Staging = "Staging";
        public const string Production = "Production";
        public const string DisasterRecovery = "DisasterRecovery";
    }

    /// <summary>
    /// Copilot Maturity Levels
    /// </summary>
    public static class MaturityLevels
    {
        /// <summary>Level 1: Basic copilot with simple Q&A</summary>
        public const string Basic = "Basic";

        /// <summary>Level 2: Enhanced with knowledge base and some actions</summary>
        public const string Enhanced = "Enhanced";

        /// <summary>Level 3: Advanced with complex workflows and evaluations</summary>
        public const string Advanced = "Advanced";

        /// <summary>Level 4: Expert with full automation and AI optimization</summary>
        public const string Expert = "Expert";

        /// <summary>Level 5: Enterprise with full governance and compliance</summary>
        public const string Enterprise = "Enterprise";

        public static IEnumerable<string> AllLevels => new[]
        {
            Basic, Enhanced, Advanced, Expert, Enterprise
        };
    }

    /// <summary>
    /// Copilot Capabilities
    /// </summary>
    public static class Capabilities
    {
        public const string QuestionAnswering = "QuestionAnswering";
        public const string TaskExecution = "TaskExecution";
        public const string DataRetrieval = "DataRetrieval";
        public const string ProcessAutomation = "ProcessAutomation";
        public const string DecisionSupport = "DecisionSupport";
        public const string ContentGeneration = "ContentGeneration";
        public const string TranslationAndLocalization = "TranslationAndLocalization";
        public const string SentimentAnalysis = "SentimentAnalysis";
    }

    /// <summary>
    /// Integration/Foundry Framework Types
    /// </summary>
    public static class FoundryFrameworks
    {
        public const string PowerPlatform = "PowerPlatform";
        public const string AzureIntegrationServices = "AzureIntegrationServices";
        public const string MuleSoft = "MuleSoft";
        public const string Boomi = "Boomi";
        public const string Zapier = "Zapier";
        public const string Custom = "Custom";
    }
}
