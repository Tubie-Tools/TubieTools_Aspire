using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using TubieTools_Aspire.EnterpriseAutomation.Controllers;
using TubieTools_Aspire.EnterpriseAutomation.AIAgent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Mulitenant
{
    [TestClass]
    public class MultiTenantControllerTests
    {
        private Mock<IMultiTenantAIAgent> _mockMultiTenantAgent;
        private Mock<ITenantService> _mockTenantService;
        private Mock<ISubscriptionManager> _mockSubscriptionManager;
        private Mock<ILogger<MultiTenantController>> _mockLogger;
        private MultiTenantController _controller;

        [TestInitialize]
        public void Setup()
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

        [TestMethod]
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

            var registerRequest = new RegisterTenantRequest
            {
                TenantName = request.TenantName,
                Description = request.Description
            };

            _mockTenantService.Setup(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()))
                .ReturnsAsync(createdTenant);

            // Act
            var result = await _controller.RegisterTenant(registerRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task RegisterTenant_WithMissingTenantId_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new RegisterTenantRequest
            {
                TenantName = "Invalid Tenant",
                Description = null
            };

            _mockTenantService.Setup(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()))
                .ThrowsAsync(new ArgumentException("TenantId is required"));

            // Act
            var result = await _controller.RegisterTenant(registerRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        #endregion

        #region Tenant Retrieval Tests

        [TestMethod]
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

            var subscription = new TenantSubscription
            {
                TenantId = "get-tenant-001",
                Tier = SubscriptionTier.Starter
            };

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("get-tenant-001"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("get-tenant-001"))
                .ReturnsAsync(subscription);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            // Act
            var result = await _controller.GetTenant("get-tenant-001");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetTenant_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.GetTenant("non-existent");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region Tier Upgrade Tests

        [TestMethod]
        public async Task UpgradeTier_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var upgradeRequest = new UpgradeTierRequest { NewTier = SubscriptionTier.Professional };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("upgrade-tenant-001"))
                .ReturnsAsync(new TenantConfig { TenantId = "upgrade-tenant-001" });

            _mockTenantService.Setup(ts => ts.UpdateTenantAsync(It.IsAny<TenantConfig>()))
                .ReturnsAsync(true);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("upgrade-tenant-001"))
                .ReturnsAsync(new TenantSubscription { TenantId = "upgrade-tenant-001" });

            _mockTenantService.Setup(ts => ts.UpdateSubscriptionAsync(It.IsAny<TenantSubscription>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpgradeTier("upgrade-tenant-001", upgradeRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task UpgradeTier_WithNonexistentTenant_ReturnsNotFound()
        {
            // Arrange
            var upgradeRequest = new UpgradeTierRequest { NewTier = SubscriptionTier.Professional };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.UpgradeTier("non-existent", upgradeRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region AI Agent Interaction Tests

        [TestMethod]
        public async Task AskAgent_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var agentRequest = new AskAgentRequest { Message = "Test request" };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync("test-tenant", agentRequest.Message))
                .ReturnsAsync(new AgentResponse { Success = true, Message = "Success" });

            // Act
            var result = await _controller.AskAgent("test-tenant", agentRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task AskAgent_WithQuotaExceeded_ReturnsForbidden()
        {
            // Arrange
            var agentRequest = new AskAgentRequest { Message = "Test request" };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync("quota-tenant", agentRequest.Message))
                .ReturnsAsync(new AgentResponse { Success = false, Message = "Quota exceeded" });

            // Act
            var result = await _controller.AskAgent("quota-tenant", agentRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task AskAgent_WithAccessDenied_ReturnsForbidden()
        {
            // Arrange
            var agentRequest = new AskAgentRequest { Message = "Test request" };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync("free-tenant", agentRequest.Message))
                .ReturnsAsync(new AgentResponse { Success = false, Message = "Access denied" });

            // Act
            var result = await _controller.AskAgent("free-tenant", agentRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        #endregion

        #region Subscription Management Tests

        [TestMethod]
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
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetSubscription_WithoutSubscription_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("no-sub-tenant"))
                .ReturnsAsync((TenantSubscription)null);

            // Act
            var result = await _controller.GetSubscription("no-sub-tenant");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        #endregion

        #region Custom Agent Management Tests

        [TestMethod]
        public async Task CreateCustomAgent_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var tenantId = "agent-tenant-001";
            var agentRequest = new CreateAgentRequest
            {
                AgentName = "Incident Handler",
                SystemPrompt = "You are an incident management expert"
            };

            var createdAgent = new TenantCustomAgent
            {
                AgentId = "agent-123",
                TenantId = tenantId,
                AgentName = "Incident Handler",
                IsActive = true
            };

            _mockTenantService.Setup(ts => ts.CreateAgentAsync(It.IsAny<TenantCustomAgent>()))
                .ReturnsAsync(createdAgent);

            // Act
            var result = await _controller.CreateCustomAgent(tenantId, agentRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        #endregion

        #region Team Member Management Tests

        [TestMethod]
        public async Task AddTeamMember_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var tenantId = "team-tenant-001";
            var memberRequest = new AddTeamMemberRequest
            {
                Email = "john.doe@example.com",
                Role = "admin"
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync(tenantId))
                .ReturnsAsync(new TenantConfig { TenantId = tenantId, CurrentTier = SubscriptionTier.Professional });

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(It.IsAny<SubscriptionTier>()))
                .ReturnsAsync(new SubscriptionTierConfig { MaxTeamMembers = 10 });

            _mockTenantService.Setup(ts => ts.GetTeamMembersAsync(tenantId))
                .ReturnsAsync(new List<TenantTeamMember>());

            _mockTenantService.Setup(ts => ts.AddTeamMemberAsync(It.IsAny<TenantTeamMember>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddTeamMember(tenantId, memberRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        #endregion
    }
}