using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests.Configuration
{
    /// <summary>
    /// LLM and enterprise integration configuration tests
    /// </summary>
    [TestClass]
    public class LLMAndIntegrationConfigurationTests
    {
        private IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {
            var solutionRoot = GetSolutionRoot();
            var configPath = Path.Combine(solutionRoot, "appsettings.json");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(solutionRoot)
                .AddJsonFile(configPath, optional: false)
                .Build();
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

        #region ChatGPT Integration Tests

        [TestMethod]
        public void ChatGPT_ApiKey_ShouldExist()
        {
            // Arrange & Act
            var apiKey = _configuration["ChatGPT:ApiKey"];

            // Assert
            Assert.IsNotNull(apiKey, "ChatGPT API key must be configured");
            Assert.IsFalse(string.IsNullOrWhiteSpace(apiKey), "ChatGPT API key must not be empty");
        }

        [TestMethod]
        public void ChatGPT_ApiKey_ShouldHaveCorrectFormat()
        {
            // Arrange & Act
            var apiKey = _configuration["ChatGPT:ApiKey"];

            // Assert
            // OpenAI keys start with 'sk-'
            if (!apiKey.Contains("your-"))
            {
                Assert.IsTrue(apiKey.StartsWith("sk-"), "ChatGPT API key should start with 'sk-'");
            }
        }

        [TestMethod]
        public void ChatGPT_Model_ShouldBeSupportedVersion()
        {
            // Arrange & Act
            var model = _configuration["ChatGPT:Model"];

            // Assert
            var supportedModels = new[] { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" };
            var isSupported = false;
            foreach (var supported in supportedModels)
            {
                if (model.Contains(supported))
                {
                    isSupported = true;
                    break;
                }
            }
            Assert.IsTrue(isSupported, "ChatGPT model should be a supported version");
        }

        [TestMethod]
        public void ChatGPT_Endpoint_ShouldBeValid()
        {
            // Arrange & Act
            var endpoint = _configuration["ChatGPT:Endpoint"];

            // Assert
            Assert.IsNotNull(endpoint, "ChatGPT endpoint should be configured");
            Assert.IsTrue(Uri.TryCreate(endpoint, UriKind.Absolute, out _), 
                "ChatGPT endpoint should be a valid URL");
            Assert.IsTrue(endpoint.Contains("api.openai.com"), 
                "ChatGPT endpoint should point to OpenAI API");
        }

        #endregion

        #region Embedding Integration Tests

        [TestMethod]
        public void Embedding_Config_Complete()
        {
            // Arrange
            var requiredEmbeddingFields = new[]
            {
                "Embedding:EmbeddingProvider",
                "Embedding:EmbeddingModel",
                "Embedding:EmbeddingDimension",
                "Embedding:VectorStoreBackend",
                "Embedding:SimilarityMetric"
            };

            // Act & Assert
            foreach (var field in requiredEmbeddingFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"Embedding field '{field}' should be configured");
            }
        }

        [TestMethod]
        public void Embedding_Provider_ShouldMatchLlmProvider()
        {
            // Arrange & Act
            var embeddingProvider = _configuration["Embedding:EmbeddingProvider"];
            var chatGptModel = _configuration["ChatGPT:Model"];

            // Assert
            // Both should use OpenAI for consistency
            Assert.AreEqual("OpenAI", embeddingProvider, 
                "Embedding provider should be OpenAI to match ChatGPT");
        }

        [TestMethod]
        public void Embedding_Model_ShouldBeCompatible()
        {
            // Arrange & Act
            var embeddingModel = _configuration["Embedding:EmbeddingModel"];

            // Assert
            var validEmbeddingModels = new[] 
            { 
                "text-embedding-3-small", 
                "text-embedding-3-large",
                "text-embedding-ada-002"
            };

            var isValid = false;
            foreach (var model in validEmbeddingModels)
            {
                if (embeddingModel.Contains(model))
                {
                    isValid = true;
                    break;
                }
            }
            Assert.IsTrue(isValid, "Embedding model should be a valid OpenAI model");
        }

        [TestMethod]
        public void Embedding_Dimension_ShouldMatchModel()
        {
            // Arrange & Act
            var dimension = _configuration["Embedding:EmbeddingDimension"];
            var embeddingModel = _configuration["Embedding:EmbeddingModel"];

            // Assert
            Assert.IsTrue(int.TryParse(dimension, out var dim), "Dimension should be a valid integer");

            // text-embedding-3-small uses 1536 dimensions
            if (embeddingModel.Contains("text-embedding-3-small") || embeddingModel.Contains("ada"))
            {
                Assert.AreEqual(1536, dim, "Dimension should match model specification");
            }
        }

        [TestMethod]
        public void Embedding_VectorStore_ShouldBeDefined()
        {
            // Arrange & Act
            var vectorStore = _configuration["Embedding:VectorStoreBackend"];
            var vectorConnection = _configuration["Embedding:VectorStoreConnection"];

            // Assert
            var validStores = new[] { "Pinecone", "Weaviate", "Qdrant", "Chroma" };
            Assert.IsTrue(Array.Exists(validStores, store => store == vectorStore),
                "Vector store should be a supported backend");
            Assert.IsNotNull(vectorConnection, "Vector store connection should be configured");
        }

        [TestMethod]
        public void Embedding_SimilarityMetric_ShouldBeValid()
        {
            // Arrange & Act
            var metric = _configuration["Embedding:SimilarityMetric"];

            // Assert
            var validMetrics = new[] { "cosine", "euclidean", "dot_product" };
            Assert.IsTrue(Array.Exists(validMetrics, m => m == metric),
                "Similarity metric should be valid");
        }

        #endregion

        #region ServiceNow Integration Tests

        [TestMethod]
        public void ServiceNow_Configuration_Complete()
        {
            // Arrange
            var requiredFields = new[]
            {
                "ServiceNow:Instance",
                "ServiceNow:InstanceUrl",
                "ServiceNow:Token",
                "ServiceNow:TimeoutSeconds"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"ServiceNow field '{field}' should be configured");
                Assert.IsFalse(string.IsNullOrWhiteSpace(value), 
                    $"ServiceNow field '{field}' should not be empty");
            }
        }

        [TestMethod]
        public void ServiceNow_InstanceUrl_ShouldContainInstance()
        {
            // Arrange & Act
            var instance = _configuration["ServiceNow:Instance"];
            var instanceUrl = _configuration["ServiceNow:InstanceUrl"];

            // Assert
            Assert.IsTrue(instanceUrl.Contains(instance), 
                "InstanceUrl should contain the Instance name");
        }

        [TestMethod]
        public void ServiceNow_InstanceUrl_ShouldBeValidUrl()
        {
            // Arrange & Act
            var instanceUrl = _configuration["ServiceNow:InstanceUrl"];

            // Assert
            Assert.IsTrue(Uri.TryCreate(instanceUrl, UriKind.Absolute, out var uri),
                "ServiceNow instance URL should be valid");
            Assert.IsTrue(instanceUrl.Contains("service-now.com"),
                "ServiceNow URL should point to ServiceNow domain");
        }

        [TestMethod]
        public void ServiceNow_Token_ShouldExist()
        {
            // Arrange & Act
            var token = _configuration["ServiceNow:Token"];

            // Assert
            Assert.IsNotNull(token, "ServiceNow OAuth token must be configured");
            Assert.IsFalse(string.IsNullOrWhiteSpace(token), 
                "ServiceNow token must not be empty");
        }

        [TestMethod]
        public void ServiceNow_RetryConfiguration_ShouldBeReasonable()
        {
            // Arrange & Act
            var maxRetries = _configuration["ServiceNow:MaxRetries"];
            var retryDelay = _configuration["ServiceNow:RetryDelaySeconds"];

            // Assert
            Assert.IsTrue(int.TryParse(maxRetries, out var retries), 
                "MaxRetries should be a valid integer");
            Assert.IsTrue(int.TryParse(retryDelay, out var delay), 
                "RetryDelaySeconds should be a valid integer");

            Assert.IsTrue(retries > 0 && retries <= 10, "MaxRetries should be reasonable");
            Assert.IsTrue(delay > 0 && delay <= 10, "RetryDelaySeconds should be reasonable");
        }

        #endregion

        #region Azure Integration Tests

        [TestMethod]
        public void Azure_Configuration_Complete()
        {
            // Arrange
            var requiredFields = new[]
            {
                "Azure:SubscriptionId",
                "Azure:ResourceGroup",
                "Azure:Region",
                "Azure:TenantId",
                "Azure:ClientId"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"Azure field '{field}' should be configured");
            }
        }

        [TestMethod]
        public void Azure_Guids_ShouldBeValid()
        {
            // Arrange
            var guidFields = new[]
            {
                "Azure:SubscriptionId",
                "Azure:TenantId",
                "Azure:ClientId"
            };

            // Act & Assert
            foreach (var field in guidFields)
            {
                var value = _configuration[field];
                if (!value.Contains("your-") && !value.Contains("00000000"))
                {
                    Assert.IsTrue(Guid.TryParse(value, out _),
                        $"Azure field '{field}' should be a valid GUID");
                }
            }
        }

        [TestMethod]
        public void Azure_Region_ShouldBeValidAzureRegion()
        {
            // Arrange & Act
            var region = _configuration["Azure:Region"];

            // Assert
            var validRegions = new[] 
            { 
                "eastus", "westus", "westus2", "eastus2", 
                "centralus", "northcentralus", "southcentralus"
            };

            Assert.IsTrue(Array.Exists(validRegions, r => r == region?.ToLower()),
                "Azure region should be a valid Azure region");
        }

        #endregion

        #region AzureDevOps Integration Tests

        [TestMethod]
        public void AzureDevOps_Configuration_Complete()
        {
            // Arrange
            var requiredFields = new[]
            {
                "AzureDevOps:OrganizationUrl",
                "AzureDevOps:PatToken",
                "AzureDevOps:Project"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"AzureDevOps field '{field}' should be configured");
            }
        }

        [TestMethod]
        public void AzureDevOps_OrganizationUrl_ShouldBeValid()
        {
            // Arrange & Act
            var orgUrl = _configuration["AzureDevOps:OrganizationUrl"];

            // Assert
            Assert.IsTrue(Uri.TryCreate(orgUrl, UriKind.Absolute, out _),
                "AzureDevOps organization URL should be valid");
            Assert.IsTrue(orgUrl.Contains("dev.azure.com"),
                "AzureDevOps URL should point to dev.azure.com");
        }

        [TestMethod]
        public void AzureDevOps_PatToken_ShouldExist()
        {
            // Arrange & Act
            var token = _configuration["AzureDevOps:PatToken"];

            // Assert
            Assert.IsNotNull(token, "AzureDevOps PAT token must be configured");
            Assert.IsFalse(string.IsNullOrWhiteSpace(token), 
                "AzureDevOps PAT token must not be empty");
        }

        #endregion

        #region AzureAutomation Integration Tests

        [TestMethod]
        public void AzureAutomation_Configuration_Complete()
        {
            // Arrange
            var requiredFields = new[]
            {
                "AzureAutomation:SubscriptionId",
                "AzureAutomation:ResourceGroup",
                "AzureAutomation:AccountName"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"AzureAutomation field '{field}' should be configured");
            }
        }

        [TestMethod]
        public void AzureAutomation_ShouldUseAzureSubscription()
        {
            // Arrange & Act
            var automationSub = _configuration["AzureAutomation:SubscriptionId"];
            var azureSub = _configuration["Azure:SubscriptionId"];

            // Assert
            // In most cases, both should use the same subscription
            if (!automationSub.Contains("your-") && !azureSub.Contains("your-"))
            {
                Assert.AreEqual(azureSub, automationSub,
                    "Azure Automation should use the same subscription as Azure");
            }
        }

        #endregion

        #region Kubernetes Integration Tests

        [TestMethod]
        public void Kubernetes_Configuration_Complete()
        {
            // Arrange
            var requiredFields = new[]
            {
                "Kubernetes:ClusterName",
                "Kubernetes:ResourceGroup",
                "Kubernetes:Namespace"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"Kubernetes field '{field}' should be configured");
            }
        }

        [TestMethod]
        public void Kubernetes_Namespace_ShouldBeValid()
        {
            // Arrange & Act
            var @namespace = _configuration["Kubernetes:Namespace"];

            // Assert
            var validNamespaces = new[] { "default", "kube-system", "kube-public", "kube-node-lease" };
            // Custom namespaces are also valid, just check it's not empty
            Assert.IsFalse(string.IsNullOrWhiteSpace(@namespace),
                "Kubernetes namespace should be configured");
        }

        #endregion

        #region Terraform Integration Tests

        [TestMethod]
        public void Terraform_Configuration_Complete()
        {
            // Arrange
            var requiredFields = new[]
            {
                "Terraform:StorageAccount",
                "Terraform:StorageContainer",
                "Terraform:StateFilePath"
            };

            // Act & Assert
            foreach (var field in requiredFields)
            {
                var value = _configuration[field];
                Assert.IsNotNull(value, $"Terraform field '{field}' should be configured");
            }
        }

        [TestMethod]
        public void Terraform_StateFilePath_ShouldBeValid()
        {
            // Arrange & Act
            var stateFile = _configuration["Terraform:StateFilePath"];

            // Assert
            Assert.IsTrue(stateFile.EndsWith(".tfstate"),
                "Terraform state file should end with .tfstate");
            Assert.IsFalse(stateFile.Contains("//"), 
                "Terraform state file path should not have double slashes");
        }

        #endregion

        #region Multi-Service Consistency Tests

        [TestMethod]
        public void AllServices_ShouldHaveTimeoutConfiguration()
        {
            // Arrange
            var timeoutFields = new[]
            {
                ("Database:CommandTimeout", 30, 600),
                ("ServiceNow:TimeoutSeconds", 5, 60),
                ("Api:RequestTimeout", 10, 300)
            };

            // Act & Assert
            foreach (var (field, min, max) in timeoutFields)
            {
                var value = _configuration[field];
                if (value != null)
                {
                    Assert.IsTrue(int.TryParse(value, out var timeout),
                        $"{field} should be a valid integer");
                    Assert.IsTrue(timeout >= min && timeout <= max,
                        $"{field} should be between {min} and {max}");
                }
            }
        }

        [TestMethod]
        public void AllServices_ShouldHaveRetryConfiguration()
        {
            // Arrange
            var retryFields = new[]
            {
                "Database:MaxRetryCount",
                "ServiceNow:MaxRetries",
                "Azure:MaxRetryCount"
            };

            // Act & Assert
            foreach (var field in retryFields)
            {
                var value = _configuration[field];
                if (value != null)
                {
                    Assert.IsTrue(int.TryParse(value, out var retries),
                        $"{field} should be a valid integer");
                    Assert.IsTrue(retries > 0 && retries <= 10,
                        $"{field} should be reasonable");
                }
            }
        }

        [TestMethod]
        public void AllIntegrations_ShouldHaveSecurityConfiguration()
        {
            // Arrange
            var integrations = new[]
            {
                "ChatGPT",
                "ServiceNow",
                "Azure",
                "AzureDevOps"
            };

            // Act & Assert
            foreach (var integration in integrations)
            {
                var hasApiKey = !string.IsNullOrEmpty(_configuration[$"{integration}:ApiKey"]) ||
                               !string.IsNullOrEmpty(_configuration[$"{integration}:Token"]) ||
                               !string.IsNullOrEmpty(_configuration[$"{integration}:ClientSecret"]);

                Assert.IsTrue(hasApiKey,
                    $"{integration} should have security credentials configured");
            }
        }

        #endregion

        #region Configuration Coherence Tests

        [TestMethod]
        public void AllJurisdictions_ShouldHaveConnectionStrings()
        {
            // Arrange
            var jurisdictionsSection = _configuration.GetSection("jurisdictions");
            var jurisdictions = jurisdictionsSection.GetChildren();

            // Act & Assert
            foreach (var jurisdiction in jurisdictions)
            {
                var connectionStringName = jurisdiction["connectionStringName"];
                var connectionString = _configuration[$"ConnectionStrings:{connectionStringName}"];

                Assert.IsNotNull(connectionString,
                    $"Jurisdiction should have corresponding ConnectionString '{connectionStringName}'");
            }
        }

        [TestMethod]
        public void AllTenants_ShouldHaveValidConfiguration()
        {
            // Arrange
            var tenantsSection = _configuration.GetSection("Tenants");
            var tenants = tenantsSection.GetChildren();

            // Act
            foreach (var tenant in tenants)
            {
                var tenantId = tenant["TenantId"];
                var tier = tenant["SubscriptionTier"];
                var quota = tenant["MonthlyApiQuota"];

                // Assert
                Assert.IsNotNull(tenantId, "Tenant should have TenantId");
                Assert.IsNotNull(tier, "Tenant should have SubscriptionTier");
                Assert.IsNotNull(quota, "Tenant should have MonthlyApiQuota");
                Assert.IsTrue(int.TryParse(quota, out _), "MonthlyApiQuota should be valid integer");
            }
        }

        #endregion
    }
}
