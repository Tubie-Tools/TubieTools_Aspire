using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using TubieTools_Aspire.EnterpriseAutomation.MultiTenant;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Mulitenant
{
    [TestClass]
    public class TenantResolverMiddlewareTests
    {
        private Mock<ILogger<TenantResolverMiddleware>> _mockLogger;
        private Mock<ITenantContextAccessor> _mockContextAccessor;
        private Mock<ITenantService> _mockTenantService;
        private Mock<ISubscriptionManager> _mockSubscriptionManager;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<TenantResolverMiddleware>>();
            _mockContextAccessor = new Mock<ITenantContextAccessor>();
            _mockTenantService = new Mock<ITenantService>();
            _mockSubscriptionManager = new Mock<ISubscriptionManager>();
        }

        private TenantResolverMiddleware CreateMiddleware(RequestDelegate next = null)
        {
            next = next ?? new RequestDelegate(ctx => Task.CompletedTask);
            return new TenantResolverMiddleware(next, _mockLogger.Object);
        }

        #region Tenant ID Extraction Tests

        [TestMethod]
        public async Task InvokeAsync_WithXTenantIdHeader_ExtractsTenantId()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "test-tenant-001");

            var tenantConfig = new TenantConfig
            {
                TenantId = "test-tenant-001",
                TenantName = "Test Tenant",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            var subscription = new TenantSubscription
            {
                TenantId = "test-tenant-001",
                Tier = SubscriptionTier.Starter,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "test-tenant-001",
                QuotaExceeded = false,
                MonthlyApiCallsUsed = 50,
                MonthlyApiCallLimit = 5000
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("test-tenant-001"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("test-tenant-001"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("test-tenant-001"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowAPIAccess = true
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockTenantService.Verify(ts => ts.GetTenantAsync("test-tenant-001"), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task InvokeAsync_WithJwtTenantIdClaim_ExtractsTenantId()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();

            var tenantConfig = new TenantConfig
            {
                TenantId = "jwt-tenant-001",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            var subscription = new TenantSubscription
            {
                TenantId = "jwt-tenant-001",
                Tier = SubscriptionTier.Starter,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "jwt-tenant-001",
                QuotaExceeded = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("jwt-tenant-001"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("jwt-tenant-001"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("jwt-tenant-001"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowAPIAccess = true
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockTenantService.Verify(ts => ts.GetTenantAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task InvokeAsync_WithApiKeyInPath_ExtractsTenantId()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/api/tenant/api-key-123/resource";

            var tenantConfig = new TenantConfig
            {
                TenantId = "key-tenant-001",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            var subscription = new TenantSubscription
            {
                TenantId = "key-tenant-001",
                Tier = SubscriptionTier.Starter,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "key-tenant-001",
                QuotaExceeded = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync(It.IsAny<string>()))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync(It.IsAny<string>()))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(It.IsAny<SubscriptionTier>()))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockTenantService.Verify(ts => ts.GetTenantAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        #endregion

        #region Tenant Context Setup Tests

        [TestMethod]
        public async Task InvokeAsync_WithValidTenant_SetsUpContext()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "context-test-001");

            var tenantConfig = new TenantConfig
            {
                TenantId = "context-test-001",
                TenantName = "Context Test",
                IsActive = true,
                CurrentTier = SubscriptionTier.Professional
            };

            var subscription = new TenantSubscription
            {
                TenantId = "context-test-001",
                Tier = SubscriptionTier.Professional,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "context-test-001",
                QuotaExceeded = false,
                MonthlyApiCallsUsed = 1000,
                MonthlyApiCallLimit = 50000
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("context-test-001"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("context-test-001"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("context-test-001"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Professional,
                AllowAPIAccess = true,
                AllowCustomPrompts = true,
                AllowMultipleAgents = true,
                AllowWebhooks = true
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Professional))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockContextAccessor.Verify(ca => ca.SetTenantContext(It.IsAny<TenantContext>()), Times.Once);
        }

        #endregion

        #region Inactive Tenant Tests

        [TestMethod]
        public async Task InvokeAsync_WithInactiveTenant_ReturnsUnauthorized()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "inactive-tenant");

            var tenantConfig = new TenantConfig
            {
                TenantId = "inactive-tenant",
                IsActive = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("inactive-tenant"))
                .ReturnsAsync(tenantConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            Assert.AreEqual(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
        }

        #endregion

        #region Unknown Tenant Tests

        [TestMethod]
        public async Task InvokeAsync_WithUnknownTenant_ReturnsNotFound()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "unknown-tenant");

            _mockTenantService.Setup(ts => ts.GetTenantAsync("unknown-tenant"))
                .ReturnsAsync((TenantConfig)null);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
        }

        #endregion

        #region Feature Flags Tests

        [TestMethod]
        public async Task InvokeAsync_BuildsCorrectFeatureFlags_ForFreeTier()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "free-tier-test");

            var tenantConfig = new TenantConfig
            {
                TenantId = "free-tier-test",
                IsActive = true,
                CurrentTier = SubscriptionTier.Free
            };

            var subscription = new TenantSubscription
            {
                TenantId = "free-tier-test",
                Tier = SubscriptionTier.Free,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "free-tier-test",
                QuotaExceeded = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("free-tier-test"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("free-tier-test"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("free-tier-test"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Free,
                AllowAPIAccess = false,
                AllowCustomPrompts = false,
                AllowMultipleAgents = false,
                AllowWebhooks = false
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Free))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockContextAccessor.Verify(
                ca => ca.SetTenantContext(It.Is<TenantContext>(tc =>
                    tc.Features != null
                )),
                Times.Once
            );
        }

        [TestMethod]
        public async Task InvokeAsync_BuildsCorrectFeatureFlags_ForProfessionalTier()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "pro-tier-test");

            var tenantConfig = new TenantConfig
            {
                TenantId = "pro-tier-test",
                IsActive = true,
                CurrentTier = SubscriptionTier.Professional
            };

            var subscription = new TenantSubscription
            {
                TenantId = "pro-tier-test",
                Tier = SubscriptionTier.Professional,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "pro-tier-test",
                QuotaExceeded = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("pro-tier-test"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("pro-tier-test"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("pro-tier-test"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Professional,
                AllowAPIAccess = true,
                AllowCustomPrompts = true,
                AllowMultipleAgents = true,
                AllowWebhooks = true,
                PrioritySupport = true
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Professional))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockContextAccessor.Verify(
                ca => ca.SetTenantContext(It.Is<TenantContext>(tc =>
                    tc.Features != null
                )),
                Times.Once
            );
        }

        #endregion

        #region Quota Enforcement Tests

        [TestMethod]
        public async Task InvokeAsync_WithQuotaExceeded_SetsContinuesButMarksInContext()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "quota-exceeded-test");

            var tenantConfig = new TenantConfig
            {
                TenantId = "quota-exceeded-test",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            var subscription = new TenantSubscription
            {
                TenantId = "quota-exceeded-test",
                Tier = SubscriptionTier.Starter,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "quota-exceeded-test",
                QuotaExceeded = true,
                MonthlyApiCallsUsed = 5000,
                MonthlyApiCallLimit = 5000
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("quota-exceeded-test"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("quota-exceeded-test"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("quota-exceeded-test"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowAPIAccess = true
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockContextAccessor.Verify(
                ca => ca.SetTenantContext(It.Is<TenantContext>(tc =>
                    tc.QuotaExceeded == true
                )),
                Times.Once
            );
        }

        [TestMethod]
        public async Task InvokeAsync_WithValidQuota_MarksQuotaAsNotExceeded()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "valid-quota-test");

            var tenantConfig = new TenantConfig
            {
                TenantId = "valid-quota-test",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            var subscription = new TenantSubscription
            {
                TenantId = "valid-quota-test",
                Tier = SubscriptionTier.Starter,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "valid-quota-test",
                QuotaExceeded = false,
                MonthlyApiCallsUsed = 100,
                MonthlyApiCallLimit = 5000
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("valid-quota-test"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("valid-quota-test"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("valid-quota-test"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig
            {
                Tier = SubscriptionTier.Starter,
                AllowAPIAccess = true
            };

            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(SubscriptionTier.Starter))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            _mockContextAccessor.Verify(
                ca => ca.SetTenantContext(It.Is<TenantContext>(tc =>
                    tc.QuotaExceeded == false
                )),
                Times.Once
            );
        }

        #endregion

        #region Request Pipeline Tests

        [TestMethod]
        public async Task InvokeAsync_WithValidTenant_CallsNextMiddleware()
        {
            // Arrange
            var nextCalled = false;
            var next = new RequestDelegate(ctx => { nextCalled = true; return Task.CompletedTask; });
            var middleware = new TenantResolverMiddleware(next, _mockLogger.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("X-Tenant-ID", "next-test");

            var tenantConfig = new TenantConfig
            {
                TenantId = "next-test",
                IsActive = true,
                CurrentTier = SubscriptionTier.Starter
            };

            var subscription = new TenantSubscription
            {
                TenantId = "next-test",
                Tier = SubscriptionTier.Starter,
                Status = "active"
            };

            var quota = new TenantQuota
            {
                TenantId = "next-test",
                QuotaExceeded = false
            };

            _mockTenantService.Setup(ts => ts.GetTenantAsync("next-test"))
                .ReturnsAsync(tenantConfig);

            _mockTenantService.Setup(ts => ts.GetSubscriptionAsync("next-test"))
                .ReturnsAsync(subscription);

            _mockTenantService.Setup(ts => ts.GetQuotaAsync("next-test"))
                .ReturnsAsync(quota);

            var tierConfig = new SubscriptionTierConfig { Tier = SubscriptionTier.Starter };
            _mockSubscriptionManager.Setup(sm => sm.GetTierConfigAsync(It.IsAny<SubscriptionTier>()))
                .ReturnsAsync(tierConfig);

            // Act
            await middleware.InvokeAsync(httpContext, _mockContextAccessor.Object, _mockTenantService.Object, _mockSubscriptionManager.Object);

            // Assert
            Assert.IsTrue(nextCalled);
        }

        #endregion
    }
}