using System.Text.Json.Serialization;

namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{
    /// <summary>
    /// Subscription tier levels
    /// </summary>
    public enum SubscriptionTier
    {
        Free = 0,
        Starter = 1,
        Professional = 2,
        Enterprise = 3
    }

    /// <summary>
    /// Represents a subscription tier with features and limits
    /// </summary>
    public class SubscriptionTierConfig
    {
        public SubscriptionTier Tier { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }

        // API Limits
        public int MonthlyApiCallLimit { get; set; }
        public int DailyApiCallLimit { get; set; }
        public int ConcurrentRequestLimit { get; set; }
        public int MaxConversationHistorySize { get; set; }

        // Feature Access
        public List<string> AvailableTools { get; set; } = new();
        public bool AllowCustomPrompts { get; set; }
        public bool AllowMultipleAgents { get; set; }
        public bool AllowWorkflowOrchestration { get; set; }
        public bool AllowAnalytics { get; set; }
        public bool AllowAPIAccess { get; set; }
        public bool AllowWebhooks { get; set; }
        public bool PrioritySupport { get; set; }

        // Model Access
        public List<string> AvailableModels { get; set; } = new();
        public decimal? MaxTemperature { get; set; }
        public int? MaxTokens { get; set; }

        // Retention Policy
        public int ConversationRetentionDays { get; set; }
        public int LogRetentionDays { get; set; }

        // Additional Features
        public int MaxTeamMembers { get; set; }
        public int MaxCustomIntegrations { get; set; }
        public bool AllowDataExport { get; set; }
        public bool PublicAPI { get; set; }
        public string SLA { get; set; } // e.g., "99.5% uptime"
    }

    /// <summary>
    /// Represents a tenant configuration
    /// </summary>
    public class TenantConfig
    {
        [JsonPropertyName("tenant_id")]
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string Description { get; set; }
        public SubscriptionTier CurrentTier { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public bool IsActive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public Dictionary<string, object> CustomMetadata { get; set; } = new();
    }

    /// <summary>
    /// Tenant subscription information
    /// </summary>
    public class TenantSubscription
    {
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public SubscriptionTier Tier { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RenewalDate { get; set; }
        public bool AutoRenew { get; set; }
        public decimal BillingAmount { get; set; }
        public string BillingInterval { get; set; } // "monthly", "yearly"
        public string Status { get; set; } // "active", "suspended", "expired", "cancelled"
        public List<SubscriptionAddOn> AddOns { get; set; } = new();
    }

    /// <summary>
    /// Add-on services for subscriptions
    /// </summary>
    public class SubscriptionAddOn
    {
        public string AddOnId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public Dictionary<string, object> Features { get; set; } = new();
    }

    /// <summary>
    /// Usage tracking for tenants
    /// </summary>
    public class TenantUsage
    {
        public string TenantId { get; set; }
        public DateTime Date { get; set; }
        public int ApiCallsUsed { get; set; }
        public decimal TransactionAmount { get; set; }
        public Dictionary<string, int> ToolUsageStats { get; set; } = new();
        public Dictionary<string, int> ModelUsageStats { get; set; } = new();
        public int ConversationsCreated { get; set; }
        public int TokensUsed { get; set; }
    }

    /// <summary>
    /// Usage quota limits for a tenant
    /// </summary>
    public class TenantQuota
    {
        public string TenantId { get; set; }
        public int MonthlyApiCallLimit { get; set; }
        public int MonthlyApiCallsUsed { get; set; }
        public int DailyApiCallLimit { get; set; }
        public int DailyApiCallsUsed { get; set; }
        public DateTime ResetDate { get; set; }
        public bool QuotaExceeded { get; set; }
    }

    /// <summary>
    /// Represents a custom agent configuration for a tenant
    /// </summary>
    public class TenantCustomAgent
    {
        public string AgentId { get; set; }
        public string TenantId { get; set; }
        public string AgentName { get; set; }
        public string SystemPrompt { get; set; }
        public List<string> AssignedTools { get; set; } = new();
        public string PreferredModel { get; set; }
        public Dictionary<string, object> ModelParameters { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    /// <summary>
    /// Team member for a tenant
    /// </summary>
    public class TenantTeamMember
    {
        public string MemberId { get; set; }
        public string TenantId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // "admin", "user", "viewer"
        public List<string> AvailableAgents { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime JoinedDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    /// <summary>
    /// Feature access control for a tool based on subscription tier
    /// </summary>
    public class ToolFeatureAccess
    {
        public string ToolName { get; set; }
        public SubscriptionTier MinimumTier { get; set; }
        public bool IsAvailable { get; set; }
        public int? RateLimit { get; set; } // calls per minute
        public Dictionary<string, string> RestrictedParameters { get; set; } = new();
    }

    /// <summary>
    /// Billing record for a tenant
    /// </summary>
    public class TenantBillingRecord
    {
        public string BillingId { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } // "pending", "paid", "overdue", "cancelled"
        public string PaymentMethod { get; set; }
        public Dictionary<string, decimal> LineItems { get; set; } = new();
    }
}
