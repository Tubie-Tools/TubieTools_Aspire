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
            var registerRequest = new RegisterTenantRequest
            {
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
                CurrentTier = SubscriptionTier.Free
            };

            _mockTenantService.Setup(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()))
                .ReturnsAsync(createdTenant);

            // Act
            var result = await _controller.RegisterTenant(registerRequest) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
            _mockTenantService.Verify(ts => ts.CreateTenantAsync(It.IsAny<TenantConfig>()), Times.Once);
        }

        [TestMethod]
        public async Task RegisterTenant_WithMissingTenantName_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new RegisterTenantRequest
            {
                TenantName = null,
                Description = null
            };

            // Act
            var result = await _controller.RegisterTenant(registerRequest) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
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
            var result = await _controller.GetTenant("get-tenant-001") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task GetTenant_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.GetTenant("non-existent") as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        #endregion

        #region Tier Upgrade Tests

        [TestMethod]
        public async Task UpgradeTier_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var tenantId = "upgrade-tenant-001";
            var upgradeRequest = new UpgradeTierRequest { NewTier = SubscriptionTier.Professional };

            var existingTenant = new TenantConfig
            {
                TenantId = tenantId,
                CurrentTier = SubscriptionTier.Starter
            };

            var existingSubscription = new TenantSubscription
            {
                TenantId = tenantId,
                Tier = SubscriptionTier.Starter
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync(tenantId))
                .ReturnsAsync(existingTenant);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync(tenantId))
                .ReturnsAsync(existingSubscription);

            _mockTenantService.Setup(ts => ts.UpdateTenantAsync(It.IsAny<TenantConfig>()))
                .ReturnsAsync(true);

            _mockTenantService.Setup(ts => ts.UpdateSubscriptionAsync(It.IsAny<TenantSubscription>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpgradeTier(tenantId, upgradeRequest) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _mockTenantService.Verify(ts => ts.UpdateTenantAsync(It.IsAny<TenantConfig>()), Times.Once);
            _mockTenantService.Verify(ts => ts.UpdateSubscriptionAsync(It.IsAny<TenantSubscription>()), Times.Once);
        }

        [TestMethod]
        public async Task UpgradeTier_WithNonexistentTenant_ReturnsNotFound()
        {
            // Arrange
            var upgradeRequest = new UpgradeTierRequest { NewTier = SubscriptionTier.Professional };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("non-existent"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.UpgradeTier("non-existent", upgradeRequest) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        #endregion

        #region AI Agent Interaction Tests

        [TestMethod]
        public async Task AskAgent_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var tenantId = "test-tenant";
            var agentRequest = new AskAgentRequest { Message = "Test request" };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, agentRequest.Message))
                .ReturnsAsync(new AgentResponse { Success = true, Message = "Success", Data = new { result = "test result" } });

            // Act
            var result = await _controller.AskAgent(tenantId, agentRequest) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _mockMultiTenantAgent.Verify(mta => mta.ProcessRequestAsync(tenantId, agentRequest.Message), Times.Once);
        }

        [TestMethod]
        public async Task AskAgent_WithQuotaExceeded_ReturnsForbiddenResult()
        {
            // Arrange
            var tenantId = "quota-tenant";
            var agentRequest = new AskAgentRequest { Message = "Test request" };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, agentRequest.Message))
                .ReturnsAsync(new AgentResponse { Success = false, Message = "Quota exceeded" });

            // Act
            var result = await _controller.AskAgent(tenantId, agentRequest) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode); // Forbidden
        }

        [TestMethod]
        public async Task AskAgent_WithAccessDenied_ReturnsForbiddenResult()
        {
            // Arrange
            var tenantId = "free-tenant";
            var agentRequest = new AskAgentRequest { Message = "Test request" };

            _mockMultiTenantAgent.Setup(mta => mta.ProcessRequestAsync(tenantId, agentRequest.Message))
                .ReturnsAsync(new AgentResponse { Success = false, Message = "Access denied" });

            // Act
            var result = await _controller.AskAgent(tenantId, agentRequest) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(403, result.StatusCode); // Forbidden
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
            var result = await _controller.GetSubscription("sub-test-001") as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Value);
        }

        [TestMethod]
        public async Task GetSubscription_WithoutSubscription_ReturnsNotFound()
        {
            // Arrange
            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("no-sub-tenant"))
                .ReturnsAsync((TenantSubscription)null);

            // Act
            var result = await _controller.GetSubscription("no-sub-tenant") as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
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

            _mockTenantService.Setup(ts => ts.GetTenantAsync(tenantId))
                .ReturnsAsync(new TenantConfig { TenantId = tenantId });

            _mockTenantService.Setup(ts => ts.CreateAgentAsync(It.IsAny<TenantCustomAgent>()))
                .ReturnsAsync(createdAgent);

            // Act
            var result = await _controller.CreateCustomAgent(tenantId, agentRequest) as CreatedAtActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);
            Assert.AreEqual("GetCustomAgent", result.ActionName);
            _mockTenantService.Verify(ts => ts.CreateAgentAsync(It.IsAny<TenantCustomAgent>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateCustomAgent_WithInvalidTenant_ReturnsNotFound()
        {
            // Arrange
            var tenantId = "invalid-tenant";
            var agentRequest = new CreateAgentRequest
            {
                AgentName = "Incident Handler",
                SystemPrompt = "You are an incident management expert"
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync(tenantId))
                .ReturnsAsync((TenantConfig)null);

            // Act
            var result = await _controller.CreateCustomAgent(tenantId, agentRequest) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
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

            var tenantConfig = new TenantConfig
            {
                TenantId = tenantId,
                CurrentTier = SubscriptionTier.Professional
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync(tenantId))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Professional))
                .ReturnsAsync(new SubscriptionTierConfig { MaxTeamMembers = 10 });

            _mockTenantService.Setup(ts => ts.GetTeamMembersAsync(tenantId))
                .ReturnsAsync(new List<TenantTeamMember>());

            _mockTenantService.Setup(ts => ts.AddTeamMemberAsync(It.IsAny<TenantTeamMember>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddTeamMember(tenantId, memberRequest) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _mockTenantService.Verify(ts => ts.AddTeamMemberAsync(It.IsAny<TenantTeamMember>()), Times.Once);
        }

        [TestMethod]
        public async Task AddTeamMember_WithMaxMembersExceeded_ReturnsBadRequest()
        {
            // Arrange
            var tenantId = "full-team-tenant";
            var memberRequest = new AddTeamMemberRequest
            {
                Email = "new.member@example.com",
                Role = "member"
            };

            var tenantConfig = new TenantConfig
            {
                TenantId = tenantId,
                CurrentTier = SubscriptionTier.Free
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync(tenantId))
                .ReturnsAsync(tenantConfig);

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Free))
                .ReturnsAsync(new SubscriptionTierConfig { MaxTeamMembers = 1 });

            var existingMembers = new List<TenantTeamMember>
            {
                new TenantTeamMember { Email = "existing@example.com", TenantId = tenantId }
            };

            _mockTenantService.Setup(ts => ts.GetTeamMembersAsync(tenantId))
                .ReturnsAsync(existingMembers);

            // Act
            var result = await _controller.AddTeamMember(tenantId, memberRequest) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            _mockTenantService.Verify(ts => ts.AddTeamMemberAsync(It.IsAny<TenantTeamMember>()), Times.Never);
        }

        #endregion
    }
}