namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{
    /// <summary>
    /// Represents the current tenant context in a request
    /// </summary>
    public class TenantContext
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public SubscriptionTier CurrentTier { get; set; }
        public TenantSubscription Subscription { get; set; }
        public TenantQuota Quota { get; set; }
        public bool QuotaExceeded { get; set; } = false;
        public List<string> AvailableTools { get; set; } = new();
        public List<string> AvailableModels { get; set; } = new();
        public bool IsActive { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new();
    }

    /// <summary>
    /// Interface for tenant context accessor
    /// </summary>
    public interface ITenantContextAccessor
    {
        TenantContext TenantContext { get; set; }
        string GetTenantId();
        SubscriptionTier GetTenantTier();
        bool HasFeature(string featureName);
        bool CanAccessTool(string toolName);
        bool CanAccessModel(string modelName);
        void SetTenantContext(TenantContext tenantContext);
    }

    /// <summary>
    /// Implementation of tenant context accessor
    /// </summary>
    public class TenantContextAccessor : ITenantContextAccessor
    {
        public TenantContext TenantContext { get; set; }

        public string GetTenantId() => TenantContext?.TenantId;

        public SubscriptionTier GetTenantTier() => TenantContext?.CurrentTier ?? SubscriptionTier.Free;

        public bool HasFeature(string featureName)
        {
            if (TenantContext?.Features == null)
                return false;

            return TenantContext.Features.TryGetValue(featureName, out var hasFeature) && hasFeature;
        }

        public bool CanAccessTool(string toolName)
        {
            if (TenantContext?.AvailableTools == null)
                return false;

            return TenantContext.AvailableTools.Contains(toolName);
        }

        public bool CanAccessModel(string modelName)
        {
            if (TenantContext?.AvailableModels == null)
                return false;

            return TenantContext.AvailableModels.Contains(modelName);
        }

        public void SetTenantContext(TenantContext tenantContext)
        {
            throw new NotImplementedException();
        }
    }
}
