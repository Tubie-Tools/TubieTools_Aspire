using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using TubieTools_Aspire.EnterpriseAutomation.AIAgent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Mulitenant
{
    [TestClass]
    public class MultiTenantAIAgentTests
    {
        private Mock<IAIAgent> _mockBaseAgent;
        private Mock<ITenantService> _mockTenantService;
        private Mock<ISubscriptionManager> _mockSubscriptionManager;
        private Mock<ITenantContextAccessor> _mockContextAccessor;
        private Mock<ILogger<MultiTenantAIAgent>> _mockLogger;
        private MultiTenantAIAgent _multiTenantAgent;

        [TestInitialize]
        public void Setup()
        {
            _mockBaseAgent = new Mock<IAIAgent>();
            _mockTenantService = new Mock<ITenantService>();
            _mockSubscriptionManager = new Mock<ISubscriptionManager>();
            _mockContextAccessor = new Mock<ITenantContextAccessor>();
            _mockLogger = new Mock<ILogger<MultiTenantAIAgent>>();

            _multiTenantAgent = new MultiTenantAIAgent(
                _mockBaseAgent.Object,
                _mockTenantService.Object,
                _mockSubscriptionManager.Object,
                _mockContextAccessor.Object,
                _mockLogger.Object
            );
        }

        #region Access Validation Tests

        [TestMethod]
        public async Task ProcessRequestAsync_WithTenantHavingApiAccess_ProcessesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-001",
                TenantName = "Test Tenant",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowAPIAccess = true,
                AvailableTools = new List<string> { "search_incident", "create_incident", "close_incident" },
                AvailableModels = new List<string> { "gpt-3.5-turbo", "gpt-4" }
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-001"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-001"))
                .ReturnsAsync(false);

            var agentResponse = new AgentResponse { Success = true, Message = "Success" };
            _mockBaseAgent.Setup(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()))
                .ReturnsAsync(agentResponse);

            _mockTenantService.Setup(ts => ts.IncrementUsageAsync("test-tenant-001", 1))
                .ReturnsAsync(true);

            _mockSubscriptionManager.Setup(sm => sm.GetToolAccessAsync(It.IsAny<string>(), It.IsAny<SubscriptionTier>()))
                .ReturnsAsync(new ToolFeatureAccess { IsAvailable = true });

            // Act
            var result = await _multiTenantAgent.ProcessRequestAsync("test-tenant-001", "Test request");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            _mockBaseAgent.Verify(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()), Times.Once);
        }

        [TestMethod]
        public async Task ProcessRequestAsync_WithoutApiAccess_DeniesRequest()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-002",
                TenantName = "Free Tenant",
                CurrentTier = SubscriptionTier.Free,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Free,
                AllowAPIAccess = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-002"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Free))
                .ReturnsAsync(tierConfig);

            // Act
            var result = await _multiTenantAgent.ProcessRequestAsync("test-tenant-002", "Test request");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("Access denied"));
            _mockBaseAgent.Verify(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()), Times.Never);
        }

        [TestMethod]
        public async Task ProcessRequestAsync_WithQuotaExceeded_DeniesRequest()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-003",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig { AllowAPIAccess = true };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-003"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-003"))
                .ReturnsAsync(true);

            // Act
            var result = await _multiTenantAgent.ProcessRequestAsync("test-tenant-003", "Test request");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.ToLower().Contains("quota exceeded"));
        }

        #endregion

        #region Tool Filtering Tests

        [TestMethod]
        public async Task ProcessRequestAsync_WithFreeTier_FiltersToolsCorrectly()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-004",
                CurrentTier = SubscriptionTier.Free,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Free,
                AllowAPIAccess = true,
                AvailableTools = new List<string>()
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-004"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Free))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-004"))
                .ReturnsAsync(false);

            var agentResponse = new AgentResponse { Success = true, Message = "Success" };
            _mockBaseAgent.Setup(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()))
                .ReturnsAsync(agentResponse);

            _mockTenantService.Setup(ts => ts.IncrementUsageAsync("test-tenant-004", 1))
                .ReturnsAsync(true);

            // Act
            var result = await _multiTenantAgent.ProcessRequestAsync("test-tenant-004", "Test request");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            _mockBaseAgent.Verify(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()), Times.Once);
        }

        #endregion

        #region Conversation Management Tests

        [TestMethod]
        public async Task GetTenantConversationHistoryAsync_WithValidAccess_ReturnsHistory()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-010",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig { AllowAPIAccess = true };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-010"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            var conversationHistory = new List<AgentMessage>
            {
                new AgentMessage { Role = "user", Content = "Hello" },
                new AgentMessage { Role = "assistant", Content = "Hi there!" }
            };

            _mockBaseAgent.Setup(ba => ba.GetConversationHistory())
                .Returns(conversationHistory);

            // Act
            var result = await _multiTenantAgent.GetTenantConversationHistoryAsync("test-tenant-010");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.ConversationHistory.Count);
        }

        [TestMethod]
        public async Task GetTenantConversationHistoryAsync_WithoutApiAccess_DeniesAccess()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-011",
                CurrentTier = SubscriptionTier.Free,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig { AllowAPIAccess = false };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-011"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Free))
                .ReturnsAsync(tierConfig);

            // Act
            var result = await _multiTenantAgent.GetTenantConversationHistoryAsync("test-tenant-011");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("Access denied"));
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task ProcessRequestAsync_WithException_ReturnsErrorResponse()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-012",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                AllowAPIAccess = true,
                AvailableTools = new List<string> { "search_incident" },
                AvailableModels = new List<string> { "gpt-4" }
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-012"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-012"))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _multiTenantAgent.ProcessRequestAsync("test-tenant-012", "Test request");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("Error"));
        }

        #endregion
    }
}