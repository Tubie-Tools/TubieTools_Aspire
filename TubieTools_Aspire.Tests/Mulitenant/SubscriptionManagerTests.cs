using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Mulitenant
{
    [TestClass]
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

        [TestMethod]
        public async Task GetTierConfig_WithFreeTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(SubscriptionTier.Free, config.Tier);
            Assert.AreEqual("Free", config.Name);
            Assert.AreEqual(0, config.MonthlyPrice);
            Assert.AreEqual(100, config.MonthlyApiCallLimit);
            Assert.AreEqual(20, config.DailyApiCallLimit);
            Assert.AreEqual(1, config.AvailableTools.Count);
        }

        [TestMethod]
        public async Task GetTierConfig_WithStarterTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(SubscriptionTier.Starter, config.Tier);
            Assert.AreEqual("Starter", config.Name);
            Assert.AreEqual(29, config.MonthlyPrice);
            Assert.AreEqual(5000, config.MonthlyApiCallLimit);
            Assert.AreEqual(200, config.DailyApiCallLimit);
            Assert.AreEqual(3, config.AvailableTools.Count);
            Assert.IsTrue(config.AllowCustomPrompts);
            Assert.IsFalse(config.AllowMultipleAgents);
        }

        [TestMethod]
        public async Task GetTierConfig_WithProfessionalTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(SubscriptionTier.Professional, config.Tier);
            Assert.AreEqual("Professional", config.Name);
            Assert.AreEqual(99, config.MonthlyPrice);
            Assert.AreEqual(50000, config.MonthlyApiCallLimit);
            Assert.IsTrue(config.AllowMultipleAgents);
            Assert.IsTrue(config.AllowWorkflowOrchestration);
            Assert.IsTrue(config.AllowWebhooks);
            Assert.IsTrue(config.PrioritySupport);
        }

        [TestMethod]
        public async Task GetTierConfig_WithEnterpriseTier_ReturnsCorrectConfig()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(SubscriptionTier.Enterprise, config.Tier);
            Assert.AreEqual("Enterprise", config.Name);
            Assert.AreEqual(-1, config.MonthlyApiCallLimit); // Unlimited
            Assert.AreEqual(-1, config.DailyApiCallLimit); // Unlimited
            Assert.IsTrue(config.SLA.Contains("99.99%"));
        }

        [TestMethod]
        public async Task GetAllTierConfigs_ReturnsAllFourTiers()
        {
            // Act
            var configs = await _subscriptionManager.GetAllTierConfigsAsync();

            // Assert
            Assert.AreEqual(4, configs.Count);
            Assert.IsTrue(configs.Exists(c => c.Tier == SubscriptionTier.Free));
            Assert.IsTrue(configs.Exists(c => c.Tier == SubscriptionTier.Starter));
            Assert.IsTrue(configs.Exists(c => c.Tier == SubscriptionTier.Professional));
            Assert.IsTrue(configs.Exists(c => c.Tier == SubscriptionTier.Enterprise));
        }

        #endregion

        #region Tool Access Control Tests

        [TestMethod]
        public async Task GetToolAccess_SearchIncident_AvailableForFreeTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("search_incident", SubscriptionTier.Free);

            // Assert
            Assert.IsNotNull(toolAccess);
            Assert.IsTrue(toolAccess.IsAvailable);
            Assert.AreEqual("search_incident", toolAccess.ToolName);
            Assert.AreEqual(SubscriptionTier.Free, toolAccess.MinimumTier);
            Assert.AreEqual(20, toolAccess.RateLimit);
        }

        [TestMethod]
        public async Task GetToolAccess_CreateIncident_NotAvailableForFreeTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("create_incident", SubscriptionTier.Free);

            // Assert
            Assert.IsNotNull(toolAccess);
            Assert.IsFalse(toolAccess.IsAvailable);
        }

        [TestMethod]
        public async Task GetToolAccess_CreateIncident_AvailableForStarterTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("create_incident", SubscriptionTier.Starter);

            // Assert
            Assert.IsNotNull(toolAccess);
            Assert.IsTrue(toolAccess.IsAvailable);
        }

        [TestMethod]
        public async Task GetToolAccess_CloseIncident_NotAvailableForFreeTier()
        {
            // Act
            var toolAccess = await _subscriptionManager.GetToolAccessAsync("close_incident", SubscriptionTier.Free);

            // Assert
            Assert.IsNotNull(toolAccess);
            Assert.IsFalse(toolAccess.IsAvailable);
        }

        [TestMethod]
        public async Task GetToolAccess_AllToolsAvailable_ForEnterpriseTier()
        {
            // Act
            var searchAccess = await _subscriptionManager.GetToolAccessAsync("search_incident", SubscriptionTier.Enterprise);
            var createAccess = await _subscriptionManager.GetToolAccessAsync("create_incident", SubscriptionTier.Enterprise);
            var closeAccess = await _subscriptionManager.GetToolAccessAsync("close_incident", SubscriptionTier.Enterprise);

            // Assert
            Assert.IsTrue(searchAccess.IsAvailable);
            Assert.IsTrue(createAccess.IsAvailable);
            Assert.IsTrue(closeAccess.IsAvailable);
        }

        #endregion

        #region Tool & Model Availability Tests

        [TestMethod]
        public async Task GetAvailableToolsForTier_FreeTier_ReturnsOnlySearch()
        {
            // Act
            var tools = await _subscriptionManager.GetAvailableToolsForTierAsync(SubscriptionTier.Free);

            // Assert
            Assert.AreEqual(1, tools.Count);
            Assert.IsTrue(tools.Contains("search_incident"));
            Assert.IsFalse(tools.Contains("create_incident"));
            Assert.IsFalse(tools.Contains("close_incident"));
        }

        [TestMethod]
        public async Task GetAvailableToolsForTier_StarterTier_ReturnsAllTools()
        {
            // Act
            var tools = await _subscriptionManager.GetAvailableToolsForTierAsync(SubscriptionTier.Starter);

            // Assert
            Assert.AreEqual(3, tools.Count);
            Assert.IsTrue(tools.Contains("create_incident"));
            Assert.IsTrue(tools.Contains("search_incident"));
            Assert.IsTrue(tools.Contains("close_incident"));
        }

        [TestMethod]
        public async Task GetAvailableModelsForTier_FreeTier_ReturnsGpt35Turbo()
        {
            // Act
            var models = await _subscriptionManager.GetAvailableModelsForTierAsync(SubscriptionTier.Free);

            // Assert
            Assert.AreEqual(1, models.Count);
            Assert.IsTrue(models.Contains("gpt-3.5-turbo"));
        }

        [TestMethod]
        public async Task GetAvailableModelsForTier_StarterTier_ReturnsMultipleModels()
        {
            // Act
            var models = await _subscriptionManager.GetAvailableModelsForTierAsync(SubscriptionTier.Starter);

            // Assert
            Assert.AreEqual(2, models.Count);
            Assert.IsTrue(models.Contains("gpt-3.5-turbo"));
            Assert.IsTrue(models.Contains("gpt-4"));
        }

        [TestMethod]
        public async Task GetAvailableModelsForTier_ProfessionalTier_ReturnsAllModels()
        {
            // Act
            var models = await _subscriptionManager.GetAvailableModelsForTierAsync(SubscriptionTier.Professional);

            // Assert
            Assert.AreEqual(3, models.Count);
            Assert.IsTrue(models.Contains("gpt-3.5-turbo"));
            Assert.IsTrue(models.Contains("gpt-4"));
            Assert.IsTrue(models.Contains("gpt-4-turbo"));
        }

        #endregion

        #region Feature Flag Tests

        [TestMethod]
        public async Task GetTierConfig_FreeTier_HasCorrectFeatureFlags()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.IsFalse(config.AllowCustomPrompts);
            Assert.IsFalse(config.AllowMultipleAgents);
            Assert.IsFalse(config.AllowWorkflowOrchestration);
            Assert.IsFalse(config.AllowAnalytics);
            Assert.IsFalse(config.AllowAPIAccess);
            Assert.IsFalse(config.AllowWebhooks);
            Assert.IsFalse(config.PrioritySupport);
        }

        [TestMethod]
        public async Task GetTierConfig_StarterTier_EnablesCustomPromptsAndAnalytics()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.IsTrue(config.AllowCustomPrompts);
            Assert.IsTrue(config.AllowAnalytics);
            Assert.IsTrue(config.AllowAPIAccess);
            Assert.IsFalse(config.AllowWebhooks);
            Assert.IsFalse(config.AllowMultipleAgents);
        }

        [TestMethod]
        public async Task GetTierConfig_ProfessionalTier_EnablesAllCoreFeatures()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.IsTrue(config.AllowCustomPrompts);
            Assert.IsTrue(config.AllowMultipleAgents);
            Assert.IsTrue(config.AllowWorkflowOrchestration);
            Assert.IsTrue(config.AllowAnalytics);
            Assert.IsTrue(config.AllowAPIAccess);
            Assert.IsTrue(config.AllowWebhooks);
            Assert.IsTrue(config.PrioritySupport);
        }

        [TestMethod]
        public async Task GetTierConfig_EnterpriseTier_EnablesAllFeatures()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.IsTrue(config.AllowCustomPrompts);
            Assert.IsTrue(config.AllowMultipleAgents);
            Assert.IsTrue(config.AllowWorkflowOrchestration);
            Assert.IsTrue(config.AllowAnalytics);
            Assert.IsTrue(config.AllowAPIAccess);
            Assert.IsTrue(config.AllowWebhooks);
            Assert.IsTrue(config.PrioritySupport);
            Assert.IsTrue(config.AllowDataExport);
            Assert.IsTrue(config.PublicAPI);
        }

        #endregion

        #region Team & Integration Limits Tests

        [TestMethod]
        public async Task GetTierConfig_FreeTier_LimitsSingleTeamMember()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.AreEqual(1, config.MaxTeamMembers);
            Assert.AreEqual(0, config.MaxCustomIntegrations);
        }

        [TestMethod]
        public async Task GetTierConfig_StarterTier_AllowsThreeTeamMembers()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.AreEqual(3, config.MaxTeamMembers);
            Assert.AreEqual(1, config.MaxCustomIntegrations);
        }

        [TestMethod]
        public async Task GetTierConfig_ProfessionalTier_AllowsTenTeamMembers()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.AreEqual(10, config.MaxTeamMembers);
            Assert.AreEqual(5, config.MaxCustomIntegrations);
        }

        [TestMethod]
        public async Task GetTierConfig_EnterpriseTier_AllowsUnlimitedTeamMembers()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.AreEqual(-1, config.MaxTeamMembers); // Unlimited
            Assert.AreEqual(-1, config.MaxCustomIntegrations); // Unlimited
        }

        #endregion

        #region Data Retention Tests

        [TestMethod]
        public async Task GetTierConfig_FreeTier_RetainsDataFor7Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);

            // Assert
            Assert.AreEqual(7, config.ConversationRetentionDays);
            Assert.AreEqual(7, config.LogRetentionDays);
        }

        [TestMethod]
        public async Task GetTierConfig_StarterTier_RetainsDataFor30Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);

            // Assert
            Assert.AreEqual(30, config.ConversationRetentionDays);
            Assert.AreEqual(30, config.LogRetentionDays);
        }

        [TestMethod]
        public async Task GetTierConfig_ProfessionalTier_RetainsDataFor90Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);

            // Assert
            Assert.AreEqual(90, config.ConversationRetentionDays);
            Assert.AreEqual(90, config.LogRetentionDays);
        }

        [TestMethod]
        public async Task GetTierConfig_EnterpriseTier_RetainsDataFor365Days()
        {
            // Act
            var config = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.AreEqual(365, config.ConversationRetentionDays);
            Assert.AreEqual(365, config.LogRetentionDays);
        }

        #endregion

        #region SLA Tests

        [TestMethod]
        public async Task GetTierConfig_SLAsByTier_AreCorrect()
        {
            // Act
            var freeTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Free);
            var starterTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Starter);
            var proTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Professional);
            var enterpriseTier = await _subscriptionManager.GetTierConfigAsync(SubscriptionTier.Enterprise);

            // Assert
            Assert.IsTrue(freeTier.SLA.Contains("Best effort"));
            Assert.IsTrue(starterTier.SLA.Contains("99%"));
            Assert.IsTrue(proTier.SLA.Contains("99.5%"));
            Assert.IsTrue(enterpriseTier.SLA.Contains("99.99%"));
        }

        #endregion

        #region Subscription Management Tests

        [TestMethod]
        public async Task UpgradeTier_WithValidTenant_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.UpgradeTierAsync("test-tenant", SubscriptionTier.Starter);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DowngradeTier_WithValidTenant_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.DowngradeTierAsync("test-tenant", SubscriptionTier.Free);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CancelSubscription_WithValidTenant_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.CancelSubscriptionAsync("test-tenant");

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Add-On Tests

        [TestMethod]
        public async Task AddSubscriptionAddOn_WithValidData_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.AddSubscriptionAddOnAsync("test-tenant", "priority-support");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RemoveSubscriptionAddOn_WithValidData_CompletesSuccessfully()
        {
            // Act
            var result = await _subscriptionManager.RemoveSubscriptionAddOnAsync("test-tenant", "priority-support");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetTenantAddOns_ReturnsAvailableAddOns()
        {
            // Act
            var addOns = await _subscriptionManager.GetTenantAddOnsAsync("test-tenant");

            // Assert
            Assert.IsNotNull(addOns);
            Assert.IsTrue(addOns.Count > 0);
            Assert.IsTrue(addOns.Exists(a => a.AddOnId == "priority-support"));
        }

        #endregion
    }
}
