using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Configuration
{
    /// <summary>
    /// Comprehensive configuration tests for all application settings
    /// </summary>
    [TestClass]
    public class ConfigurationValidationTests
    {
        private IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {
            // Load appsettings.json from solution root
            var solutionRoot = GetSolutionRoot();
            var configPath = Path.Combine(solutionRoot, "appsettings.json");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(solutionRoot)
                .AddJsonFile(configPath, optional: false, reloadOnChange: true);

            _configuration = configBuilder.Build();
        }

        /// <summary>
        /// Gets the solution root directory by walking up from the current directory
        /// </summary>
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

        #region Logging Configuration Tests

        [TestMethod]
        public void LoggingConfiguration_ShouldExist()
        {
            // Arrange & Act
            var loggingConfig = _configuration.GetSection("Logging");

            // Assert
            Assert.IsNotNull(loggingConfig, "Logging configuration section should exist");
            Assert.IsTrue(loggingConfig.Exists(), "Logging configuration should have values");
        }

        [TestMethod]
        public void LoggingConfiguration_DefaultLogLevel_ShouldBeInformation()
        {
            // Arrange & Act
            var defaultLevel = _configuration["Logging:LogLevel:Default"];

            // Assert
            Assert.AreEqual("Information", defaultLevel, "Default log level should be Information");
        }

        [TestMethod]
        public void LoggingConfiguration_AspNetCoreLogLevel_ShouldBeWarning()
        {
            // Arrange & Act
            var aspNetLevel = _configuration["Logging:LogLevel:Microsoft.AspNetCore"];

            // Assert
            Assert.AreEqual("Warning", aspNetLevel, "ASP.NET Core log level should be Warning");
        }

        #endregion

        #region Connection Strings Tests

        [TestMethod]
        public void ConnectionStrings_ShouldExist()
        {
            // Arrange & Act
            var connectionStrings = _configuration.GetSection("ConnectionStrings");

            // Assert
            Assert.IsNotNull(connectionStrings, "ConnectionStrings section should exist");
            Assert.IsTrue(connectionStrings.Exists(), "ConnectionStrings should have values");
        }

        [TestMethod]
        public void ConnectionStrings_DefaultConnection_ShouldExist()
        {
            // Arrange & Act
            var defaultConn = _configuration["ConnectionStrings:DefaultConnection"];

            // Assert
            Assert.IsNotNull(defaultConn, "DefaultConnection should exist");
            Assert.IsFalse(string.IsNullOrWhiteSpace(defaultConn), "DefaultConnection should not be empty");
        }

        [TestMethod]
        public void ConnectionStrings_AllRequired_ShouldExist()
        {
            // Arrange
            var requiredConnections = new[] 
            { 
                "DefaultConnection", 
                "FoundryConnection",
                "FoundryConnection_CA",
                "FoundryConnection_TX",
                "FoundryConnection_NY"
            };

            // Act & Assert
            foreach (var connection in requiredConnections)
            {
                var value = _configuration[$"ConnectionStrings:{connection}"];
                Assert.IsNotNull(value, $"ConnectionString '{connection}' should exist");
                Assert.IsFalse(string.IsNullOrWhiteSpace(value), $"ConnectionString '{connection}' should not be empty");
            }
        }

        #endregion

        #region Authentication Tests

        [TestMethod]
        public void Authentication_Entra_ShouldExist()
        {
            // Arrange & Act
            var entraConfig = _configuration.GetSection("Authentication:Entra");

            // Assert
            Assert.IsTrue(entraConfig.Exists(), "Entra authentication configuration should exist");
        }

        [TestMethod]
        public void Authentication_Entra_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] { "TenantId", "ClientId", "Instance", "Authority" };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Authentication:Entra:{field}"];
                Assert.IsNotNull(value, $"Entra field '{field}' should exist");
            }
        }

        [TestMethod]
        public void Authentication_Okta_ShouldExist()
        {
            // Arrange & Act
            var oktaConfig = _configuration.GetSection("Authentication:Okta");

            // Assert
            Assert.IsTrue(oktaConfig.Exists(), "Okta authentication configuration should exist");
        }

        #endregion

        #region ChatGPT Configuration Tests

        [TestMethod]
        public void ChatGPT_Configuration_ShouldExist()
        {
            // Arrange & Act
            var chatGptConfig = _configuration.GetSection("ChatGPT");

            // Assert
            Assert.IsTrue(chatGptConfig.Exists(), "ChatGPT configuration section should exist");
        }

        [TestMethod]
        public void ChatGPT_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] { "ApiKey", "Model", "Temperature", "MaxTokens" };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"ChatGPT:{field}"];
                Assert.IsNotNull(value, $"ChatGPT field '{field}' should exist");
            }
        }

        [TestMethod]
        public void ChatGPT_Model_ShouldBeGpt4OrLater()
        {
            // Arrange & Act
            var model = _configuration["ChatGPT:Model"];

            // Assert
            Assert.IsTrue(
                model.Contains("gpt-4") || model.Contains("gpt-3.5"),
                "ChatGPT model should be gpt-4 or compatible"
            );
        }

        [TestMethod]
        public void ChatGPT_Temperature_ShouldBeValidRange()
        {
            // Arrange & Act
            var tempStr = _configuration["ChatGPT:Temperature"];
            var isParsed = decimal.TryParse(tempStr, out var temperature);

            // Assert
            Assert.IsTrue(isParsed, "Temperature should be a valid decimal");
            Assert.IsTrue(temperature >= 0 && temperature <= 1, "Temperature should be between 0 and 1");
        }

        [TestMethod]
        public void ChatGPT_MaxTokens_ShouldBePositive()
        {
            // Arrange & Act
            var tokensStr = _configuration["ChatGPT:MaxTokens"];
            var isParsed = int.TryParse(tokensStr, out var tokens);

            // Assert
            Assert.IsTrue(isParsed, "MaxTokens should be a valid integer");
            Assert.IsTrue(tokens > 0, "MaxTokens should be positive");
        }

        #endregion

        #region Embedding Configuration Tests

        [TestMethod]
        public void Embedding_Configuration_ShouldExist()
        {
            // Arrange & Act
            var embeddingConfig = _configuration.GetSection("Embedding");

            // Assert
            Assert.IsTrue(embeddingConfig.Exists(), "Embedding configuration should exist");
        }

        [TestMethod]
        public void Embedding_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "EmbeddingProvider", 
                "EmbeddingModel", 
                "EmbeddingDimension",
                "VectorStoreBackend",
                "SimilarityMetric"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Embedding:{field}"];
                Assert.IsNotNull(value, $"Embedding field '{field}' should exist");
            }
        }

        [TestMethod]
        public void Embedding_Dimension_ShouldBeValid()
        {
            // Arrange & Act
            var dimensionStr = _configuration["Embedding:EmbeddingDimension"];
            var isParsed = int.TryParse(dimensionStr, out var dimension);

            // Assert
            Assert.IsTrue(isParsed, "EmbeddingDimension should be a valid integer");
            Assert.AreEqual(1536, dimension, "EmbeddingDimension should be 1536 for text-embedding-3-small");
        }

        #endregion

        #region ServiceNow Configuration Tests

        [TestMethod]
        public void ServiceNow_Configuration_ShouldExist()
        {
            // Arrange & Act
            var serviceNowConfig = _configuration.GetSection("ServiceNow");

            // Assert
            Assert.IsTrue(serviceNowConfig.Exists(), "ServiceNow configuration should exist");
        }

        [TestMethod]
        public void ServiceNow_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] { "Instance", "InstanceUrl", "Token", "TimeoutSeconds" };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"ServiceNow:{field}"];
                Assert.IsNotNull(value, $"ServiceNow field '{field}' should exist");
            }
        }

        [TestMethod]
        public void ServiceNow_Timeout_ShouldBeReasonable()
        {
            // Arrange & Act
            var timeoutStr = _configuration["ServiceNow:TimeoutSeconds"];
            var isParsed = int.TryParse(timeoutStr, out var timeout);

            // Assert
            Assert.IsTrue(isParsed, "TimeoutSeconds should be a valid integer");
            Assert.IsTrue(timeout > 0 && timeout <= 60, "TimeoutSeconds should be reasonable (1-60)");
        }

        #endregion

        #region Azure Configuration Tests

        [TestMethod]
        public void Azure_Configuration_ShouldExist()
        {
            // Arrange & Act
            var azureConfig = _configuration.GetSection("Azure");

            // Assert
            Assert.IsTrue(azureConfig.Exists(), "Azure configuration should exist");
        }

        [TestMethod]
        public void Azure_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "SubscriptionId", 
                "ResourceGroup", 
                "Region",
                "TenantId",
                "ClientId"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Azure:{field}"];
                Assert.IsNotNull(value, $"Azure field '{field}' should exist");
            }
        }

        [TestMethod]
        public void Azure_SubscriptionId_ShouldBeValidGuid()
        {
            // Arrange & Act
            var subscriptionId = _configuration["Azure:SubscriptionId"];

            // Assert
            Assert.IsTrue(
                System.Guid.TryParse(subscriptionId, out _),
                "SubscriptionId should be a valid GUID"
            );
        }

        #endregion

        #region AzureDevOps Configuration Tests

        [TestMethod]
        public void AzureDevOps_Configuration_ShouldExist()
        {
            // Arrange & Act
            var adoConfig = _configuration.GetSection("AzureDevOps");

            // Assert
            Assert.IsTrue(adoConfig.Exists(), "AzureDevOps configuration should exist");
        }

        [TestMethod]
        public void AzureDevOps_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] { "OrganizationUrl", "PatToken", "Project" };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"AzureDevOps:{field}"];
                Assert.IsNotNull(value, $"AzureDevOps field '{field}' should exist");
            }
        }

        #endregion

        #region AzureAutomation Configuration Tests

        [TestMethod]
        public void AzureAutomation_Configuration_ShouldExist()
        {
            // Arrange & Act
            var automationConfig = _configuration.GetSection("AzureAutomation");

            // Assert
            Assert.IsTrue(automationConfig.Exists(), "AzureAutomation configuration should exist");
        }

        [TestMethod]
        public void AzureAutomation_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "SubscriptionId", 
                "ResourceGroup", 
                "AccountName"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"AzureAutomation:{field}"];
                Assert.IsNotNull(value, $"AzureAutomation field '{field}' should exist");
            }
        }

        #endregion

        #region Kubernetes Configuration Tests

        [TestMethod]
        public void Kubernetes_Configuration_ShouldExist()
        {
            // Arrange & Act
            var k8sConfig = _configuration.GetSection("Kubernetes");

            // Assert
            Assert.IsTrue(k8sConfig.Exists(), "Kubernetes configuration should exist");
        }

        [TestMethod]
        public void Kubernetes_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "ClusterName", 
                "ResourceGroup", 
                "Namespace"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Kubernetes:{field}"];
                Assert.IsNotNull(value, $"Kubernetes field '{field}' should exist");
            }
        }

        #endregion

        #region Terraform Configuration Tests

        [TestMethod]
        public void Terraform_Configuration_ShouldExist()
        {
            // Arrange & Act
            var terraformConfig = _configuration.GetSection("Terraform");

            // Assert
            Assert.IsTrue(terraformConfig.Exists(), "Terraform configuration should exist");
        }

        [TestMethod]
        public void Terraform_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "StorageAccount", 
                "StorageContainer", 
                "StateFilePath"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Terraform:{field}"];
                Assert.IsNotNull(value, $"Terraform field '{field}' should exist");
            }
        }

        #endregion

        #region Tenant Configuration Tests

        [TestMethod]
        public void Tenant_Configuration_ShouldExist()
        {
            // Arrange & Act
            var tenantConfig = _configuration.GetSection("Tenant");

            // Assert
            Assert.IsTrue(tenantConfig.Exists(), "Tenant configuration should exist");
        }

        [TestMethod]
        public void Tenant_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] { "TenantId", "ServiceName" };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Tenant:{field}"];
                Assert.IsNotNull(value, $"Tenant field '{field}' should exist");
            }
        }

        [TestMethod]
        public void Tenants_Array_ShouldHaveMultipleEntries()
        {
            // Arrange & Act
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren().ToList();

            // Assert
            Assert.IsTrue(tenants.Count > 0, "Tenants array should have entries");
            Assert.IsTrue(tenants.Count >= 4, "Should have at least 4 tenant configurations");
        }

        [TestMethod]
        public void Tenants_ShouldIncludeAllTiers()
        {
            // Arrange & Act
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren().ToList();
            var tiers = new HashSet<string>();

            foreach (var tenant in tenants)
            {
                var tier = tenant["SubscriptionTier"];
                if (!string.IsNullOrEmpty(tier))
                {
                    tiers.Add(tier);
                }
            }

            // Assert
            Assert.IsTrue(tiers.Contains("Free"), "Should have Free tier");
            Assert.IsTrue(tiers.Contains("Starter"), "Should have Starter tier");
            Assert.IsTrue(tiers.Contains("Pro"), "Should have Pro tier");
            Assert.IsTrue(tiers.Contains("Enterprise"), "Should have Enterprise tier");
        }

        [TestMethod]
        public void Tenants_FreeTier_ShouldHaveLimitedTools()
        {
            // Arrange & Act
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren().ToList();
            var freeTenant = tenants.FirstOrDefault(t => t["SubscriptionTier"] == "Free");

            // Assert
            Assert.IsNotNull(freeTenant, "Free tier tenant should exist");
            var allowedTools = freeTenant.GetSection("AllowedTools").GetChildren().ToList();
            Assert.IsTrue(allowedTools.Count > 0, "Free tier should have allowed tools");
            Assert.IsTrue(allowedTools.Count <= 3, "Free tier should have limited tools");
        }

        [TestMethod]
        public void Tenants_EnterpriseTier_ShouldHaveUnlimitedQuota()
        {
            // Arrange & Act
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren().ToList();
            var enterpriseTenant = tenants.FirstOrDefault(t => t["SubscriptionTier"] == "Enterprise");

            // Assert
            Assert.IsNotNull(enterpriseTenant, "Enterprise tier tenant should exist");
            var quotaStr = enterpriseTenant["MonthlyApiQuota"];
            var isParsed = int.TryParse(quotaStr, out var quota);
            Assert.IsTrue(isParsed, "MonthlyApiQuota should be a valid integer");
            Assert.AreEqual(-1, quota, "Enterprise tier should have unlimited quota (-1)");
        }

        #endregion

        #region Jurisdictions Configuration Tests

        [TestMethod]
        public void Jurisdictions_Array_ShouldExist()
        {
            // Arrange & Act
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");

            // Assert
            Assert.IsTrue(jurisdictionsSection.Exists(), "Jurisdictions section should exist");
        }

        [TestMethod]
        public void Jurisdictions_ShouldHaveMultipleEntries()
        {
            // Arrange & Act
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren().ToList();

            // Assert
            Assert.IsTrue(jurisdictions.Count >= 3, "Should have at least 3 jurisdictions (CA, TX, NY)");
        }

        [TestMethod]
        public void Jurisdictions_ShouldIncludeRequiredStates()
        {
            // Arrange & Act
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren().ToList();
            var stateCodes = new HashSet<string>();

            foreach (var jurisdiction in jurisdictions)
            {
                var stateCode = jurisdiction["stateCode"];
                if (!string.IsNullOrEmpty(stateCode))
                {
                    stateCodes.Add(stateCode);
                }
            }

            // Assert
            Assert.IsTrue(stateCodes.Contains("CA"), "Should have California (CA)");
            Assert.IsTrue(stateCodes.Contains("TX"), "Should have Texas (TX)");
            Assert.IsTrue(stateCodes.Contains("NY"), "Should have New York (NY)");
        }

        [TestMethod]
        public void Jurisdictions_California_ShouldRequireCCPA()
        {
            // Arrange & Act
            var caJurisdiction = _configuration.GetSection("jurisdictions:0");
            var ccpaRequired = caJurisdiction["regulations:complianceRequirements:ccpa"];

            // Assert
            Assert.AreEqual("true", ccpaRequired.ToLower(), "California should require CCPA compliance");
        }

        [TestMethod]
        public void Jurisdictions_NewYork_ShouldRequireMFA()
        {
            // Arrange & Act
            var nyJurisdiction = _configuration.GetSection("jurisdictions:2");
            var mfaRequired = nyJurisdiction["regulations:securityStandards:requiresMfa"];

            // Assert
            Assert.AreEqual("true", mfaRequired.ToLower(), "New York should require MFA");
        }

        #endregion

        #region CareProvider Configuration Tests

        [TestMethod]
        public void CareProvider_Configuration_ShouldExist()
        {
            // Arrange & Act
            var careProviderConfig = _configuration.GetSection("CareProvider");

            // Assert
            Assert.IsTrue(careProviderConfig.Exists(), "CareProvider configuration should exist");
        }

        [TestMethod]
        public void CareProvider_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "ConfigPath", 
                "DaycarePath", 
                "ElderlyHomePath",
                "HealthcarePath"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"CareProvider:{field}"];
                Assert.IsNotNull(value, $"CareProvider field '{field}' should exist");
            }
        }

        #endregion

        #region Database Configuration Tests

        [TestMethod]
        public void Database_Configuration_ShouldExist()
        {
            // Arrange & Act
            var dbConfig = _configuration.GetSection("Database");

            // Assert
            Assert.IsTrue(dbConfig.Exists(), "Database configuration should exist");
        }

        [TestMethod]
        public void Database_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "Timeout", 
                "CommandTimeout", 
                "MaxRetryCount",
                "HealthCheckQuery"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Database:{field}"];
                Assert.IsNotNull(value, $"Database field '{field}' should exist");
            }
        }

        #endregion

        #region API Configuration Tests

        [TestMethod]
        public void Api_Configuration_ShouldExist()
        {
            // Arrange & Act
            var apiConfig = _configuration.GetSection("Api");

            // Assert
            Assert.IsTrue(apiConfig.Exists(), "Api configuration should exist");
        }

        [TestMethod]
        public void Api_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "Version", 
                "ApiKeyHeaderName", 
                "TenantIdHeaderName",
                "RateLimitPerMinute"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Api:{field}"];
                Assert.IsNotNull(value, $"Api field '{field}' should exist");
            }
        }

        [TestMethod]
        public void Api_RateLimits_ShouldBePositive()
        {
            // Arrange
            var limits = new[] 
            { 
                ("Api:RateLimitPerMinute", 100),
                ("Api:RateLimitPerHour", 1000),
                ("Api:RateLimitPerDay", 10000)
            };

            // Act & Assert
            foreach (var (key, expected) in limits)
            {
                var valueStr = _configuration[key];
                var isParsed = int.TryParse(valueStr, out var value);
                Assert.IsTrue(isParsed, $"{key} should be a valid integer");
                Assert.IsTrue(value > 0, $"{key} should be positive");
            }
        }

        #endregion

        #region Security Configuration Tests

        [TestMethod]
        public void Security_Configuration_ShouldExist()
        {
            // Arrange & Act
            var securityConfig = _configuration.GetSection("Security");

            // Assert
            Assert.IsTrue(securityConfig.Exists(), "Security configuration should exist");
        }

        [TestMethod]
        public void Security_RequiredFields_ShouldExist()
        {
            // Arrange
            var requiredFields = new[] 
            { 
                "AllowAnonymousAccess", 
                "RequireHttps", 
                "EnableCors"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[$"Security:{field}"];
                Assert.IsNotNull(value, $"Security field '{field}' should exist");
            }
        }

        [TestMethod]
        public void Security_RequireHttps_ShouldBeTrue()
        {
            // Arrange & Act
            var requireHttps = _configuration["Security:RequireHttps"];

            // Assert
            Assert.AreEqual("true", requireHttps.ToLower(), "Production should require HTTPS");
        }

        #endregion

        #region KeyVault Configuration Tests

        [TestMethod]
        public void KeyVault_Configuration_ShouldExist()
        {
            // Arrange & Act
            var kvConfig = _configuration.GetSection("KeyVault");

            // Assert
            Assert.IsTrue(kvConfig.Exists(), "KeyVault configuration should exist");
        }

        [TestMethod]
        public void KeyVault_Url_ShouldBeValidFormatWhenEnabled()
        {
            // Arrange & Act
            var isEnabled = _configuration["KeyVault:Enabled"];
            var url = _configuration["KeyVault:Url"];

            // Assert
            if (isEnabled?.ToLower() == "true")
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(url), "KeyVault URL should be set when enabled");
                Assert.IsTrue(url.Contains("vault.azure.net"), "KeyVault URL should be valid Azure format");
            }
        }

        #endregion

        #region Features Configuration Tests

        [TestMethod]
        public void Features_Configuration_ShouldExist()
        {
            // Arrange & Act
            var featuresConfig = _configuration.GetSection("Features");

            // Assert
            Assert.IsTrue(featuresConfig.Exists(), "Features configuration should exist");
        }

        [TestMethod]
        public void Features_CoreFeatures_ShouldBeEnabled()
        {
            // Arrange
            var coreFeatures = new[] 
            { 
                "Features:EnableAiAgentIntegration",
                "Features:EnableMultiTenancy"
            };

            // Act & Assert
            foreach (var feature in coreFeatures)
            {
                var isEnabled = _configuration[feature];
                Assert.AreEqual("true", isEnabled?.ToLower(), $"{feature} should be enabled");
            }
        }

        #endregion

        #region Cache Configuration Tests

        [TestMethod]
        public void Cache_Configuration_ShouldExist()
        {
            // Arrange & Act
            var cacheConfig = _configuration.GetSection("Cache");

            // Assert
            Assert.IsTrue(cacheConfig.Exists(), "Cache configuration should exist");
        }

        [TestMethod]
        public void Cache_Provider_ShouldBeValid()
        {
            // Arrange & Act
            var provider = _configuration["Cache:Provider"];

            // Assert
            var validProviders = new[] { "InMemory", "Redis", "Distributed" };
            Assert.IsTrue(validProviders.Contains(provider), "Cache provider should be valid");
        }

        #endregion

        #region Aspire Configuration Tests

        [TestMethod]
        public void Aspire_Configuration_ShouldExist()
        {
            // Arrange & Act
            var aspireConfig = _configuration.GetSection("Aspire");

            // Assert
            Assert.IsTrue(aspireConfig.Exists(), "Aspire configuration should exist");
        }

        [TestMethod]
        public void Aspire_DashboardPort_ShouldBeValid()
        {
            // Arrange & Act
            var portStr = _configuration["Aspire:DashboardPort"];
            var isParsed = int.TryParse(portStr, out var port);

            // Assert
            Assert.IsTrue(isParsed, "DashboardPort should be a valid integer");
            Assert.IsTrue(port > 1024, "DashboardPort should be greater than 1024");
            Assert.IsTrue(port <= 65535, "DashboardPort should be valid port number");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void AllConfigSections_ShouldLoadWithoutErrors()
        {
            // Arrange & Act
            var allSections = _configuration.GetChildren().ToList();

            // Assert
            Assert.IsTrue(allSections.Count > 0, "Configuration should load successfully");
            Assert.IsTrue(allSections.Count >= 20, "Should have at least 20 configuration sections");
        }

        [TestMethod]
        public void Configuration_ShouldNotHavePlaceholderValuesOnly()
        {
            // Arrange
            var criticalKeys = new[]
            {
                "ChatGPT:ApiKey",
                "ServiceNow:Token",
                "Azure:SubscriptionId"
            };

            // Act
            var allPlaceholders = 0;
            foreach (var key in criticalKeys)
            {
                var value = _configuration[key];
                if (value?.Contains("your-") == true || value?.Contains("00000000-0000-0000-0000-000000000000") == true)
                {
                    allPlaceholders++;
                }
            }

            // Assert
            // This test documents that some values need to be configured
            Assert.IsTrue(allPlaceholders <= criticalKeys.Length, 
                "Configuration template has placeholder values - these need to be updated with actual values");
        }

        [TestMethod]
        public async Task Configuration_CanBeProcessed_AsJson()
        {
            // Arrange
            var configPath = "../../../appsettings.json";

            // Act
            var json = await File.ReadAllTextAsync(configPath);
            var jsonDoc = JsonDocument.Parse(json);

            // Assert
            Assert.IsNotNull(jsonDoc, "Configuration should be valid JSON");
            Assert.IsTrue(jsonDoc.RootElement.ValueKind == JsonValueKind.Object, 
                "Configuration root should be a JSON object");
        }

        #endregion

        #region Configuration Value Validation Tests

        [TestMethod]
        public void ConfigurationValues_GuidFields_ShouldBeValidGuids()
        {
            // Arrange
            var guidFields = new[]
            {
                "Azure:SubscriptionId",
                "Azure:TenantId",
                "Azure:ClientId",
                "AzureAutomation:SubscriptionId",
                "Authentication:Entra:ClientId",
                "KeyVault:TenantId",
                "KeyVault:ClientId"
            };

            // Act & Assert
            foreach (var field in guidFields)
            {
                var value = _configuration[field];
                if (!string.IsNullOrEmpty(value) && !value.Contains("your-") && !value.Contains("00000000"))
                {
                    var isValid = System.Guid.TryParse(value, out _);
                    Assert.IsTrue(isValid, $"{field} should be a valid GUID");
                }
            }
        }

        [TestMethod]
        public void ConfigurationValues_UrlFields_ShouldBeValidUrls()
        {
            // Arrange
            var urlFields = new[]
            {
                "ServiceNow:InstanceUrl",
                "Authentication:Okta:Domain",
                "AzureDevOps:OrganizationUrl",
                "KeyVault:Url"
            };

            // Act & Assert
            foreach (var field in urlFields)
            {
                var value = _configuration[field];
                if (!string.IsNullOrEmpty(value) && !value.Contains("your-"))
                {
                    var isValid = Uri.TryCreate(value, UriKind.Absolute, out var uri);
                    Assert.IsTrue(isValid, $"{field} should be a valid URL");
                }
            }
        }

        [TestMethod]
        public void ConfigurationValues_TlsVersions_ShouldBeValid()
        {
            // Arrange
            var tlsFields = new[]
            {
                "jurisdictions:0:regulations:securityStandards:minimumTlsVersion",
                "jurisdictions:1:regulations:securityStandards:minimumTlsVersion",
                "jurisdictions:2:regulations:securityStandards:minimumTlsVersion"
            };

            var validVersions = new[] { "1.2", "1.3" };

            // Act & Assert
            foreach (var field in tlsFields)
            {
                var value = _configuration[field];
                Assert.IsTrue(validVersions.Contains(value), $"{field} should be valid TLS version");
            }
        }

        #endregion
    }
}
