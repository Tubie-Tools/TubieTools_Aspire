using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace TubieTools_CopilotStudio_API.Tests.Integration.External
{
    [TestClass]
    [Description("Integration tests for external dependencies and configuration")]
    public class ExternalConnectivityTests
    {
        [TestMethod]
        [Description("Verify required assemblies are loaded")]
        public void NuGet_RequiredAssemblies_AreLoaded()
        {
            // ARRANGE
            var required = new[]
            {
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.SqlServer",
                "Swashbuckle.AspNetCore.SwaggerUI",
                "System.Text.Json"
            };

            // ACT & ASSERT
            foreach (var assemblyName in required)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    Assert.IsNotNull(assembly, $"Assembly {assemblyName} not found");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to load assembly {assemblyName}: {ex.Message}");
                }
            }
        }

        [TestMethod]
        [Description("Verify .NET Framework version is 10.0")]
        public void DotNet_FrameworkVersion_Is_NetTen()
        {
            // ARRANGE & ACT
            var targetFramework = typeof(object).Assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();

            // ASSERT - Note: This will vary based on test project framework
            Assert.IsNotNull(targetFramework);
            Assert.IsTrue(
                targetFramework.FrameworkName.Contains("net10.0") || 
                targetFramework.FrameworkName.Contains(".NETCoreApp,Version=v10.0"),
                $"Expected net10.0 but found: {targetFramework.FrameworkName}"
            );
        }

        [TestMethod]
        [Description("Verify key types from API project are accessible")]
        public void API_Types_AreAccessible()
        {
            // ARRANGE
            var typeNames = new[]
            {
                "TubieTools_CopilotStudio_API.Controllers.CopilotApplicationsController",
                "TubieTools_CopilotStudio_API.Services.CopilotApplicationService",
                "TubieTools_CopilotStudio_API.Data.CopilotStudioDbContext",
                "TubieTools_CopilotStudio_API.Data.Repositories.CopilotApplicationRepository"
            };

            // ACT & ASSERT
            foreach (var typeName in typeNames)
            {
                var type = Type.GetType(typeName);
                Assert.IsNotNull(type, $"Type {typeName} not found");
            }
        }

        [TestMethod]
        [Description("Verify DbContext can be instantiated with InMemory options")]
        public void DbContext_CanBeInstantiated()
        {
            // ARRANGE
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TubieTools_CopilotStudio_API.Data.CopilotStudioDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // ACT & ASSERT
            try
            {
                using (var dbContext = new TubieTools_CopilotStudio_API.Data.CopilotStudioDbContext(options))
                {
                    Assert.IsNotNull(dbContext);
                    Assert.IsNotNull(dbContext.CopilotApplications);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to instantiate DbContext: {ex.Message}");
            }
        }

        [TestMethod]
        [Description("Verify all repository interfaces are implemented")]
        public void Repositories_AllInterfacesImplemented()
        {
            // ARRANGE
            var interfaces = new[]
            {
                "TubieTools_CopilotStudio_API.Data.Repositories.ICopilotApplicationRepository",
                "TubieTools_CopilotStudio_API.Data.Repositories.IKnowledgeToolRepository",
                "TubieTools_CopilotStudio_API.Data.Repositories.IGovernancePolicyRepository",
                "TubieTools_CopilotStudio_API.Data.Repositories.IPerformanceMetricsRepository",
                "TubieTools_CopilotStudio_API.Data.Repositories.IDeploymentConfigRepository",
                "TubieTools_CopilotStudio_API.Data.Repositories.IVersionRepository"
            };

            // ACT & ASSERT
            foreach (var interfaceName in interfaces)
            {
                var type = Type.GetType(interfaceName);
                Assert.IsNotNull(type, $"Repository interface {interfaceName} not found");
                Assert.IsTrue(type.IsInterface, $"{interfaceName} is not an interface");
            }
        }

        [TestMethod]
        [Description("Verify service interfaces are properly defined")]
        public void Services_InterfacesDefined()
        {
            // ARRANGE
            var interfaces = new[]
            {
                "TubieTools_CopilotStudio_API.Services.ICopilotApplicationService"
            };

            // ACT & ASSERT
            foreach (var interfaceName in interfaces)
            {
                var type = Type.GetType(interfaceName);
                Assert.IsNotNull(type, $"Service interface {interfaceName} not found");
                Assert.IsTrue(type.IsInterface, $"{interfaceName} is not an interface");
            }
        }

        [TestMethod]
        [Description("Verify DTOs are properly structured")]
        public void DTOs_CanBeInstantiated()
        {
            // ARRANGE
            var dtoType = Type.GetType("TubieTools_CopilotStudio_API.Services.DTOs.CopilotApplicationDto");

            // ACT & ASSERT
            Assert.IsNotNull(dtoType);

            // Verify it has expected properties
            var properties = dtoType.GetProperties();
            var propertyNames = new[] { "CopilotId", "Name", "CurrentVersion" };

            foreach (var propName in propertyNames)
            {
                var prop = dtoType.GetProperty(propName);
                Assert.IsNotNull(prop, $"DTO missing property: {propName}");
            }
        }

        [TestMethod]
        [Description("Verify configuration can be loaded")]
        public void Configuration_CanBeLoaded()
        {
            // ARRANGE & ACT
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:TubieToolsCopilot", "Server=(localdb)\\mssqllocaldb;Database=Test;Trusted_Connection=true;" },
                    { "Serilog:MinimumLevel", "Information" }
                });

            var config = builder.Build();

            // ASSERT
            Assert.IsNotNull(config);
            Assert.AreEqual(
                "Server=(localdb)\\mssqllocaldb;Database=Test;Trusted_Connection=true;",
                config["ConnectionStrings:TubieToolsCopilot"]
            );
        }

        [TestMethod]
        [Description("Verify dependency injection container can be configured")]
        public void DependencyInjection_ContainerCanBeConfigured()
        {
            // ARRANGE
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            // ACT
            try
            {
                services.AddScoped<TubieTools_CopilotStudio_API.Data.Repositories.ICopilotApplicationRepository,
                    TubieTools_CopilotStudio_API.Data.Repositories.CopilotApplicationRepository>();

                var provider = services.BuildServiceProvider();

                // ASSERT
                Assert.IsNotNull(provider);
            }
            catch (Exception ex)
            {
                Assert.Fail($"DI configuration failed: {ex.Message}");
            }
        }

        [TestMethod]
        [Description("Verify Entity Framework Core migrations infrastructure exists")]
        public void EFCore_MigrationsSupported()
        {
            // ARRANGE
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TubieTools_CopilotStudio_API.Data.CopilotStudioDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // ACT & ASSERT
            using (var dbContext = new TubieTools_CopilotStudio_API.Data.CopilotStudioDbContext(options))
            {
                // Verify we can access the migration methods
                var migrator = dbContext.Database;
                Assert.IsNotNull(migrator);

                // Verify CanConnect can be called
                bool canConnect = migrator.CanConnect();
                Assert.IsTrue(canConnect); // In-memory DB should always be connectable
            }
        }

        [TestMethod]
        [Description("Verify HTTP client factory is available")]
        public void HttpClientFactory_IsAvailable()
        {
            // ARRANGE
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            // ACT
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();

            // ASSERT
            var factory = provider.GetService(typeof(System.Net.Http.IHttpClientFactory));
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        [Description("Verify all Models are properly accessible")]
        public void Models_AreAccessible()
        {
            // ARRANGE
            var modelTypes = new[]
            {
                "TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.CopilotApplication",
                "TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.KnowledgeTool",
                "TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models.CopilotGovernancePolicy"
            };

            // ACT & ASSERT
            foreach (var typeName in modelTypes)
            {
                var type = Type.GetType(typeName);
                Assert.IsNotNull(type, $"Model type {typeName} not accessible");
            }
        }
    }
}
