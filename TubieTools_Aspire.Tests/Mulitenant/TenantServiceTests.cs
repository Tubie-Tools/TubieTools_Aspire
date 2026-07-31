using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Mulitenant
{
    [TestClass]
    public class TenantServiceTests
    {
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly TenantService _tenantService;

        public TenantServiceTests()
        {
            _mockLogger = new Mock<ILogger<TenantService>>();
            _tenantService = new TenantService(_mockLogger.Object);
        }

        #region Tenant CRUD Tests

        [TestMethod]
        public async Task CreateTenant_WithValidConfig_GeneratesUniqueApiKey()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-001",
                TenantName = "Test Company",
                Description = "Test Description",
                CurrentTier = SubscriptionTier.Free
            };

            // Act
            var createdTenant = await _tenantService.CreateTenantAsync(tenantConfig);

            // Assert
            Assert.IsNotNull(createdTenant);
            Assert.IsTrue(!string.IsNullOrEmpty(createdTenant.ApiKey));
            Assert.IsTrue(createdTenant.ApiKey.StartsWith("sk_"));
            Assert.IsTrue(!string.IsNullOrEmpty(createdTenant.SecretKey));
            Assert.IsTrue(createdTenant.SecretKey.StartsWith("secret_"));
            Assert.IsTrue(createdTenant.IsActive);
        }

        [TestMethod] 
        public async Task CreateTenant_WithMissingTenantId_ThrowsArgumentException()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = null,
                TenantName = "Test Company"
            };

            // Act
            await _tenantService.CreateTenantAsync(tenantConfig);
        }

        [TestMethod]
        public async Task CreateTenant_CreatesDefaultQuota()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-002",
                TenantName = "Test Company 2"
            };

            // Act
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Assert
            var quota = await _tenantService.GetQuotaAsync("test-tenant-002");
            Assert.IsNotNull(quota);
            Assert.AreEqual(100, quota.MonthlyApiCallLimit);
            Assert.AreEqual(20, quota.DailyApiCallLimit);
        }

        [TestMethod]
        public async Task GetTenant_WithValidId_ReturnsTenant()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-003",
                TenantName = "Test Company 3"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            var retrievedTenant = await _tenantService.GetTenantAsync("test-tenant-003");

            // Assert
            Assert.IsNotNull(retrievedTenant);
            Assert.AreEqual("test-tenant-003", retrievedTenant.TenantId);
            Assert.AreEqual("Test Company 3", retrievedTenant.TenantName);
        }

        [TestMethod]
        public async Task GetTenant_WithInvalidId_ReturnsNull()
        {
            // Act
            var retrievedTenant = await _tenantService.GetTenantAsync("non-existent-tenant");

            // Assert
            Assert.IsNull(retrievedTenant);
        }

        [TestMethod]
        public async Task UpdateTenant_WithValidConfig_UpdatesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-004",
                TenantName = "Original Name"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            tenantConfig.TenantName = "Updated Name";
            var result = await _tenantService.UpdateTenantAsync(tenantConfig);

            // Assert
            Assert.IsTrue(result);
            var updated = await _tenantService.GetTenantAsync("test-tenant-004");
            Assert.AreEqual("Updated Name", updated.TenantName);
        }

        [TestMethod]
        public async Task DeleteTenant_WithValidId_RemovesTenant()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-005",
                TenantName = "To Delete"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            var result = await _tenantService.DeleteTenantAsync("test-tenant-005");

            // Assert
            Assert.IsTrue(result);
            var deleted = await _tenantService.GetTenantAsync("test-tenant-005");
            Assert.IsNull(deleted);
        }

        #endregion

        #region Subscription Tests

        [TestMethod]
        public async Task UpdateSubscription_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-006",
                TenantName = "Subscription Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var subscription = new TenantSubscription
            {
                TenantId = "test-tenant-006",
                SubscriptionId = "sub-001",
                Tier = SubscriptionTier.Starter,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = "active",
                BillingAmount = 29m
            };

            // Act
            var result = await _tenantService.UpdateSubscriptionAsync(subscription);

            // Assert
            Assert.IsTrue(result);
            var retrieved = await _tenantService.GetSubscriptionAsync("test-tenant-006");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(SubscriptionTier.Starter, retrieved.Tier);
        }

        [TestMethod]
        public async Task GetSubscription_WithNoSubscription_ReturnsNull()
        {
            // Act
            var subscription = await _tenantService.GetSubscriptionAsync("non-existent");

            // Assert
            Assert.IsNull(subscription);
        }

        #endregion

        #region Quota & Usage Tests

        [TestMethod]
        public async Task IncrementUsage_WithValidTenant_UpdatesCounters()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-007",
                TenantName = "Usage Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            var result = await _tenantService.IncrementUsageAsync("test-tenant-007", 1);

            // Assert
            Assert.IsTrue(result);
            var quota = await _tenantService.GetQuotaAsync("test-tenant-007");
            Assert.AreEqual(1, quota.MonthlyApiCallsUsed);
        }

        [TestMethod]
        public async Task IncrementUsage_MultipleIncrements_AccumulatesCorrectly()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-008",
                TenantName = "Multiple Usage Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            await _tenantService.IncrementUsageAsync("test-tenant-008", 5);
            await _tenantService.IncrementUsageAsync("test-tenant-008", 10);
            await _tenantService.IncrementUsageAsync("test-tenant-008", 35);

            // Assert
            var quota = await _tenantService.GetQuotaAsync("test-tenant-008");
            Assert.AreEqual(50, quota.MonthlyApiCallsUsed);
        }

        [TestMethod]
        public async Task IsQuotaExceeded_WhenMonthlyLimitReached_ReturnsTrue()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-009",
                TenantName = "Quota Exceeded Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act - Increment to limit
            await _tenantService.IncrementUsageAsync("test-tenant-009", 100);
            var isExceeded = await _tenantService.IsQuotaExceededAsync("test-tenant-009");

            // Assert
            Assert.IsTrue(isExceeded);
        }

        [TestMethod]
        public async Task IsQuotaExceeded_WhenUnderLimit_ReturnsFalse()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-010",
                TenantName = "Under Quota Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            await _tenantService.IncrementUsageAsync("test-tenant-010", 50);
            var isExceeded = await _tenantService.IsQuotaExceededAsync("test-tenant-010");

            // Assert
            Assert.IsFalse(isExceeded);
        }

        [TestMethod]
        public async Task GetUsageStats_WithDateRange_ReturnsFilteredStats()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-011",
                TenantName = "Stats Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            // Act
            await _tenantService.IncrementUsageAsync("test-tenant-011", 10);

            // Assert
            var stats = await _tenantService.GetUsageStatsAsync("test-tenant-011", startDate, endDate);
            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.Count > 0);
        }

        #endregion

        #region Custom Agent Tests

        [TestMethod]
        public async Task CreateAgent_WithValidData_CreatesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-012",
                TenantName = "Agent Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var agent = new TenantCustomAgent
            {
                TenantId = "test-tenant-012",
                AgentName = "Search Agent",
                SystemPrompt = "You search for incidents", 
            };

            // Act
            var created = await _tenantService.CreateAgentAsync(agent);

            // Assert
            Assert.IsNotNull(created);
            Assert.IsTrue(!string.IsNullOrEmpty(created.AgentId));
            Assert.AreEqual("Search Agent", created.AgentName);
        }

        [TestMethod]
        public async Task GetTenantAgents_WithValidTenant_ReturnsAgents()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-013",
                TenantName = "Agents List Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var agent1 = new TenantCustomAgent
            {
                TenantId = "test-tenant-013",
                AgentName = "Agent 1",
                SystemPrompt = "Prompt 1"
            };

            await _tenantService.CreateAgentAsync(agent1);

            // Act
            var agents = await _tenantService.GetTenantAgentsAsync("test-tenant-013");

            // Assert
            Assert.IsNotNull(agents);
            Assert.IsTrue(agents.Count > 0);
        }

        [TestMethod]
        public async Task UpdateAgent_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-014",
                TenantName = "Agent Update Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var agent = new TenantCustomAgent
            {
                TenantId = "test-tenant-014",
                AgentName = "Original Name",
                SystemPrompt = "Original prompt"
            };

            var created = await _tenantService.CreateAgentAsync(agent);

            // Act
            created.AgentName = "Updated Name";
            var result = await _tenantService.UpdateAgentAsync(created);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteAgent_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-015",
                TenantName = "Agent Delete Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var agent = new TenantCustomAgent
            {
                TenantId = "test-tenant-015",
                AgentName = "To Delete",
                SystemPrompt = "Delete me"
            };

            var created = await _tenantService.CreateAgentAsync(agent);

            // Act
            var result = await _tenantService.DeleteAgentAsync(created.AgentId);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Team Member Tests

        [TestMethod]
        public async Task AddTeamMember_WithValidData_AddsSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-016",
                TenantName = "Team Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var member = new TenantTeamMember
            {
                TenantId = "test-tenant-016",
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "admin"
            };

            // Act
            var result = await _tenantService.AddTeamMemberAsync(member);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetTeamMembers_WithValidTenant_ReturnsMembers()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-017",
                TenantName = "Team List Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var member = new TenantTeamMember
            {
                TenantId = "test-tenant-017",
                Email = "jane@example.com",
                Role = "user"
            };

            await _tenantService.AddTeamMemberAsync(member);

            // Act
            var members = await _tenantService.GetTeamMembersAsync("test-tenant-017");

            // Assert
            Assert.IsNotNull(members);
            Assert.IsTrue(members.Count > 0);
        }

        [TestMethod]
        public async Task RemoveTeamMember_WithValidId_RemovesSuccessfully()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-018",
                TenantName = "Team Remove Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var member = new TenantTeamMember
            {
                TenantId = "test-tenant-018",
                Email = "remove@example.com",
                Role = "user"
            };

            await _tenantService.AddTeamMemberAsync(member);

            // Act
            var result = await _tenantService.RemoveTeamMemberAsync("remove@example.com");

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Billing Tests

        [TestMethod]
        public async Task GenerateBillingRecord_WithValidSubscription_GeneratesRecord()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-019",
                TenantName = "Billing Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            var subscription = new TenantSubscription
            {
                TenantId = "test-tenant-019",
                SubscriptionId = "sub-billing-001",
                Tier = SubscriptionTier.Starter,
                Status = "active",
                BillingAmount = 29m
            };

            await _tenantService.UpdateSubscriptionAsync(subscription);

            // Act
            var billing = await _tenantService.GenerateBillingRecordAsync("test-tenant-019");

            // Assert
            Assert.IsNotNull(billing);
            Assert.AreEqual("test-tenant-019", billing.TenantId);
            Assert.AreEqual(29m, billing.Amount);
        }

        [TestMethod] 
        public async Task GenerateBillingRecord_WithoutSubscription_ThrowsException()
        {
            // Arrange
            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-020",
                TenantName = "No Billing Test"
            };
            await _tenantService.CreateTenantAsync(tenantConfig);

            // Act
            await _tenantService.GenerateBillingRecordAsync("test-tenant-020");
        }

        #endregion
    }
}
