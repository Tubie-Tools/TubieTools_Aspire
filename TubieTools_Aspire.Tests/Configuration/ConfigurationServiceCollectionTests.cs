using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Configuration
{
    // Option configuration classes for testing
    public class ChatGPTConfigOptions
    {
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
    }

    public class ServiceNowConfigOptions
    {
        public string? Instance { get; set; }
        public string? InstanceUrl { get; set; }
        public string? Token { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
    }

    public class TenantOption
    {
        public string? TenantId { get; set; }
        public string? DisplayName { get; set; }
        public string? SubscriptionTier { get; set; }
        public int MonthlyApiQuota { get; set; }
        public string[]? AllowedTools { get; set; }
        public string[]? SupportedModels { get; set; }
        public bool IsActive { get; set; }
    }

    public class TenantConfigurationOptions
    {
        public TenantOption[]? Tenants { get; set; }
    }

    public class JurisdictionOption
    {
        public string? JurisdictionId { get; set; }
        public string? StateCode { get; set; }
        public string? ConnectionStringName { get; set; }
        public bool IsActive { get; set; }
    }

    public class JurisdictionConfigurationOptions
    {
        public JurisdictionOption[]? Jurisdictions { get; set; }
    }

    /// <summary>
    /// End-to-end configuration and DI container tests
    /// </summary>
    [TestClass]
    public class ConfigurationServiceCollectionTests
    {
        private IConfiguration _configuration;
        private IServiceCollection _services;
        private IServiceProvider _serviceProvider;

        [TestInitialize]
        public void Setup()
        {
            // Load configuration
            var solutionRoot = GetSolutionRoot();
            var configPath = Path.Combine(solutionRoot, "appsettings.json");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(solutionRoot)
                .AddJsonFile(configPath, optional: false)
                .Build();

            // Setup DI container
            _services = new ServiceCollection();
            _services.AddSingleton(_configuration);
        }

        private string GetSolutionRoot()
        {
            var currentDir = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (File.Exists(Path.Combine(currentDir, "TubieTools_Aspire.sln")) ||
                    File.Exists(Path.Combine(currentDir, "appsettings.json")))
                {
                    return currentDir;
                }

                var parent = Directory.GetParent(currentDir);
                currentDir = parent?.FullName;
            }

            throw new DirectoryNotFoundException("Solution root not found. Ensure appsettings.json is in the solution directory.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            //_serviceProvider?.Dispose();
        }

        #region DI Container Configuration Tests

        [TestMethod]
        public void ServiceCollection_ShouldAcceptConfiguration()
        {
            // Arrange & Act
            _serviceProvider = _services.BuildServiceProvider();
            var config = _serviceProvider.GetRequiredService<IConfiguration>();

            // Assert
            Assert.IsNotNull(config, "Configuration should be resolvable from DI container");
        }

        [TestMethod]
        public void ChatGPTConfig_Should_BindFromConfiguration()
        {
            // Arrange
            _services.Configure<ChatGPTConfigOptions>(_configuration.GetSection("ChatGPT"));
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<ChatGPTConfigOptions>>();

            // Assert
            Assert.IsNotNull(options, "ChatGPT configuration should bind");
            Assert.IsNotNull(options.Value, "ChatGPT config value should exist");
        }

        [TestMethod]
        public void ServiceNowConfig_Should_BindFromConfiguration()
        {
            // Arrange
            _services.Configure<ServiceNowConfigOptions>(_configuration.GetSection("ServiceNow"));
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<ServiceNowConfigOptions>>();

            // Assert
            Assert.IsNotNull(options, "ServiceNow configuration should bind");
            Assert.IsNotNull(options.Value.Instance, "ServiceNow instance should be configured");
        }

        #endregion

        #region Configuration Options Validation

        [TestMethod]
        public void ChatGPTConfigOptions_ApiKey_ShouldNotBeEmpty()
        {
            // Arrange
            _services.Configure<ChatGPTConfigOptions>(_configuration.GetSection("ChatGPT"));
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<ChatGPTConfigOptions>>();

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(options.Value.ApiKey), 
                "ChatGPT ApiKey should be configured");
        }

        [TestMethod]
        public void ChatGPTConfigOptions_Model_ShouldBeValid()
        {
            // Arrange
            _services.Configure<ChatGPTConfigOptions>(_configuration.GetSection("ChatGPT"));
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<ChatGPTConfigOptions>>();
            var model = options.Value.Model;

            // Assert
            Assert.IsNotNull(model, "Model should be configured");
            Assert.IsTrue(model.Contains("gpt"), "Model should be a valid GPT model");
        }

        [TestMethod]
        public void ChatGPTConfigOptions_Temperature_ShouldBeInRange()
        {
            // Arrange
            _services.Configure<ChatGPTConfigOptions>(_configuration.GetSection("ChatGPT"));
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<ChatGPTConfigOptions>>();

            // Assert
            Assert.IsTrue(options.Value.Temperature >= 0 && options.Value.Temperature <= 2,
                "Temperature should be between 0 and 2");
        }

        #endregion

        #region Multi-Tenant Configuration Tests

        [TestMethod]
        public void TenantConfiguration_ShouldBinding()
        {
            // Arrange
            _services.Configure<TenantConfigurationOptions>(_configuration);
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<TenantConfigurationOptions>>();

            // Assert
            Assert.IsNotNull(options, "Tenant configuration should bind");
            Assert.IsNotNull(options.Value.Tenants, "Tenants array should exist");
            Assert.IsTrue(options.Value.Tenants.Length > 0, "Should have at least one tenant");
        }

        [TestMethod]
        public void TenantConfiguration_AllTiersShouldExist()
        {
            // Arrange
            _services.Configure<TenantConfigurationOptions>(_configuration);
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<TenantConfigurationOptions>>();
            var tiers = new HashSet<string>();

            foreach (var tenant in options.Value.Tenants)
            {
                if (!string.IsNullOrEmpty(tenant.SubscriptionTier))
                {
                    tiers.Add(tenant.SubscriptionTier);
                }
            }

            // Assert
            Assert.IsTrue(tiers.Contains("Free"), "Free tier should exist");
            Assert.IsTrue(tiers.Contains("Starter"), "Starter tier should exist");
            Assert.IsTrue(tiers.Contains("Pro"), "Pro tier should exist");
            Assert.IsTrue(tiers.Contains("Enterprise"), "Enterprise tier should exist");
        }

        #endregion

        #region Jurisdiction Configuration Tests

        [TestMethod]
        public void JurisdictionConfiguration_ShouldBinding()
        {
            // Arrange
            _services.Configure<JurisdictionConfigurationOptions>(_configuration);
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<JurisdictionConfigurationOptions>>();

            // Assert
            Assert.IsNotNull(options, "Jurisdiction configuration should bind");
            Assert.IsNotNull(options.Value.Jurisdictions, "Jurisdictions array should exist");
            Assert.IsTrue(options.Value.Jurisdictions.Length >= 3, "Should have at least 3 jurisdictions");
        }

        [TestMethod]
        public void JurisdictionConfiguration_RequiredStates_ShouldExist()
        {
            // Arrange
            _services.Configure<JurisdictionConfigurationOptions>(_configuration);
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var options = _serviceProvider.GetRequiredService<IOptionsSnapshot<JurisdictionConfigurationOptions>>();
            var stateCodes = new HashSet<string>();

            foreach (var jurisdiction in options.Value.Jurisdictions)
            {
                stateCodes.Add(jurisdiction.StateCode);
            }

            // Assert
            Assert.IsTrue(stateCodes.Contains("CA"), "California should be defined");
            Assert.IsTrue(stateCodes.Contains("TX"), "Texas should be defined");
            Assert.IsTrue(stateCodes.Contains("NY"), "New York should be defined");
        }

        #endregion

        #region Connection String Configuration Tests

        [TestMethod]
        public void ConnectionStrings_AllRequired_ShouldBeAccessible()
        {
            // Arrange
            var requiredConnections = new[]
            {
                "ConnectionStrings:DefaultConnection",
                "ConnectionStrings:FoundryConnection",
                "ConnectionStrings:FoundryConnection_CA",
                "ConnectionStrings:FoundryConnection_TX",
                "ConnectionStrings:FoundryConnection_NY"
            };

            // Act & Assert
            foreach (var connString in requiredConnections)
            {
                var value = _configuration[connString];
                Assert.IsNotNull(value, $"Connection string '{connString}' should exist");
                Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"Connection string '{connString}' should not be empty");
            }
        }

        #endregion

        #region Feature Flags Tests

        [TestMethod]
        public void FeatureFlags_CoreFeatures_ShouldBeEnabled()
        {
            // Arrange
            var coreFeatures = new[]
            {
                "Features:EnableAiAgentIntegration",
                "Features:EnableMultiTenancy",
                "Features:EnableServiceNowIntegration"
            };

            // Act
            var enabledFeatures = new Dictionary<string, bool>();
            foreach (var feature in coreFeatures)
            {
                var value = _configuration[feature];
                enabledFeatures[feature] = value?.ToLower() == "true";
            }

            // Assert
            foreach (var (feature, enabled) in enabledFeatures)
            {
                Assert.IsTrue(enabled, $"Core feature '{feature}' should be enabled");
            }
        }

        [TestMethod]
        public void FeatureFlags_OptionalFeatures_ShouldHaveConsistentState()
        {
            // Arrange
            var optionalFeatures = new[]
            {
                "Features:EnableAdvancedAnalytics",
                "Features:EnableVectorSearch"
            };

            // Act
            var featureStates = new Dictionary<string, bool>();
            foreach (var feature in optionalFeatures)
            {
                var value = _configuration[feature];
                if (bool.TryParse(value, out var enabled))
                {
                    featureStates[feature] = enabled;
                }
            }

            // Assert
            Assert.IsTrue(featureStates.Count > 0, "Should have feature flag states");
        }

        #endregion

        #region Security Configuration Tests

        [TestMethod]
        public void SecurityConfiguration_HttpsRequired_ShouldBeTrueForProduction()
        {
            // Arrange & Act
            var requireHttps = _configuration["Security:RequireHttps"];

            // Assert
            Assert.AreEqual("true", requireHttps?.ToLower(), "Production should require HTTPS");
        }

        [TestMethod]
        public void SecurityConfiguration_CorsEnabled_ShouldHaveOrigins()
        {
            // Arrange & Act
            var corsEnabled = _configuration["Security:EnableCors"];
            var corsAllowedOrigins = _configuration.GetSection("Security:CorsAllowedOrigins").GetChildren();

            // Assert
            Assert.AreEqual("true", corsEnabled?.ToLower(), "CORS should be enabled");
            Assert.IsTrue(corsAllowedOrigins.Any(), "Should have CORS allowed origins");
        }

        #endregion

        #region Cache Configuration Tests

        [TestMethod]
        public void CacheConfiguration_ProviderShouldBeValid()
        {
            // Arrange & Act
            var cacheEnabled = _configuration["Cache:Enabled"];
            var cacheProvider = _configuration["Cache:Provider"];

            // Assert
            Assert.AreEqual("true", cacheEnabled?.ToLower(), "Cache should be enabled");
            Assert.IsNotNull(cacheProvider, "Cache provider should be configured");
            Assert.IsTrue(
                cacheProvider == "InMemory" || cacheProvider == "Redis" || cacheProvider == "Distributed",
                "Cache provider should be valid"
            );
        }

        [TestMethod]
        public void CacheConfiguration_ExpirationShouldBeConfigured()
        {
            // Arrange & Act
            var defaultExpiration = _configuration["Cache:DefaultExpiration"];
            var slidingExpiration = _configuration["Cache:SlidingExpiration"];

            // Assert
            Assert.IsTrue(int.TryParse(defaultExpiration, out var defExp), 
                "Default expiration should be a valid integer");
            Assert.IsTrue(int.TryParse(slidingExpiration, out var slidExp), 
                "Sliding expiration should be a valid integer");
            Assert.IsTrue(defExp > slidExp, "Default should be greater than sliding");
        }

        #endregion

        #region Aspire Configuration Tests

        [TestMethod]
        public void AspireConfiguration_DashboardPortShouldBeValid()
        {
            // Arrange & Act
            var dashboardPort = _configuration["Aspire:DashboardPort"];

            // Assert
            Assert.IsTrue(int.TryParse(dashboardPort, out var port), "Dashboard port should be a valid integer");
            Assert.IsTrue(port > 1024 && port <= 65535, "Dashboard port should be in valid range");
        }

        #endregion

        #region Configuration Completeness Tests

        [TestMethod]
        public void Configuration_AllCriticalSections_ShouldExist()
        {
            // Arrange
            var criticalSections = new[]
            {
                "Logging",
                "ConnectionStrings",
                "Authentication",
                "ChatGPT",
                "ServiceNow",
                "Azure",
                "Tenant",
                "Tenants",
                "jurisdictions",
                "Database",
                "Security",
                "Features",
                "Cache"
            };

            // Act & Assert
            foreach (var section in criticalSections)
            {
                var sectionConfig = _configuration.GetSection(section);
                Assert.IsTrue(sectionConfig.Exists(), $"Critical section '{section}' should exist");
            }
        }

        [TestMethod]
        public void Configuration_ShouldBeFullyLoaded()
        {
            // Arrange & Act
            var allConfigValues = _configuration.AsEnumerable();
            var valueCount = allConfigValues.Count();

            // Assert
            Assert.IsTrue(valueCount > 100, "Configuration should have 100+ values loaded");
        }

        #endregion
    }
    
}
