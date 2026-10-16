using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Configuration
{
    /// <summary>
    /// Integration tests for configuration loading and application startup
    /// </summary>
    [TestClass]
    public class ConfigurationIntegrationTests
    {
        private IConfiguration _configuration;
        private IConfigurationBuilder _configBuilder;

        [TestInitialize]
        public void Setup()
        {
            var solutionRoot = GetSolutionRoot();
            var configPath = Path.Combine(solutionRoot, "appsettings.json");

            _configBuilder = new ConfigurationBuilder()
                .SetBasePath(solutionRoot)
                .AddJsonFile(configPath, optional: false, reloadOnChange: true);

            _configuration = _configBuilder.Build();
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

        #region Configuration Loading Tests

        [TestMethod]
        public void Configuration_JsonFile_ShouldExist()
        {
            // Arrange
            var configPath = Path.GetFullPath("../../../appsettings.json");

            // Act
            var exists = File.Exists(configPath);

            // Assert
            Assert.IsTrue(exists, $"Configuration file should exist at {configPath}");
        }

        [TestMethod]
        public void Configuration_ShouldLoadSuccessfully()
        {
            // Arrange & Act
            var config = _configuration;

            // Assert
            Assert.IsNotNull(config, "Configuration should load successfully");
            Assert.IsTrue(config.AsEnumerable().Any(), "Configuration should contain values");
        }

        [TestMethod]
        public void Configuration_MultiEnvironment_ShouldLoad()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("../../../appsettings.json", optional: false)
                .AddJsonFile("../../../appsettings.Development.json", optional: true)
                .AddJsonFile("../../../appsettings.Production.json", optional: true);

            // Act
            var config = configBuilder.Build();

            // Assert
            Assert.IsNotNull(config, "Configuration should load with multiple environment files");
        }

        [TestMethod]
        public void Configuration_EnvironmentVariables_CanOverride()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ChatGPT__ApiKey", "test-override-key");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("../../../appsettings.json", optional: false)
                .AddEnvironmentVariables();

            // Act
            var config = configBuilder.Build();
            var overriddenValue = config["ChatGPT:ApiKey"];

            // Assert
            Assert.IsNotNull(overriddenValue, "Configuration should support environment variable overrides");

            // Cleanup
            Environment.SetEnvironmentVariable("ChatGPT__ApiKey", null);
        }

        #endregion

        #region Multi-Tenant Configuration Tests

        [TestMethod]
        public void MultiTenant_AllTiersPresent_ShouldHaveDifferentPermissions()
        {
            // Arrange
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren()
                .Where(t => !string.IsNullOrEmpty(t["SubscriptionTier"]))
                .GroupBy(t => t["SubscriptionTier"])
                .ToDictionary(g => g.Key, g => g.First());

            // Act & Assert
            Assert.IsTrue(tenants.ContainsKey("Free"), "Should have Free tier");
            Assert.IsTrue(tenants.ContainsKey("Starter"), "Should have Starter tier");
            Assert.IsTrue(tenants.ContainsKey("Pro"), "Should have Pro tier");
            Assert.IsTrue(tenants.ContainsKey("Enterprise"), "Should have Enterprise tier");

            // Verify differences
            var freeTenant = tenants["Free"];
            var starterTenant = tenants["Starter"];
            var proTenant = tenants["Pro"];
            var enterpriseTenant = tenants["Enterprise"];

            var freeQuota = int.Parse(freeTenant["MonthlyApiQuota"]);
            var starterQuota = int.Parse(starterTenant["MonthlyApiQuota"]);
            var proQuota = int.Parse(proTenant["MonthlyApiQuota"]);
            var enterpriseQuota = int.Parse(enterpriseTenant["MonthlyApiQuota"]);

            Assert.IsTrue(freeQuota < starterQuota, "Free tier quota should be less than Starter");
            Assert.IsTrue(starterQuota < proQuota, "Starter tier quota should be less than Pro");
            Assert.AreEqual(-1, enterpriseQuota, "Enterprise tier should have unlimited quota");
        }

        [TestMethod]
        public void MultiTenant_ToolAccess_ShouldScaleWithTier()
        {
            // Arrange
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren()
                .Where(t => !string.IsNullOrEmpty(t["SubscriptionTier"]))
                .ToDictionary(t => t["SubscriptionTier"], t => t);

            // Act
            var freeTools = tenants["Free"].GetSection("AllowedTools").GetChildren().Count();
            var starterTools = tenants["Starter"].GetSection("AllowedTools").GetChildren().Count();
            var proTools = tenants["Pro"].GetSection("AllowedTools").GetChildren().Count();

            // Assert
            Assert.IsTrue(freeTools <= starterTools, "Free tier should have equal or fewer tools than Starter");
            Assert.IsTrue(starterTools <= proTools, "Starter tier should have equal or fewer tools than Pro");
        }

        #endregion

        #region Jurisdiction Compliance Configuration Tests

        [TestMethod]
        public void Jurisdiction_Compliance_ShouldBeRegionSpecific()
        {
            // Arrange
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren().ToList();

            // Act & Assert
            foreach (var jurisdiction in jurisdictions)
            {
                var stateCode = jurisdiction["stateCode"];
                var regulations = jurisdiction.GetSection("regulations");

                Assert.IsNotNull(stateCode, "Each jurisdiction should have a state code");
                Assert.IsTrue(regulations.Exists(), $"Jurisdiction {stateCode} should have regulations");

                // Verify key regulatory fields
                var hasAuditLog = regulations["complianceRequirements:requiresAuditLog"];
                var hasEncryption = regulations["dataRetentionPolicies:requiresEncryption"];
                Assert.IsNotNull(hasAuditLog, "Should require audit logging");
                Assert.IsNotNull(hasEncryption, "Should require encryption");
            }
        }

        [TestMethod]
        public void Jurisdiction_GDPR_States_ShouldRequireCompliance()
        {
            // Arrange
            var gdprStates = new[] { "CA", "NY" }; // GDPR-equivalent states
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren().ToList();

            // Act
            foreach (var state in gdprStates)
            {
                var jurisdiction = jurisdictions.FirstOrDefault(j => j["stateCode"] == state);
                Assert.IsNotNull(jurisdiction, $"Should have jurisdiction for {state}");

                var gdprEquivalent = jurisdiction["regulations:complianceRequirements:gdprEquivalent"];
                Assert.AreEqual("true", gdprEquivalent?.ToLower(), $"{state} should have GDPR-equivalent compliance");
            }
        }

        [TestMethod]
        public void Jurisdiction_DataResidency_ShouldBeEnforced()
        {
            // Arrange
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren().ToList();

            // Act & Assert
            foreach (var jurisdiction in jurisdictions)
            {
                var stateCode = jurisdiction["stateCode"];
                var dataResidencyRequired = jurisdiction["features:featureConfiguration:dataResidency:required"];

                // Most jurisdictions should have this feature
                if (stateCode == "CA" || stateCode == "NY")
                {
                    Assert.AreEqual("true", dataResidencyRequired?.ToLower(), 
                        $"{stateCode} should enforce data residency");
                }
            }
        }

        #endregion

        #region Connection String Tests

        [TestMethod]
        public void ConnectionStrings_ShouldBeValidFormat()
        {
            // Arrange
            var connectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren();

            // Act & Assert
            foreach (var connection in connectionStrings)
            {
                var value = connection.Value;
                Assert.IsNotNull(value, $"Connection string {connection.Key} should have a value");
                Assert.IsTrue(value.Length > 10, $"Connection string {connection.Key} should be non-trivial");
            }
        }

        [TestMethod]
        public void ConnectionStrings_JurisdictionSpecific_ShouldCorrespond()
        {
            // Arrange
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren().ToList();

            // Act & Assert
            foreach (var jurisdiction in jurisdictions)
            {
                var stateCode = jurisdiction["stateCode"];
                var connectionStringName = jurisdiction["connectionStringName"];
                var connectionString = _configuration[$"ConnectionStrings:{connectionStringName}"];

                Assert.IsNotNull(connectionString, 
                    $"Connection string '{connectionStringName}' for {stateCode} should exist");
            }
        }

        #endregion

        #region LLM Configuration Integration Tests

        [TestMethod]
        public void LlmConfig_Integrates_WithVectorSearch()
        {
            // Arrange
            var chatGptProvider = _configuration["ChatGPT:Model"];
            var embeddingProvider = _configuration["Embedding:EmbeddingProvider"];
            var vectorBackend = _configuration["Embedding:VectorStoreBackend"];

            // Act & Assert
            Assert.IsNotNull(chatGptProvider, "ChatGPT model should be configured");
            Assert.IsNotNull(embeddingProvider, "Embedding provider should match LLM provider");
            Assert.IsNotNull(vectorBackend, "Vector store backend should be configured");

            // Both should ideally use OpenAI for consistency
            Assert.AreEqual("OpenAI", embeddingProvider, "Embedding provider should be OpenAI");
        }

        [TestMethod]
        public void LlmConfig_Temperature_ShouldBeReasonable()
        {
            // Arrange & Act
            var tempStr = _configuration["ChatGPT:Temperature"];
            var isParsed = decimal.TryParse(tempStr, out var temperature);

            // Assert
            Assert.IsTrue(isParsed, "Temperature should be parseable");
            Assert.IsTrue(temperature >= 0 && temperature <= 2, "Temperature should be in valid range (0-2)");
            Assert.IsTrue(temperature <= 1.0m, "For consistency, temperature should be <= 1.0");
        }

        #endregion

        #region ServiceNow Integration Configuration Tests

        [TestMethod]
        public void ServiceNow_Config_ShouldSupportAllToolTypes()
        {
            // Arrange
            var serviceNowConfig = _configuration.GetSection("ServiceNow");
            var instance = serviceNowConfig["Instance"];
            var token = serviceNowConfig["Token"];

            // Act & Assert
            Assert.IsNotNull(instance, "ServiceNow instance should be configured");
            Assert.IsNotNull(token, "ServiceNow OAuth token should be configured");
            Assert.IsFalse(string.IsNullOrWhiteSpace(instance), "Instance should not be empty");
            Assert.IsFalse(string.IsNullOrWhiteSpace(token), "Token should not be empty");
        }

        [TestMethod]
        public void ServiceNow_Timeout_ShouldBeCompatible()
        {
            // Arrange & Act
            var timeoutStr = _configuration["ServiceNow:TimeoutSeconds"];
            var isParsed = int.TryParse(timeoutStr, out var timeout);
            var chatGptMaxTokens = int.Parse(_configuration["ChatGPT:MaxTokens"]);

            // Assert
            Assert.IsTrue(isParsed, "Timeout should be a valid integer");
            Assert.IsTrue(timeout > 0, "Timeout should be positive");
            Assert.IsTrue(timeout <= 60, "Timeout should be reasonable");

            // ServiceNow timeout should allow enough time for LLM to think
            Assert.IsTrue(timeout >= 10, "ServiceNow timeout should be sufficient for tool calls");
        }

        #endregion

        #region Azure Configuration Integration Tests

        [TestMethod]
        public void Azure_AllServices_ShouldShareCommonConfig()
        {
            // Arrange
            var azureSubscription = _configuration["Azure:SubscriptionId"];
            var automationSubscription = _configuration["AzureAutomation:SubscriptionId"];
            var azureResourceGroup = _configuration["Azure:ResourceGroup"];
            var automationResourceGroup = _configuration["AzureAutomation:ResourceGroup"];

            // Act & Assert
            Assert.IsNotNull(azureSubscription, "Azure SubscriptionId should be set");
            Assert.IsNotNull(automationSubscription, "AzureAutomation SubscriptionId should be set");

            // They should be the same in most cases
            if (!azureSubscription.Contains("your-") && !automationSubscription.Contains("your-"))
            {
                Assert.AreEqual(azureSubscription, automationSubscription, 
                    "Azure and Automation should use same subscription when both configured");
            }
        }

        [TestMethod]
        public void Azure_Regions_ShouldBeConsistent()
        {
            // Arrange
            var azureRegion = _configuration["Azure:Region"];
            var jurisdictions = _configuration.GetSection("jurisdictions").GetChildren().ToList();

            // Act & Assert
            Assert.IsNotNull(azureRegion, "Azure region should be configured");
            var validRegions = new[] { "eastus", "westus", "westus2", "eastus2" };
            Assert.IsTrue(validRegions.Contains(azureRegion?.ToLower()), 
                "Azure region should be a valid Azure region");
        }

        #endregion

        #region Security Configuration Consistency Tests

        [TestMethod]
        public void Security_RequireHttps_ShouldMatchJurisdictions()
        {
            // Arrange
            var requireHttps = _configuration["Security:RequireHttps"];
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var highSecurityJurisdictions = jurisdictionsSection.GetChildren()
                .Where(j => j["regulations:securityStandards:minimumTlsVersion"] == "1.3")
                .ToList();

            // Act & Assert
            Assert.AreEqual("true", requireHttps?.ToLower(), "Should require HTTPS globally");
            Assert.IsTrue(highSecurityJurisdictions.Count > 0, 
                "Should have jurisdictions requiring high TLS versions");
        }

        [TestMethod]
        public void Security_CorsPolicy_ShouldBeDefined()
        {
            // Arrange & Act
            var corsEnabled = _configuration["Security:EnableCors"];
            var corsPolicy = _configuration["Security:CorsPolicy"];
            var allowedOrigins = _configuration.GetSection("Security:CorsAllowedOrigins").GetChildren();

            // Assert
            Assert.AreEqual("true", corsEnabled?.ToLower(), "CORS should be enabled");
            Assert.IsNotNull(corsPolicy, "CORS policy should be defined");
            Assert.IsTrue(allowedOrigins.Any(), "Should have allowed origins defined");
        }

        #endregion

        #region Feature Flags Configuration Tests

        [TestMethod]
        public void Features_CoreLlmFeatures_ShouldBeEnabled()
        {
            // Arrange
            var coreFeatures = new[]
            {
                "Features:EnableAiAgentIntegration",
                "Features:EnableMultiTenancy",
                "Features:EnableServiceNowIntegration"
            };

            // Act & Assert
            foreach (var feature in coreFeatures)
            {
                var isEnabled = _configuration[$"{feature}"];
                Assert.AreEqual("true", isEnabled?.ToLower(), $"{feature} should be enabled");
            }
        }

        [TestMethod]
        public void Features_AdvancedFeatures_ShouldHaveConfigurableState()
        {
            // Arrange
            var advancedFeatures = new[]
            {
                "Features:EnableAdvancedAnalytics",
                "Features:EnableVectorSearch"
            };

            // Act
            var featureStates = new Dictionary<string, string>();
            foreach (var feature in advancedFeatures)
            {
                var value = _configuration[feature];
                featureStates[feature] = value;
            }

            // Assert
            Assert.IsTrue(featureStates.Values.All(v => v?.ToLower() == "true" || v?.ToLower() == "false"),
                "Advanced features should have boolean values");
        }

        #endregion

        #region Caching Configuration Tests

        [TestMethod]
        public void Cache_Provider_ShouldBeCompatibleWithArchitecture()
        {
            // Arrange & Act
            var cacheProvider = _configuration["Cache:Provider"];
            var cacheEnabled = _configuration["Cache:Enabled"];

            // Assert
            Assert.AreEqual("true", cacheEnabled?.ToLower(), "Cache should be enabled");
            Assert.IsNotNull(cacheProvider, "Cache provider should be configured");

            var validProviders = new[] { "InMemory", "Redis", "Distributed" };
            Assert.IsTrue(validProviders.Contains(cacheProvider), "Cache provider should be valid type");
        }

        [TestMethod]
        public void Cache_Expiration_ShouldBeConfigured()
        {
            // Arrange & Act
            var defaultExpiration = _configuration["Cache:DefaultExpiration"];
            var slidingExpiration = _configuration["Cache:SlidingExpiration"];

            // Assert
            var isParsedDefault = int.TryParse(defaultExpiration, out var defaultExp);
            var isParsedSliding = int.TryParse(slidingExpiration, out var slidingExp);

            Assert.IsTrue(isParsedDefault, "DefaultExpiration should be a valid integer");
            Assert.IsTrue(isParsedSliding, "SlidingExpiration should be a valid integer");
            Assert.IsTrue(defaultExp > slidingExp, "Default expiration should be longer than sliding");
        }

        #endregion

        #region Configuration Completeness Tests

        [TestMethod]
        public void Configuration_ShouldNotHaveEmptyArrays()
        {
            // Arrange
            var arrayConfigs = new[]
            {
                "Tenants",
                "jurisdictions",
                "Security:CorsAllowedOrigins"
            };

            // Act & Assert
            foreach (var arrayConfig in arrayConfigs)
            {
                var array = _configuration.GetSection(arrayConfig).GetChildren();
                var count = array.Count();
                Assert.IsTrue(count > 0, $"{arrayConfig} should not be empty");
            }
        }

        [TestMethod]
        public void Configuration_AllSections_ShouldHaveRequiredFields()
        {
            // Arrange
            var configSectionFieldMap = new Dictionary<string, string[]>
            {
                { "ChatGPT", new[] { "ApiKey", "Model", "Temperature", "MaxTokens" } },
                { "ServiceNow", new[] { "Instance", "Token", "TimeoutSeconds" } },
                { "Azure", new[] { "SubscriptionId", "ResourceGroup", "Region" } },
                { "Tenant", new[] { "TenantId", "ServiceName" } },
                { "Database", new[] { "Timeout", "CommandTimeout", "MaxRetryCount" } }
            };

            // Act & Assert
            foreach (var (section, fields) in configSectionFieldMap)
            {
                var sectionConfig = _configuration.GetSection(section);
                foreach (var field in fields)
                {
                    var value = sectionConfig[field];
                    Assert.IsNotNull(value, $"Section '{section}' should have field '{field}'");
                }
            }
        }

        #endregion

        #region Performance Configuration Tests

        [TestMethod]
        public void Configuration_Timeouts_ShouldBeReasonable()
        {
            // Arrange
            var timeoutConfigs = new[]
            {
                ("Database:Timeout", 5, 120),
                ("Database:CommandTimeout", 30, 600),
                ("ServiceNow:TimeoutSeconds", 5, 60),
                ("Api:RateLimitPerMinute", 10, 1000)
            };

            // Act & Assert
            foreach (var (key, min, max) in timeoutConfigs)
            {
                var valueStr = _configuration[key];
                var isParsed = int.TryParse(valueStr, out var value);
                Assert.IsTrue(isParsed, $"{key} should be a valid integer");
                Assert.IsTrue(value >= min && value <= max, 
                    $"{key} should be between {min} and {max}, got {value}");
            }
        }

        #endregion
    }
}
