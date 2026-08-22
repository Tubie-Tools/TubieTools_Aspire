namespace TubieTools_Aspire.EnterpriseAutomation;

/// <summary>
/// Constants for CA Framework AI adoption phases and AI agent lifecycle.
/// Implements Microsoft CAF guidance for enterprise AI automation.
/// </summary>
public static class EnterpriseAutomationConstants
{
    /// <summary>
    /// CAF AI Adoption Phases
    /// </summary>
    public static class AdoptionPhases
    {
        /// <summary>Align on business goals, vision, and outcomes for AI</summary>
        public const string Strategy = "Strategy";

        /// <summary>Assess AI capabilities, create roadmaps, identify opportunities</summary>
        public const string Plan = "Plan";

        /// <summary>Build foundations, acquire skills, establish infrastructure</summary>
        public const string Ready = "Ready";

        /// <summary>Establish policies, controls, compliance frameworks</summary>
        public const string Govern = "Govern";

        /// <summary>Implement security, data protection, access controls</summary>
        public const string Secure = "Secure";

        /// <summary>Monitor, optimize, maintain, and continuously improve</summary>
        public const string Manage = "Manage";

        public static IEnumerable<string> AllPhases => new[]
        {
            Strategy, Plan, Ready, Govern, Secure, Manage
        };
    }

    /// <summary>
    /// AI Agent Lifecycle Phases
    /// </summary>
    public static class AgentLifecyclePhases
    {
        /// <summary>Design, architecture, and capability planning</summary>
        public const string PlanAgents = "PlanAgents";

        /// <summary>Establish governance, policies, and security controls</summary>
        public const string GovernAndSecureAgents = "GovernAndSecureAgents";

        /// <summary>Development, training, testing, and deployment</summary>
        public const string BuildAgents = "BuildAgents";

        /// <summary>Monitoring, optimization, maintenance, and incident response</summary>
        public const string OperateAgents = "OperateAgents";

        public static IEnumerable<string> AllPhases => new[]
        {
            PlanAgents, GovernAndSecureAgents, BuildAgents, OperateAgents
        };
    }

    /// <summary>
    /// AI Agent Types supported by the framework
    /// </summary>
    public static class AgentTypes
    {
        public const string TaskAutomation = "TaskAutomation";
        public const string DataAnalysis = "DataAnalysis";
        public const string CustomerService = "CustomerService";
        public const string ProcessOptimization = "ProcessOptimization";
        public const string DecisionSupport = "DecisionSupport";
        public const string ContentGeneration = "ContentGeneration";
        public const string CodeGeneration = "CodeGeneration";
        public const string Custom = "Custom";
    }

    /// <summary>
    /// Risk levels for AI initiatives
    /// </summary>
    public static class RiskLevels
    {
        public const string Critical = "Critical";
        public const string High = "High";
        public const string Medium = "Medium";
        public const string Low = "Low";
        public const string Minimal = "Minimal";
    }

    /// <summary>
    /// Governance policy categories
    /// </summary>
    public static class PolicyCategories
    {
        public const string DataGovernance = "DataGovernance";
        public const string AccessControl = "AccessControl";
        public const string ComplianceRequirements = "ComplianceRequirements";
        public const string SecurityStandards = "SecurityStandards";
        public const string AuditLogging = "AuditLogging";
        public const string UsageMonitoring = "UsageMonitoring";
        public const string BiasAndFairness = "BiasAndFairness";
        public const string TransparencyAndExplainability = "TransparencyAndExplainability";
    }
}
