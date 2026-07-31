using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using TubieTools_Aspire.EnterpriseAutomation.AIAgent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TubieTools_Aspire.EnterpriseAutomation.Tests.MultiTenant
{
    public class MultiTenantAIAgentTests
    {
        private readonly Mock<IAIAgent> _mockBaseAgent;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<ISubscriptionManager> _mockSubscriptionManager;
        private readonly Mock<ITenantContextAccessor> _mockContextAccessor;
        private readonly Mock<ILogger<MultiTenantAIAgent>> _mockLogger;
        private readonly MultiTenantAIAgent _multiTenantAgent;

        public MultiTenantAIAgentTests()
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

        [Fact]
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

            var quota = new TenantQuota { TenantId = "test-tenant-001", QuotaExceeded = false };
            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-001"))
                .ReturnsAsync(false);

            var mockTools = new List<AIChatTool>
            {
                new AIChatTool { Name = "search_incident", Description = "Search" }
            };

            var agentResponse = new AgentResponse { Success = true, Message = "Success" };
            _mockBaseAgent.Setup(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()))
                .ReturnsAsync(agentResponse);

            _mockTenantService.Setup(ts => ts.IncrementUsageAsync("test-tenant-001", 1))
                .ReturnsAsync(true);

            // Act
            var result = await _multiTenantAgent.ProcessRequestAsync("test-tenant-001", "Test request");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockBaseAgent.Verify(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()), Times.Once);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Access denied", result.Message);
            _mockBaseAgent.Verify(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()), Times.Never);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("quota exceeded", result.Message.ToLower());
        }

        #endregion

        #region Tool Filtering Tests

        [Fact]
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
                AvailableTools = new List<string> { "search_incident" },
                AvailableModels = new List<string> { "gpt-3.5-turbo" }
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
            await _multiTenantAgent.ProcessRequestAsync("test-tenant-004", "Test request");

            // Assert
            _mockBaseAgent.Verify(
                ba => ba.ProcessRequestAsync(
                    It.IsAny<string>(),
                    It.Is<List<AIChatTool>>(tools =>
                        tools.Count == 1 && tools[0].Name == "search_incident"
                    )
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task ProcessRequestAsync_WithStarterTier_AllowsAllTools()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-005",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowAPIAccess = true,
                AvailableTools = new List<string> { "create_incident", "search_incident", "close_incident" },
                AvailableModels = new List<string> { "gpt-3.5-turbo", "gpt-4" }
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-005"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-005"))
                .ReturnsAsync(false);

            _mockSubscriptionManager.Setup(sm => sm.GetToolAccessAsync(It.IsAny<string>(), SubscriptionTier.Starter))
                .ReturnsAsync(new ToolFeatureAccess { IsAvailable = true });

            var agentResponse = new AgentResponse { Success = true };
            _mockBaseAgent.Setup(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()))
                .ReturnsAsync(agentResponse);

            _mockTenantService.Setup(ts => ts.IncrementUsageAsync("test-tenant-005", 1))
                .ReturnsAsync(true);

            // Act
            await _multiTenantAgent.ProcessRequestAsync("test-tenant-005", "Test request");

            // Assert
            _mockBaseAgent.Verify(
                ba => ba.ProcessRequestAsync(
                    It.IsAny<string>(),
                    It.Is<List<AIChatTool>>(tools => tools.Count == 3)
                ),
                Times.Once
            );
        }

        #endregion

        #region Feature Access Tests

        [Fact]
        public async Task ValidateAccessAsync_WithAllowedFeature_ReturnsTrue()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-006",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowCustomPrompts = true,
                AllowAnalytics = true
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-006"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            // Act
            var result = await _multiTenantAgent.ValidateAccessAsync("test-tenant-006", "custom_prompts");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateAccessAsync_WithDisallowedFeature_ReturnsFalse()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-007",
                CurrentTier = SubscriptionTier.Free,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Free,
                AllowCustomPrompts = false,
                AllowMultipleAgents = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-007"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Free))
                .ReturnsAsync(tierConfig);

            // Act
            var result = await _multiTenantAgent.ValidateAccessAsync("test-tenant-007", "multiple_agents");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAccessAsync_WithInactiveTenant_ReturnsFalse()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-008",
                IsActive = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-008"))
                .ReturnsAsync(tenantConfig);

            // Act
            var result = await _multiTenantAgent.ValidateAccessAsync("test-tenant-008", "api_access");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAccessAsync_WithNonexistentTenant_ReturnsFalse()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _multiTenantAgent.ValidateAccessAsync("non-existent", "api_access");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Usage Tracking Tests

        [Fact]
        public async Task ProcessRequestAsync_TracksUsageAfterSuccessfulRequest()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-009",
                CurrentTier = SubscriptionTier.Starter,
                IsActive = true
            };

            var tierConfig = new SubscriptionTierConfig
            {
                AllowAPIAccess = true,
                AvailableTools = new List<string> { "search_incident" },
                AvailableModels = new List<string> { "gpt-4" }
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-009"))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.IsQuotaExceededAsync("test-tenant-009"))
                .ReturnsAsync(false);

            var agentResponse = new AgentResponse { Success = true };
            _mockBaseAgent.Setup(ba => ba.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<List<AIChatTool>>()))
                .ReturnsAsync(agentResponse);

            var incrementUsageSetup = _mockTenantService.Setup(ts => ts.IncrementUsageAsync("test-tenant-009", 1));
            incrementUsageSetup.ReturnsAsync(true);

            // Act
            await _multiTenantAgent.ProcessRequestAsync("test-tenant-009", "Test request");

            // Assert
            _mockTenantService.Verify(
                ts => ts.IncrementUsageAsync("test-tenant-009", 1),
                Times.Once
            );
        }

        #endregion

        #region Conversation Management Tests

        [Fact]
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
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.ConversationHistory.Count);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Access denied", result.Message);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
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
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Error", result.Message);
        }

        #endregion
    }
}
