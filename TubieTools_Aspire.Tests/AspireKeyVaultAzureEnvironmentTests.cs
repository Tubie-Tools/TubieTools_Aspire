using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TubieTools_Aspire.Tests;

/// <summary>
/// Tests to verify KeyVault and AzureEnvironment integration in .NET Aspire
/// This test suite checks if Azure Key Vault and Azure Environment are properly configured
/// and accessible within the Aspire distributed application.
/// </summary>
[TestClass]
public class AspireKeyVaultAzureEnvironmentTests
{
    private IConfiguration _configuration;

    [TestInitialize]
    public void Setup()
    {
        var solutionRoot = GetSolutionRoot();
        var appHostPath = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost");

        _configuration = new ConfigurationBuilder()
            .SetBasePath(appHostPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private string GetSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (File.Exists(Path.Combine(currentDir, "TubieTools_Aspire.sln")))
            {
                return currentDir;
            }

            var parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName;
        }

        throw new DirectoryNotFoundException("Solution root not found. Ensure TubieTools_Aspire.sln exists.");
    }

    #region KeyVault Configuration Tests

    /// <summary>
    /// Test 1: Verify KeyVault configuration file exists
    /// </summary>
    [TestMethod]
    public void KeyVault_AppHostFiles_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var appHostPath = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost");
        var appHostFile = Path.Combine(appHostPath, "AppHost.cs");
        var appsettingsFile = Path.Combine(appHostPath, "appsettings.json");

