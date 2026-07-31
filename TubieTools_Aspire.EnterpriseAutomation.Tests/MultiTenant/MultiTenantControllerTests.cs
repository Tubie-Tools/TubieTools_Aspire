using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using TubieTools_Aspire.EnterpriseAutomation.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TubieTools_Aspire.EnterpriseAutomation.Tests.MultiTenant
{
    public class MultiTenantControllerTests
    {
        private readonly Mock<IMultiTenantAIAgent> _mockMultiTenantAgent;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<ISubscriptionManager> _mockSubscriptionManager;
        private readonly Mock<ILogger<MultiTenantController>> _mockLogger;
        private readonly MultiTenantController _controller;

        public MultiTenantControllerTests()
        {
            _mockMultiTenantAgent = new Mock<IMultiTenantAIAgent>();
            _mockTenantService = new Mock<ITenantService>();
            _mockSubscriptionManager = new Mock<ISubscriptionManager>();
            _mockLogger = new Mock<ILogger<MultiTenantController>>();

            _controller = new MultiTenantController(
                _mockMultiTenantAgent.Object,
                _mockTenantService.Object,
                _mockSubscriptionManager.Object,
                _mockLogger.Object
            );
        }

        #region Tenant Registration Tests

        [Fact]
        public async Task RegisterTenant_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new TenantConfig
            {
                TenantId = "new-tenant-001",
                TenantName = "New Tenant Corp",
                Description = "A new customer"
            };

            var createdTenant = new TenantConfig
            {
                TenantId = "new-tenant-001",
                TenantName = "New Tenant Corp",
                Description = "A new customer",
                IsActive = true,
                ApiKey = "sk_1234567890",
                SecretKey = "secret_0987654321",
                CurrentTier = SubscriptionTier.Free
            };

            _mockTenantService.Setup(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()))
                .ReturnsAsync(createdTenant);

            // Act
            var result = await _controller.RegisterTenant(request);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.Equal(nameof(_controller.GetTenant), createdResult.ActionName);
        }

        [Fact]
        public async Task RegisterTenant_WithMissingTenantId_ReturnsBadRequest()
        {
            // Arrange
            var request = new TenantConfig
            {
                TenantName = "Invalid Tenant"
            };

            _mockTenantService.Setup(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()))
                .ThrowsAsync(new ArgumentException("TenantId is required"));

            // Act
            var result = await _controller.RegisterTenant(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Tenant Retrieval Tests

        [Fact]
        public async Task GetTenant_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "get-tenant-001",
                TenantName = "Test Tenant",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("get-tenant-001"))
                .ReturnsAsync(tenantConfig);

            // Act
            var result = await _controller.GetTenant("get-tenant-001");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(tenantConfig, okResult.Value);
        }

        [Fact]
        public async Task GetTenant_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.GetTenant("non-existent");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Tier Upgrade Tests

        [Fact]
        public async Task UpgradeTier_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var upgradeRequest = new { TenantId = "upgrade-tenant-001", NewTier = "Professional" };
            var updatedTenant = new TenantConfig
            {
                TenantId = "upgrade-tenant-001",
                CurrentTier = SubscriptionTier.Professional
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("upgrade-tenant-001"))
                .ReturnsAsync(new TenantConfig { TenantId = "upgrade-tenant-001" });

            _mockSubscriptionManager.Setup(sm => sm.UpgradeTierAsync("upgrade-tenant-001", SubscriptionTier.Professional))
                .ReturnsAsync(true);

            _mockTenantService.Setup(ts => ts.UpdateTenantAsync(It.IsAny<TenantConfig>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpgradeTier("upgrade-tenant-001", SubscriptionTier.Professional);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpgradeTier_WithNonexistentTenant_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.UpgradeTier("non-existent", SubscriptionTier.Professional);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region AI Agent Interaction Tests

        [Fact]
        public async Task AskAgent_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = "Search for high-priority incidents";
            var tenantId = "agent-test-001";

            var agentResponse = new AgentResponse
            {
                Success = true,
                Message = "Found 5 incidents",
                ToolsUsed = new List<string> { "search_incident" }
            };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, request))
                .ReturnsAsync(agentResponse);

            // Act
            var result = await _controller.AskAgent(tenantId, request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task AskAgent_WithQuotaExceeded_ReturnsForbidden()
        {
            // Arrange
            var request = "Create incident";
            var tenantId = "quota-tenant-001";

            var agentResponse = new AgentResponse
            {
                Success = false,
                Message = "Monthly quota exceeded"
            };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, request))
                .ReturnsAsync(agentResponse);

            // Act
            var result = await _controller.AskAgent(tenantId, request);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objResult = result as ObjectResult;
            Assert.Equal(429, objResult.StatusCode);
        }

        [Fact]
        public async Task AskAgent_WithAccessDenied_ReturnsForbidden()
        {
            // Arrange
            var request = "Create incident";
            var tenantId = "free-tenant-001";

            var agentResponse = new AgentResponse
            {
                Success = false,
                Message = "Access denied for this tier"
            };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, request))
                .ReturnsAsync(agentResponse);

            // Act
            var result = await _controller.AskAgent(tenantId, request);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objResult = result as ObjectResult;
            Assert.Equal(403, objResult.StatusCode);
        }

        #endregion

        #region Subscription Management Tests

        [Fact]
        public async Task GetSubscription_WithValidTenant_ReturnsOkResult()
        {
            // Arrange
            var subscription = new TenantSubscription
            {
                TenantId = "sub-test-001",
                SubscriptionId = "sub-123",
                Tier = SubscriptionTier.Professional,
                Status = "active"
            };

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("sub-test-001"))
                .ReturnsAsync(subscription);

            // Act
            var result = await _controller.GetSubscription("sub-test-001");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal(subscription, okResult.Value);
        }

        [Fact]
        public async Task GetSubscription_WithoutSubscription_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("no-sub-tenant"))
                .ReturnsAsync((TenantSubscription)null);

            // Act
            var result = await _controller.GetSubscription("no-sub-tenant");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Usage Statistics Tests

        [Fact]
        public async Task GetUsage_WithValidTenant_ReturnsOkResult()
        {
            // Arrange
            var tenantId = "usage-test-001";
            var startDate = System.DateTime.UtcNow.AddDays(-30);
            var endDate = System.DateTime.UtcNow;

            var usageStats = new List<TenantUsage>
            {
                new TenantUsage
                {
                    TenantId = tenantId,
                    ApiCallsCount = 150,
                    Timestamp = System.DateTime.UtcNow
                },
                new TenantUsage
                {
                    TenantId = tenantId,
                    ApiCallsCount = 200,
                    Timestamp = System.DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockTenantService.Setup(ts => ts.GetUsageStatsAsync(tenantId, startDate, endDate))
                .ReturnsAsync(usageStats);

            // Act
            var result = await _controller.GetUsage(tenantId, startDate, endDate);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var returnedList = okResult.Value as List<TenantUsage>;
            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public async Task GetUsage_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            var tenantId = "empty-usage-test";
            var startDate = System.DateTime.UtcNow.AddDays(-30);
            var endDate = System.DateTime.UtcNow;

            _mockTenantService.Setup(ts => ts.GetUsageStatsAsync(tenantId, startDate, endDate))
                .ReturnsAsync(new List<TenantUsage>());

            // Act
            var result = await _controller.GetUsage(tenantId, startDate, endDate);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var returnedList = okResult.Value as List<TenantUsage>;
            Assert.Empty(returnedList);
        }

        #endregion

        #region Available Tiers Tests

        [Fact]
        public async Task GetAvailableTiers_ReturnsAllFourTiers()
        {
            // Arrange
            var tierConfigs = new List<SubscriptionTierConfig>
            {
                new SubscriptionTierConfig { Tier = SubscriptionTier.Free, Name = "Free" },
                new SubscriptionTierConfig { Tier = SubscriptionTier.Starter, Name = "Starter" },
                new SubscriptionTierConfig { Tier = SubscriptionTier.Professional, Name = "Professional" },
                new SubscriptionTierConfig { Tier = SubscriptionTier.Enterprise, Name = "Enterprise" }
            };

            _mockSubscriptionManager.Setup(sm => sm.GetAllTierConfigsAsync())
                .ReturnsAsync(tierConfigs);

            // Act
            var result = await _controller.GetAvailableTiers();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var returnedList = okResult.Value as List<SubscriptionTierConfig>;
            Assert.Equal(4, returnedList.Count);
        }

        #endregion

        #region Custom Agent Management Tests

        [Fact]
        public async Task CreateCustomAgent_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var tenantId = "agent-tenant-001";
            var agentRequest = new TenantCustomAgent
            {
                TenantId = tenantId,
                AgentName = "Incident Handler",
                SystemPrompt = "You are an incident management expert"
            };

            var createdAgent = new TenantCustomAgent
            {
                AgentId = "agent-123",
                TenantId = tenantId,
                AgentName = "Incident Handler",
                SystemPrompt = "You are an incident management expert",
                CreatedDate = System.DateTime.UtcNow
            };

            _mockTenantService.Setup(ts => ts.CreateAgentAsync(It.IsAny<TenantCustomAgent>()))
                .ReturnsAsync(createdAgent);

            // Act
            var result = await _controller.CreateCustomAgent(tenantId, agentRequest);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetTenantAgents_WithValidTenant_ReturnsAgents()
        {
            // Arrange
            var tenantId = "agents-list-001";
            var agents = new List<TenantCustomAgent>
            {
                new TenantCustomAgent
                {
                    AgentId = "agent-1",
                    TenantId = tenantId,
                    AgentName = "Search Agent",
                    SystemPrompt = "Search for incidents"
                },
                new TenantCustomAgent
                {
                    AgentId = "agent-2",
                    TenantId = tenantId,
                    AgentName = "Create Agent",
                    SystemPrompt = "Create incidents"
                }
            };

            _mockTenantService.Setup(ts => ts.GetTenantAgentsAsync(tenantId))
                .ReturnsAsync(agents);

            // Act
            var result = await _controller.GetTenantAgents(tenantId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var returnedList = okResult.Value as List<TenantCustomAgent>;
            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public async Task GetTenantAgents_WithNoAgents_ReturnsEmptyList()
        {
            // Arrange
            var tenantId = "no-agents-tenant";

            _mockTenantService.Setup(ts => ts.GetTenantAgentsAsync(tenantId))
                .ReturnsAsync(new List<TenantCustomAgent>());

            // Act
            var result = await _controller.GetTenantAgents(tenantId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var returnedList = okResult.Value as List<TenantCustomAgent>;
            Assert.Empty(returnedList);
        }

        #endregion

        #region Team Member Management Tests

        [Fact]
        public async Task AddTeamMember_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var tenantId = "team-tenant-001";
            var memberRequest = new TenantTeamMember
            {
                TenantId = tenantId,
                Email = "john.doe@example.com",
                Role = "admin"
            };

            _mockTenantService.Setup(ts => ts.AddTeamMemberAsync(It.IsAny<TenantTeamMember>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddTeamMember(tenantId, memberRequest);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddTeamMember_WithinMaxLimit_ReturnsOkResult()
        {
            // Arrange
            var tenantId = "pro-team-tenant";
            var memberRequest = new TenantTeamMember
            {
                TenantId = tenantId,
                Email = "member@example.com",
                Role = "user"
            };

            var tierConfig = new SubscriptionTierConfig
            {
                MaxTeamMembers = 10
            };

            var existingMembers = new List<TenantTeamMember>
            {
                new TenantTeamMember { Email = "existing@example.com", Role = "admin" }
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(It.IsAny<SubscriptionTier>()))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.GetTeamMembersAsync(tenantId))
                .ReturnsAsync(existingMembers);

            _mockTenantService.Setup(ts => ts.AddTeamMemberAsync(It.IsAny<TenantTeamMember>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddTeamMember(tenantId, memberRequest);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddTeamMember_ExceedsMaxLimit_ReturnsBadRequest()
        {
            // Arrange
            var tenantId = "free-team-tenant";
            var memberRequest = new TenantTeamMember
            {
                TenantId = tenantId,
                Email = "another@example.com",
                Role = "user"
            };

            var tierConfig = new SubscriptionTierConfig
            {
                MaxTeamMembers = 1
            };

            var existingMembers = new List<TenantTeamMember>
            {
                new TenantTeamMember { Email = "owner@example.com", Role = "admin" }
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(It.IsAny<SubscriptionTier>()))
                .ReturnsAsync(tierConfig);

            _mockTenantService.Setup(ts => ts.GetTeamMembersAsync(tenantId))
                .ReturnsAsync(existingMembers);

            // Act
            var result = await _controller.AddTeamMember(tenantId, memberRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task RegisterTenant_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new TenantConfig { TenantId = "error-tenant" };

            _mockTenantService.Setup(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.RegisterTenant(request);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objResult = result as ObjectResult;
            Assert.Equal(500, objResult.StatusCode);
        }

        [Fact]
        public async Task AskAgent_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var tenantId = "error-agent-test";
            var request = "Test request";

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, request))
                .ThrowsAsync(new Exception("AI service unavailable"));

            // Act
            var result = await _controller.AskAgent(tenantId, request);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objResult = result as ObjectResult;
            Assert.Equal(500, objResult.StatusCode);
        }

        #endregion
    }
}
