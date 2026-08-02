using System.Text.Json.Serialization;

namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{
    /// <summary>
    /// Options for loading tenant configuration from JSON with proper case mapping
    /// </summary>
    public class TenantConfigurationOptions
    {
        [JsonPropertyName("tenants")]
        public List<TenantConfig> Tenants { get; set; } = new();

        [JsonPropertyName("subscriptions")]
        public List<TenantSubscription> Subscriptions { get; set; } = new();
    }
}