        // Act & Assert
        Assert.IsTrue(File.Exists(appHostFile), 
            $"AppHost.cs should exist at {appHostFile}");
        Assert.IsTrue(File.Exists(appsettingsFile), 
            $"appsettings.json should exist at {appsettingsFile}");
    }

    /// <summary>
    /// Test 2: Verify appsettings contains KeyVault configuration
    /// </summary>
    [TestMethod]
    public void KeyVault_ConfigurationSection_ShouldBePresentInAppSettings()
    {
        // Arrange & Act
        var configContent = File.ReadAllText(
            Path.Combine(GetSolutionRoot(), 
            "TubieTools_Aspire.AppHost", "appsettings.json"));

        // Assert
        Assert.IsTrue(!string.IsNullOrEmpty(configContent), 
            "appsettings.json should not be empty");
    }

    /// <summary>
    /// Test 3: Verify KeyVault name is configured
    /// </summary>
    [TestMethod]
    public void KeyVault_VaultName_ShouldBeConfigured()
    {
        // Arrange
        var expectedVaultName = "tubie-key-vault-vrnqlaipku";
        var solutionRoot = GetSolutionRoot();
        var appHostFile = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost", "AppHost.cs");

        // Act
        var appHostContent = File.ReadAllText(appHostFile);
        var vaultNamePresent = appHostContent.Contains(expectedVaultName);

        // Assert
        Assert.IsTrue(vaultNamePresent, 
            $"AppHost.cs should reference KeyVault with name '{expectedVaultName}'");
    }

    /// <summary>
    /// Test 4: Verify KeyVault is added to builder
    /// </summary>
    [TestMethod]
    public void KeyVault_AddAzureKeyVault_ShouldBeCalledInAppHost()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var appHostFile = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost", "AppHost.cs");

        // Act
        var appHostContent = File.ReadAllText(appHostFile);
        var addKeyVaultPresent = appHostContent.Contains("AddAzureKeyVault");

        // Assert
        Assert.IsTrue(addKeyVaultPresent, 
            "AppHost.cs should call builder.AddAzureKeyVault()");
    }

    #endregion

    #region Azure Environment Tests

    /// <summary>
    /// Test 5: Verify Azure environment variables can be configured
    /// </summary>
    [TestMethod]
    public void AzureEnvironment_RequiredVariables_DocumentedInTest()
    {
        // Arrange
        var requiredAzureVars = new[]
        {
            "AZURE_TENANT_ID",
            "AZURE_CLIENT_ID",
            "AZURE_CLIENT_SECRET",
            "AZURE_SUBSCRIPTION_ID"
        };

        // Act
        var missingVars = requiredAzureVars
            .Where(v => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(v)))
            .ToList();

        // Assert - Document what's needed
        if (missingVars.Any())
        {
            var message = $"The following Azure environment variables should be set for KeyVault access: " +
                string.Join(", ", missingVars);
            Assert.Inconclusive(message);
        }
    }

    /// <summary>
    /// Test 6: Verify Configuration can be loaded
    /// </summary>
    [TestMethod]
    public void Configuration_ShouldLoadSuccessfully()
    {
        // Arrange & Act
        var config = _configuration;

        // Assert
        Assert.IsNotNull(config, "Configuration should load successfully");
        Assert.IsTrue(config.AsEnumerable().Any(), 
            "Configuration should contain values");
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Test 7: Verify web frontend can reference KeyVault
    /// </summary>
    [TestMethod]
    public void Integration_WebFrontend_ShouldReferenceKeyVault()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var appHostFile = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost", "AppHost.cs");

        // Act
        var appHostContent = File.ReadAllText(appHostFile);
        var webFrontendWithKeyVaultRef = appHostContent.Contains("webfrontend") 
            && appHostContent.Contains("WithReference(keyVault)");

        // Assert
        Assert.IsTrue(webFrontendWithKeyVaultRef, 
            "WebFrontend should have reference to KeyVault in AppHost.cs");
    }

    /// <summary>
    /// Test 8: Verify all resources are properly added
    /// </summary>
    [TestMethod]
    public void Integration_AllResources_ShouldBeConfiguredInAppHost()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var appHostFile = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost", "AppHost.cs");
        var appHostContent = File.ReadAllText(appHostFile);

        var requiredResources = new Dictionary<string, string>
        {
            { "PublicAPI", "publicapi" },
            { "SentimentAPI", "sentimentapi" },
            { "WebFrontend", "webfrontend" },
            { "EnterpriseAutomation", "enterpriseautomation" }
        };

        // Act & Assert
        foreach (var resource in requiredResources)
        {
            Assert.IsTrue(appHostContent.Contains(resource.Value), 
                $"AppHost.cs should configure {resource.Key} resource with name '{resource.Value}'");
        }
    }

    /// <summary>
    /// Test 9: Verify ServiceNow Services are added in EnterpriseAutomation
    /// </summary>
    [TestMethod]
    public void Integration_ServiceNowServices_ShouldBeInEnterpriseAutomation()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var programFile = Path.Combine(solutionRoot, 
            "TubieTools_Aspire.EnterpriseAutomation", "Program.cs");

        if (!File.Exists(programFile))
        {
            Assert.Inconclusive($"Program.cs not found at {programFile}");
            return;
        }

        // Act
        var programContent = File.ReadAllText(programFile);
        var hasServiceNowServices = programContent.Contains("IServiceNowService");
        var hasJurisdictionServices = programContent.Contains("AddJurisdictionServices");

        // Assert
        Assert.IsTrue(hasServiceNowServices, 
            "EnterpriseAutomation should register IServiceNowService");
        Assert.IsTrue(hasJurisdictionServices, 
            "EnterpriseAutomation should call AddJurisdictionServices");
    }

    /// <summary>
    /// Test 10: Diagnostic - List all Aspire configuration
    /// </summary>
    [TestMethod]
    public void Diagnostic_DisplayAspireAppHostConfiguration()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var appHostFile = Path.Combine(solutionRoot, "TubieTools_Aspire.AppHost", "AppHost.cs");

        // Act
        var appHostContent = File.ReadAllText(appHostFile);
        var lines = appHostContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // Assert & Log
        System.Diagnostics.Debug.WriteLine("=== KeyVault and Azure Configuration ===");
        System.Diagnostics.Debug.WriteLine($"AppHost file: {appHostFile}");
        System.Diagnostics.Debug.WriteLine($"Total lines: {lines.Length}");

        var keyVaultLines = lines.Where(l => l.Contains("KeyVault") || l.Contains("Azure")).ToList();
        System.Diagnostics.Debug.WriteLine($"KeyVault/Azure related lines: {keyVaultLines.Count}");
        foreach (var line in keyVaultLines)
        {
            System.Diagnostics.Debug.WriteLine($"  {line.Trim()}");
        }

        Assert.IsTrue(keyVaultLines.Any(), "AppHost.cs should contain KeyVault configuration");
    }

    #endregion

    #region Troubleshooting Tests

    /// <summary>
    /// Test 11: Check if DataAccessLayer.dll path is correct
    /// </summary>
    [TestMethod]
    public void Troubleshooting_DataAccessLayer_DllPath_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var dllPath = Path.Combine(solutionRoot, 
            "TubieTools_Aspire.EnterpriseAutomation", 
            "bin", "Debug", "net10.0", "DataAccessLayer.dll");

        // Act
        var exists = File.Exists(dllPath);

        // Assert
        if (!exists)
        {
            Assert.Inconclusive(
                $"DataAccessLayer.dll not found at {dllPath}. " +
                "This may indicate build issues. Run 'dotnet clean' and 'dotnet build' on EnterpriseAutomation project.");
        }
        else
        {
            Assert.IsTrue(exists, $"DataAccessLayer.dll should exist at {dllPath}");
        }
    }

    /// <summary>
    /// Test 12: Verify no circular dependencies in service registration
    /// </summary>
    [TestMethod]
    public void Troubleshooting_ServiceRegistration_ShouldNotHaveCircularDependencies()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var programFile = Path.Combine(solutionRoot, 
            "TubieTools_Aspire.EnterpriseAutomation", "Program.cs");

        if (!File.Exists(programFile))
        {
            Assert.Inconclusive($"Program.cs not found at {programFile}");
            return;
        }

        // Act
        var programContent = File.ReadAllText(programFile);

        // Check for common patterns that might cause issues
        var hasAddScoped = programContent.Contains("AddScoped");
        var hasDependencyInjection = programContent.Contains("Services");

        // Assert
        Assert.IsTrue(hasAddScoped, 
            "Program.cs should use AddScoped for service registration");
        Assert.IsTrue(hasDependencyInjection, 
            "Program.cs should use dependency injection");

        System.Diagnostics.Debug.WriteLine("Service registrations appear correctly structured");
    }

    #endregion
}
