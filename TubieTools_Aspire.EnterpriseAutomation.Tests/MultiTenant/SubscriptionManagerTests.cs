using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.EnterpriseAutomation.Tests.MultiTenant
{
    public class SubscriptionManagerTests
    {
        private readonly Mock<ILogger<SubscriptionManager>> _mockLogger;
        private readonly SubscriptionManager _subscriptionManager;

        public SubscriptionManagerTests()
        {
            _mockLogger = new Mock<ILogger<SubscriptionManager>>();
            _subscriptionManager = new SubscriptionManager(_mockLogger.Object);
        }

        #region Tier Configuration Tests

        [Fact]
        public async Task GetTierConfig_WithFreeTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(SubscriptionTier.Free, config.Tier);
            Assert.Equal("Free", config.Name);
            Assert.Equal(0, config.MonthlyPrice);
            Assert.Equal(100, config.MonthlyApiCallLimit);
            Assert.Equal(20, config.DailyApiCallLimit);
            Assert.Single(config.AvailableTools);
            Assert.Contains("search_incident", config.AvailableTools);
        }

        [Fact]
        public async Task GetTierConfig_WithStarterTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(SubscriptionTier.Starter, config.Tier);
            Assert.Equal("Starter", config.Name);
            Assert.Equal(29, config.MonthlyPrice);
            Assert.Equal(5000, config.MonthlyApiCallLimit);
            Assert.Equal(200, config.DailyApiCallLimit);
            Assert.Equal(3, config.AvailableTools.Count);
            Assert.True(config.AllowCustomPrompts);
            Assert.False(config.AllowMultipleAgents);
        }

        [Fact]
        public async Task GetTierConfig_WithProfessionalTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(SubscriptionTier.Professional, config.Tier);
            Assert.Equal("Professional", config.Name);
            Assert.Equal(99, config.MonthlyPrice);
            Assert.Equal(50000, config.MonthlyApiCallLimit);
            Assert.True(config.AllowMultipleAgents);
            Assert.True(config.AllowWorkflowOrchestration);
            Assert.True(config.AllowWebhooks);
            Assert.True(config.PrioritySupport);
        }

        [Fact]
        public async Task GetTierConfig_WithEnterpriseTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(SubscriptionTier.Enterprise, config.Tier);
            Assert.Equal("Enterprise", config.Name);
            Assert.Equal(-1, config.MonthlyApiCallLimit); // Unlimited
            Assert.Equal(-1, config.DailyApiCallLimit); // Unlimited
            Assert.Contains("99.99%", config.SLA);
        }

        [Fact]
        public async Task GetAllTierConfigs_ReturnsAllFourTiers()
        {
            // Act
            var configs = await _subscriptionManager.GetAllTierConfigsAsync();

            // Assert
            Assert.Equal(4, configs.Count);
            Assert.Contains(configs, c => c.Tier == SubscriptionTier.Free);
            Assert.Contains(configs, c => c.Tier == SubscriptionTier.Starter);
            Assert.Contains(configs, c => c.Tier == SubscriptionTier.Professional);
            Assert.Contains(configs, c => c.Tier == SubscriptionTier.Enterprise);
        }

        #endregion

        #region Tool Access Control Tests

        [Fact]
        public async Task GetToolAccess_SearchIncident_AvailableForFreeTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("search_incident", SubscriptionTier.Free);

            // Assert
            Assert.NotNull(toolAccess);
            Assert.True(toolAccess.IsAvailable);
            Assert.Equal("search_incident", toolAccess.ToolName);
            Assert.Equal(SubscriptionTier.Free, toolAccess.MinimumTier);
            Assert.Equal(20, toolAccess.RateLimit);
        }

        [Fact]
        public async Task GetToolAccess_CreateIncident_NotAvailableForFreeTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("create_incident", SubscriptionTier.Free);

            // Assert
            Assert.NotNull(toolAccess);
            Assert.False(toolAccess.IsAvailable);
        }

        [Fact]
        public async Task GetToolAccess_CreateIncident_AvailableForStarterTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("create_incident", SubscriptionTier.Starter);

            // Assert
            Assert.NotNull(toolAccess);
            Assert.True(toolAccess.IsAvailable);
        }

        [Fact]
        public async Task GetToolAccess_CloseIncident_NotAvailableForFreeTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("close_incident", SubscriptionTier.Free);

            // Assert
            Assert.NotNull(toolAccess);
            Assert.False(toolAccess.IsAvailable);
        }

        [Fact]
        public async Task GetToolAccess_AllToolsAvailable_ForEnterpriseTier()
        {
            // Act
            var searchAccess = await _subscriptionManager.GetToolAccessAsync("search_incident", SubscriptionTier.Enterprise);
            var createAccess = await _subscriptionManager.GetToolAccessAsync("create_incident", SubscriptionTier.Enterprise);
            var closeAccess = await _subscriptionManager.GetToolAccessAsync("close_incident", SubscriptionTier.Enterprise);

            // Assert
            Assert.True(searchAccess.IsAvailable);
            Assert.True(createAccess.IsAvailable);
            Assert.True(closeAccess.IsAvailable);
        }

        #endregion

        #region Tool & Model Availability Tests

        [Fact]
        public async Task GetAvailableToolsForTier_FreeTier_ReturnsOnlySearch()
        {
            // Act
            var tools = await _subscriptionManager.GetAvailableToolsForTierAsync(SubscriptionTier.Free);

            // Assert
            Assert.Single(tools);
            Assert.Contains("search_incident", tools);
            Assert.DoesNotContain("create_incident", tools);
            Assert.DoesNotContain("close_incident", tools);
        }

        [Fact]
        public async Task GetAvailableToolsForTier_StarterTier_ReturnsAllTools()
        {
            // Act
            var tools = await _subscriptionManager.GetAvailableToolsForTierAsync(SubscriptionTier.Starter);

            // Assert
            Assert.Equal(3, tools.Count);
            Assert.Contains("create_incident", tools);
            Assert.Contains("search_incident", tools);
            Assert.Contains("close_incident", tools);
        }

        [Fact]
        public async Task GetAvailableModelsForTier_FreeTier_ReturnsGpt35Turbo()
        {
            // Act
            var models = await _subscriptionManager.GetAvailableModelsForTierAsync(SubscriptionTier.Free);

            // Assert
            Assert.Single(models);
            Assert.Contains("gpt-3.5-turbo", models);
        }

        [Fact]
        public async Task GetAvailableModelsForTier_StarterTier_ReturnsMultipleModels()
        {
            // Act
            var models = await _subscriptionManager.GetAvailableModelsForTierAsync(SubscriptionTier.Starter);

            // Assert
            Assert.Equal(2, models.Count);
            Assert.Contains("gpt-3.5-turbo", models);
            Assert.Contains("gpt-4", models);
        }

        [Fact]
        public async Task GetAvailableModelsForTier_ProfessionalTier_ReturnsAllModels()
        {
            // Act
            var models = await _subscriptionManager.GetAvailableModelsForTierAsync(SubscriptionTier.Professional);

            // Assert
            Assert.Equal(3, models.Count);
            Assert.Contains("gpt-3.5-turbo", models);
            Assert.Contains("gpt-4", models);
            Assert.Contains("gpt-4-turbo", models);
        }

        #endregion

        #region Feature Flag Tests

        [Fact]
        public async Task GetTierConfig_FreeTier_HasCorrectFeatureFlags()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.False(config.AllowCustomPrompts);
            Assert.False(config.AllowMultipleAgents);
            Assert.False(config.AllowWorkflowOrchestration);
            Assert.False(config.AllowAnalytics);
            Assert.False(config.AllowAPIAccess);
            Assert.False(config.AllowWebhooks);
            Assert.False(config.PrioritySupport);
        }

        [Fact]
        public async Task GetTierConfig_StarterTier_EnablesCustomPromptsAndAnalytics()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.True(config.AllowCustomPrompts);
            Assert.True(config.AllowAnalytics);
            Assert.True(config.AllowAPIAccess);
            Assert.False(config.AllowWebhooks);
            Assert.False(config.AllowMultipleAgents);
        }

        [Fact]
        public async Task GetTierConfig_ProfessionalTier_EnablesAllCoreFeatures()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.True(config.AllowCustomPrompts);
            Assert.True(config.AllowMultipleAgents);
            Assert.True(config.AllowWorkflowOrchestration);
            Assert.True(config.AllowAnalytics);
            Assert.True(config.AllowAPIAccess);
            Assert.True(config.AllowWebhooks);
            Assert.True(config.PrioritySupport);
        }

        [Fact]
        public async Task GetTierConfig_EnterpriseTier_EnablesAllFeatures()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.True(config.AllowCustomPrompts);
            Assert.True(config.AllowMultipleAgents);
            Assert.True(config.AllowWorkflowOrchestration);
            Assert.True(config.AllowAnalytics);
            Assert.True(config.AllowAPIAccess);
            Assert.True(config.AllowWebhooks);
            Assert.True(config.PrioritySupport);
            Assert.True(config.AllowDataExport);
            Assert.True(config.PublicAPI);
        }

        #endregion

        #region Team & Integration Limits Tests

        [Fact]
        public async Task GetTierConfig_FreeTier_LimitsSingleTeamMember()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.Equal(1, config.MaxTeamMembers);
            Assert.Equal(0, config.MaxCustomIntegrations);
        }

        [Fact]
        public async Task GetTierConfig_StarterTier_AllowsThreeTeamMembers()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.Equal(3, config.MaxTeamMembers);
            Assert.Equal(1, config.MaxCustomIntegrations);
        }

        [Fact]
        public async Task GetTierConfig_ProfessionalTier_AllowsTenTeamMembers()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.Equal(10, config.MaxTeamMembers);
            Assert.Equal(5, config.MaxCustomIntegrations);
        }

        [Fact]
        public async Task GetTierConfig_EnterpriseTier_AllowsUnlimitedTeamMembers()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.Equal(-1, config.MaxTeamMembers); // Unlimited
            Assert.Equal(-1, config.MaxCustomIntegrations); // Unlimited
        }

        #endregion

        #region Data Retention Tests

        [Fact]
        public async Task GetTierConfig_FreeTier_RetainsDataFor7Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.Equal(7, config.ConversationRetentionDays);
            Assert.Equal(7, config.LogRetentionDays);
        }

        [Fact]
        public async Task GetTierConfig_StarterTier_RetainsDataFor30Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.Equal(30, config.ConversationRetentionDays);
            Assert.Equal(30, config.LogRetentionDays);
        }

        [Fact]
        public async Task GetTierConfig_ProfessionalTier_RetainsDataFor90Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.Equal(90, config.ConversationRetentionDays);
            Assert.Equal(90, config.LogRetentionDays);
        }

        [Fact]
        public async Task GetTierConfig_EnterpriseTier_RetainsDataFor365Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.Equal(365, config.ConversationRetentionDays);
            Assert.Equal(365, config.LogRetentionDays);
        }

        #endregion

        #region SLA Tests

        [Fact]
        public async Task GetTierConfig_SLAsByTier_AreCorrect()
        {
            // Act
            var freeTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);
            var starterTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);
            var proTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);
            var enterpriseTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.Contains("Best effort", freeTier.SLA);
            Assert.Contains("99%", starterTier.SLA);
            Assert.Contains("99.5%", proTier.SLA);
            Assert.Contains("99.99%", enterpriseTier.SLA);
        }

        #endregion

        #region Subscription Management Tests

        [Fact]
        public async Task UpgradeTier_WithValidTenant_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.UpgradeTierAsync("test-tenant", SubscriptionTier.Starter);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DowngradeTier_WithValidTenant_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.DowngradeTierAsync("test-tenant", SubscriptionTier.Free);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CancelSubscription_WithValidTenant_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.CancelSubscriptionAsync("test-tenant");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Add-On Tests

        [Fact]
        public async Task AddSubscriptionAddOn_WithValidData_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.AddSubscriptionAddOnAsync("test-tenant", "priority-support");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RemoveSubscriptionAddOn_WithValidData_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.RemoveSubscriptionAddOnAsync("test-tenant", "priority-support");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetTenantAddOns_ReturnsAvailableAddOns()
        {
            // Act
            var addOns = await _subscriptionManager.GetTenantAddOnsAsync("test-tenant");

            // Assert
            Assert.NotEmpty(addOns);
            Assert.Contains(addOns, a => a.AddOnId == "priority-support");
        }

        #endregion
    }
}
