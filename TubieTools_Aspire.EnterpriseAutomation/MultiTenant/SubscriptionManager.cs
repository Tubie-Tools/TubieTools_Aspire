using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{
    /// <summary>
    /// Interface for subscription management
    /// </summary>
    public interface ISubscriptionManager
    {
        Task<SubscriptionTierConfig> GetTierConfigAsync(SubscriptionTier tier);
        Task<List<SubscriptionTierConfig>> GetAllTierConfigsAsync();
        Task<bool> UpgradeTierAsync(string tenantId, SubscriptionTier newTier);
        Task<bool> DowngradeTierAsync(string tenantId, SubscriptionTier newTier);
        Task<bool> CancelSubscriptionAsync(string tenantId);
        Task<Dictionary<string, bool>> GetTenantFeaturesAsync(string tenantId);
        Task<List<string>> GetAvailableToolsForTierAsync(SubscriptionTier tier);
        Task<List<string>> GetAvailableModelsForTierAsync(SubscriptionTier tier);
        Task<ToolFeatureAccess> GetToolAccessAsync(string toolName, SubscriptionTier tier);
        Task<bool> AddSubscriptionAddOnAsync(string tenantId, string addOnId);
        Task<bool> RemoveSubscriptionAddOnAsync(string tenantId, string addOnId);
        Task<List<SubscriptionAddOn>> GetTenantAddOnsAsync(string tenantId);
    }

    /// <summary>
    /// Implementation of subscription manager
    /// </summary>
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly Dictionary<SubscriptionTier, SubscriptionTierConfig> _tierConfigs;
        private readonly Dictionary<string, ToolFeatureAccess> _toolAccess;
        private readonly List<SubscriptionAddOn> _availableAddOns;

        public SubscriptionManager(ILogger<SubscriptionManager> logger)
        {
            _logger = logger;
            _tierConfigs = new Dictionary<SubscriptionTier, SubscriptionTierConfig>();
            _toolAccess = new Dictionary<string, ToolFeatureAccess>();
            _availableAddOns = new List<SubscriptionAddOn>();
            InitializeDefaultConfigs();
        }

        private void InitializeDefaultConfigs()
        {
            // Free Tier
            _tierConfigs[SubscriptionTier.Free] = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Free,
                Name = "Free",
                Description = "Perfect for getting started with AI agents",
                MonthlyPrice = 0,
                AnnualPrice = 0,
                MonthlyApiCallLimit = 100,
                DailyApiCallLimit = 20,
                ConcurrentRequestLimit = 1,
                MaxConversationHistorySize = 10,
                AvailableTools = new List<string> { "search_incident" },
                AllowCustomPrompts = false,
                AllowMultipleAgents = false,
                AllowWorkflowOrchestration = false,
                AllowAnalytics = false,
                AllowAPIAccess = false,
                AllowWebhooks = false,
                PrioritySupport = false,
                AvailableModels = new List<string> { "gpt-3.5-turbo" },
                MaxTemperature = 0.7m,
                MaxTokens = 500,
                ConversationRetentionDays = 7,
                LogRetentionDays = 7,
                MaxTeamMembers = 1,
                MaxCustomIntegrations = 0,
                AllowDataExport = false,
                PublicAPI = false,
                SLA = "Best effort"
            };

            // Starter Tier
            _tierConfigs[SubscriptionTier.Starter] = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                Name = "Starter",
                Description = "Great for individual developers and small teams",
                MonthlyPrice = 29,
                AnnualPrice = 290,
                MonthlyApiCallLimit = 5000,
                DailyApiCallLimit = 200,
                ConcurrentRequestLimit = 5,
                MaxConversationHistorySize = 50,
                AvailableTools = new List<string> { "create_incident", "search_incident", "close_incident" },
                AllowCustomPrompts = true,
                AllowMultipleAgents = false,
                AllowWorkflowOrchestration = false,
                AllowAnalytics = true,
                AllowAPIAccess = true,
                AllowWebhooks = false,
                PrioritySupport = false,
                AvailableModels = new List<string> { "gpt-3.5-turbo", "gpt-4" },
                MaxTemperature = 1.0m,
                MaxTokens = 1000,
                ConversationRetentionDays = 30,
                LogRetentionDays = 30,
                MaxTeamMembers = 3,
                MaxCustomIntegrations = 1,
                AllowDataExport = true,
                PublicAPI = false,
                SLA = "99% uptime"
            };

            // Professional Tier
            _tierConfigs[SubscriptionTier.Professional] = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Professional,
                Name = "Professional",
                Description = "For growing teams and production use cases",
                MonthlyPrice = 99,
                AnnualPrice = 990,
                MonthlyApiCallLimit = 50000,
                DailyApiCallLimit = 2000,
                ConcurrentRequestLimit = 20,
                MaxConversationHistorySize = 200,
                AvailableTools = new List<string> { "create_incident", "search_incident", "close_incident" },
                AllowCustomPrompts = true,
                AllowMultipleAgents = true,
                AllowWorkflowOrchestration = true,
                AllowAnalytics = true,
                AllowAPIAccess = true,
                AllowWebhooks = true,
                PrioritySupport = true,
                AvailableModels = new List<string> { "gpt-3.5-turbo", "gpt-4", "gpt-4-turbo" },
                MaxTemperature = 2.0m,
                MaxTokens = 2000,
                ConversationRetentionDays = 90,
                LogRetentionDays = 90,
                MaxTeamMembers = 10,
                MaxCustomIntegrations = 5,
                AllowDataExport = true,
                PublicAPI = true,
                SLA = "99.5% uptime"
            };

            // Enterprise Tier
            _tierConfigs[SubscriptionTier.Enterprise] = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Enterprise,
                Name = "Enterprise",
                Description = "For large organizations with advanced requirements",
                MonthlyPrice = 0, // Custom pricing
                AnnualPrice = 0,
                MonthlyApiCallLimit = -1, // Unlimited
                DailyApiCallLimit = -1,
                ConcurrentRequestLimit = 100,
                MaxConversationHistorySize = -1,
                AvailableTools = new List<string> { "create_incident", "search_incident", "close_incident" },
                AllowCustomPrompts = true,
                AllowMultipleAgents = true,
                AllowWorkflowOrchestration = true,
                AllowAnalytics = true,
                AllowAPIAccess = true,
                AllowWebhooks = true,
                PrioritySupport = true,
                AvailableModels = new List<string> { "gpt-3.5-turbo", "gpt-4", "gpt-4-turbo" },
                MaxTemperature = 2.0m,
                MaxTokens = -1,
                ConversationRetentionDays = 365,
                LogRetentionDays = 365,
                MaxTeamMembers = -1,
                MaxCustomIntegrations = -1,
                AllowDataExport = true,
                PublicAPI = true,
                SLA = "99.99% uptime, dedicated support"
            };

            InitializeToolAccess();
            InitializeAddOns();
        }

        private void InitializeToolAccess()
        {
            _toolAccess["create_incident"] = new ToolFeatureAccess
            {
                ToolName = "create_incident",
                MinimumTier = SubscriptionTier.Starter,
                IsAvailable = true,
                RateLimit = 10
            };

            _toolAccess["search_incident"] = new ToolFeatureAccess
            {
                ToolName = "search_incident",
                MinimumTier = SubscriptionTier.Free,
                IsAvailable = true,
                RateLimit = 20
            };

            _toolAccess["close_incident"] = new ToolFeatureAccess
            {
                ToolName = "close_incident",
                MinimumTier = SubscriptionTier.Starter,
                IsAvailable = true,
                RateLimit = 5
            };
        }

        private void InitializeAddOns()
        {
            _availableAddOns.Add(new SubscriptionAddOn
            {
                AddOnId = "priority-support",
                Name = "Priority Support",
                Description = "24/7 priority support with 1-hour response time",
                MonthlyPrice = 50
            });

            _availableAddOns.Add(new SubscriptionAddOn
            {
                AddOnId = "advanced-analytics",
                Name = "Advanced Analytics",
                Description = "Deep insights into agent performance",
                MonthlyPrice = 25
            });

            _availableAddOns.Add(new SubscriptionAddOn
            {
                AddOnId = "sso",
                Name = "Single Sign-On",
                Description = "Enterprise SSO with SAML support",
                MonthlyPrice = 75
            });
        }

        public async Task<SubscriptionTierConfig> GetTierConfigAsync(SubscriptionTier tier)
        {
            return _tierConfigs.TryGetValue(tier, out var config) ? config : null;
        }

        public async Task<List<SubscriptionTierConfig>> GetAllTierConfigsAsync()
        {
            return _tierConfigs.Values.ToList();
        }

        public async Task<bool> UpgradeTierAsync(string tenantId, SubscriptionTier newTier)
        {
            try
            {
                _logger.LogInformation("Upgrading tenant {TenantId} to tier {Tier}", tenantId, newTier);
                // Implementation would update database
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upgrading tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<bool> DowngradeTierAsync(string tenantId, SubscriptionTier newTier)
        {
            try
            {
                _logger.LogInformation("Downgrading tenant {TenantId} to tier {Tier}", tenantId, newTier);
                // Implementation would update database
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downgrading tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<bool> CancelSubscriptionAsync(string tenantId)
        {
            try
            {
                _logger.LogInformation("Cancelling subscription for tenant {TenantId}", tenantId);
                // Implementation would update database
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription for tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<Dictionary<string, bool>> GetTenantFeaturesAsync(string tenantId)
        {
            try
            {
                // This would normally fetch from database, for now returning empty dict
                return new Dictionary<string, bool>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving features for tenant {TenantId}", tenantId);
                return new Dictionary<string, bool>();
            }
        }

        public async Task<List<string>> GetAvailableToolsForTierAsync(SubscriptionTier tier)
        {
            var config = await GetTierConfigAsync(tier);
            return config?.AvailableTools ?? new List<string>();
        }

        public async Task<List<string>> GetAvailableModelsForTierAsync(SubscriptionTier tier)
        {
            var config = await GetTierConfigAsync(tier);
            return config?.AvailableModels ?? new List<string>();
        }

        public async Task<ToolFeatureAccess> GetToolAccessAsync(string toolName, SubscriptionTier tier)
        {
            if (!_toolAccess.TryGetValue(toolName, out var access))
                return null;

            if (tier < access.MinimumTier)
                return new ToolFeatureAccess { ToolName = toolName, IsAvailable = false };

            return access;
        }

        public async Task<bool> AddSubscriptionAddOnAsync(string tenantId, string addOnId)
        {
            try
            {
                _logger.LogInformation("Adding add-on {AddOnId} to tenant {TenantId}", addOnId, tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding add-on to tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<bool> RemoveSubscriptionAddOnAsync(string tenantId, string addOnId)
        {
            try
            {
                _logger.LogInformation("Removing add-on {AddOnId} from tenant {TenantId}", addOnId, tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing add-on from tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task<List<SubscriptionAddOn>> GetTenantAddOnsAsync(string tenantId)
        {
            try
            {
                // This would normally fetch from database
                return _availableAddOns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving add-ons for tenant {TenantId}", tenantId);
                return new List<SubscriptionAddOn>();
            }
        }
    }
}
