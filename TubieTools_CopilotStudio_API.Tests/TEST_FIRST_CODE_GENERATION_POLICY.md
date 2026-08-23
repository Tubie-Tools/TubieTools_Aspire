# TEST-FIRST CODE GENERATION POLICY

**Effective**: Immediately for all future code generation  
**Framework**: MSTest (AAA pattern)  
**Enforcement**: All generated code requires passing tests before PR submission  

---

## 🎯 CORE PRINCIPLE

**No code generation without corresponding tests.**

Tests are the machine-verified proof that:
- ✅ Package dependencies will restore
- ✅ API calls compile and execute
- ✅ Database operations work
- ✅ External service connectivity is valid
- ✅ Configuration is correct

---

## 📋 TEST GENERATION REQUIREMENTS

### For Every API Controller Generated
**Unit Tests Required**:
- ✅ Constructor dependency injection works
- ✅ Each endpoint returns correct status codes
- ✅ Each endpoint returns correct DTO structure
- ✅ Error handling (null checks, validation failures)
- ✅ Logging is called appropriately

**Integration Tests Required**:
- ✅ Database operations (CRUD via actual DbContext)
- ✅ Service layer returns correct data
- ✅ HTTP requests produce valid responses
- ✅ Migrations apply without errors

### For Every Service Generated
**Unit Tests Required**:
- ✅ Business logic with mocked repositories
- ✅ Null reference handling
- ✅ Collection empty/populated scenarios
- ✅ DTO mapping correctness
- ✅ Exception handling

**Integration Tests Required**:
- ✅ Real repository operations
- ✅ Database state changes
- ✅ Transaction rollback on errors
- ✅ Concurrent operation safety

### For Every Repository Generated
**Unit Tests Required**:
- ✅ Query construction correctness
- ✅ Filter application
- ✅ Sorting and pagination
- ✅ Exception wrapping

**Integration Tests Required**:
- ✅ Actual SQL Server operations
- ✅ INSERT/UPDATE/DELETE/SELECT verification
- ✅ Foreign key constraints
- ✅ Index performance
- ✅ Migration compatibility

### For Every DbContext Generated
**Unit Tests Required**:
- ✅ Entity validation rules
- ✅ Value object creation
- ✅ JSON serialization roundtrips

**Integration Tests Required**:
- ✅ Database schema matches model
- ✅ Migrations create correct tables/columns
- ✅ Constraints are enforced
- ✅ Queries generate efficient SQL
- ✅ Owned entities persist correctly

---

## 🧪 TEST PROJECT STRUCTURE

### Folder Organization:
```
TubieTools_CopilotStudio_API.Tests/
├── Unit/
│   ├── Controllers/
│   │   └── CopilotApplicationsControllerTests.cs
│   ├── Services/
│   │   └── CopilotApplicationServiceTests.cs
│   ├── Repositories/
│   │   └── CopilotApplicationRepositoryTests.cs
│   └── Data/
│       └── CopilotStudioDbContextTests.cs
│
└── Integration/
	├── Database/
	│   ├── DatabaseInitializationTests.cs
	│   ├── MigrationTests.cs
	│   └── CopilotApplicationRepositoryIntegrationTests.cs
	│
	├── API/
	│   ├── CopilotApplicationsEndpointTests.cs
	│   └── HealthCheckEndpointTests.cs
	│
	└── External/
		├── ExternalServiceConnectivityTests.cs
		├── PayPalIntegrationTests.cs
		└── CacheServiceConnectionTests.cs
```

---

## ✍️ TEST TEMPLATE - UNIT TESTS (AAA Pattern)

### Example: Controller Test

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TubieTools_CopilotStudio_API.Controllers;
using TubieTools_CopilotStudio_API.Services;
using TubieTools_CopilotStudio_API.Services.DTOs;

namespace TubieTools_CopilotStudio_API.Tests.Unit.Controllers
{
	[TestClass]
	public class CopilotApplicationsControllerTests
	{
		private Mock<ICopilotApplicationService> _mockService;
		private CopilotApplicationsController _controller;

		[TestInitialize]
		public void Setup()
		{
			// ARRANGE: Set up mocks and controller
			_mockService = new Mock<ICopilotApplicationService>();
			_controller = new CopilotApplicationsController(_mockService.Object);
		}

		[TestMethod]
		[Description("GetAll returns 200 OK with list of applications")]
		public async Task GetAll_WhenApplicationsExist_Returns200WithList()
		{
			// ARRANGE
			var applications = new List<CopilotApplicationDto>
			{
				new(CopilotId: "app-1", Name: "TestApp1", CurrentVersion: "1.0.0"),
				new(CopilotId: "app-2", Name: "TestApp2", CurrentVersion: "1.0.1")
			};
			_mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(applications);

			// ACT
			var result = await _controller.GetAll();

			// ASSERT
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("TestApp1", result[0].Name);
			_mockService.Verify(s => s.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		[Description("GetById returns 404 when application not found")]
		public async Task GetById_WhenApplicationNotFound_Returns404()
		{
			// ARRANGE
			string copilotId = "nonexistent-id";
			_mockService.Setup(s => s.GetByIdAsync(copilotId, It.IsAny<CancellationToken>()))
				.ReturnsAsync((CopilotApplicationDto)null);

			// ACT
			var result = await _controller.GetById(copilotId);

			// ASSERT
			Assert.IsNull(result);
			_mockService.Verify(s => s.GetByIdAsync(copilotId, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		[Description("Create returns 201 Created with new application")]
		public async Task Create_WithValidRequest_Returns201AndCreatedApplication()
		{
			// ARRANGE
			var request = new CreateCopilotApplicationRequest(
				Name: "NewApp",
				LandingZone: "Zone-A",
				Environment: "Dev"
			);
			var createdApp = new CopilotApplicationDto(
				CopilotId: "app-new",
				Name: "NewApp",
				CurrentVersion: "0.0.1"
			);
			_mockService.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
				.ReturnsAsync(createdApp);

			// ACT
			var result = await _controller.Create(request);

			// ASSERT
			Assert.IsNotNull(result);
			Assert.AreEqual("NewApp", result.Name);
			_mockService.Verify(s => s.CreateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		[Description("Update returns 200 OK with updated application")]
		public async Task Update_WithValidRequest_Returns200AndUpdatedApplication()
		{
			// ARRANGE
			string copilotId = "app-1";
			var request = new UpdateCopilotApplicationRequest(Name: "UpdatedName");
			var updatedApp = new CopilotApplicationDto(
				CopilotId: copilotId,
				Name: "UpdatedName",
				CurrentVersion: "1.0.1"
			);
			_mockService.Setup(s => s.UpdateAsync(copilotId, request, It.IsAny<CancellationToken>()))
				.ReturnsAsync(updatedApp);

			// ACT
			var result = await _controller.Update(copilotId, request);

			// ASSERT
			Assert.IsNotNull(result);
			Assert.AreEqual("UpdatedName", result.Name);
		}

		[TestMethod]
		[Description("Delete removes application and returns success")]
		public async Task Delete_WithValidId_Returns200AndDeletes()
		{
			// ARRANGE
			string copilotId = "app-1";
			_mockService.Setup(s => s.DeleteAsync(copilotId, It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			// ACT
			await _controller.Delete(copilotId);

			// ASSERT
			_mockService.Verify(s => s.DeleteAsync(copilotId, It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
```

---

## ✍️ TEST TEMPLATE - SERVICE TEST (AAA Pattern)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TubieTools_CopilotStudio_API.Services;
using TubieTools_CopilotStudio_API.Data.Repositories;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace TubieTools_CopilotStudio_API.Tests.Unit.Services
{
	[TestClass]
	public class CopilotApplicationServiceTests
	{
		private Mock<ICopilotApplicationRepository> _mockRepository;
		private CopilotApplicationService _service;

		[TestInitialize]
		public void Setup()
		{
			// ARRANGE
			_mockRepository = new Mock<ICopilotApplicationRepository>();
			_service = new CopilotApplicationService(_mockRepository.Object);
		}

		[TestMethod]
		[Description("CreateAsync with valid request persists and returns DTO")]
		public async Task CreateAsync_WithValidRequest_PersistsAndReturnsDto()
		{
			// ARRANGE
			var request = new CreateCopilotApplicationRequest(
				Name: "TestApp",
				LandingZone: "Zone-A",
				Environment: "Dev"
			);
			var entity = new CopilotApplication
			{
				CopilotId = "app-NEW",
				Name = "TestApp",
				LandingZone = "Zone-A",
				CurrentVersion = "0.0.1",
				CreatedDate = DateTime.UtcNow
			};
			_mockRepository.Setup(r => r.AddAsync(It.IsAny<CopilotApplication>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);
			_mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(1);

			// ACT
			var result = await _service.CreateAsync(request, CancellationToken.None);

			// ASSERT
			Assert.IsNotNull(result);
			Assert.AreEqual("TestApp", result.Name);
			_mockRepository.Verify(r => r.AddAsync(It.IsAny<CopilotApplication>(), It.IsAny<CancellationToken>()), Times.Once);
			_mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		[Description("GetByIdAsync returns null when not found")]
		public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
		{
			// ARRANGE
			string id = "nonexistent";
			_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
				.ReturnsAsync((CopilotApplication)null);

			// ACT
			var result = await _service.GetByIdAsync(id, CancellationToken.None);

			// ASSERT
			Assert.IsNull(result);
		}

		[TestMethod]
		[Description("GetAllAsync returns all applications")]
		public async Task GetAllAsync_Returns_AllApplications()
		{
			// ARRANGE
			var apps = new List<CopilotApplication>
			{
				new CopilotApplication { CopilotId = "1", Name = "App1", CurrentVersion = "1.0" },
				new CopilotApplication { CopilotId = "2", Name = "App2", CurrentVersion = "1.0" }
			};
			_mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(apps);

			// ACT
			var result = await _service.GetAllAsync(CancellationToken.None);

			// ASSERT
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("App1", result[0].Name);
		}

		[TestMethod]
		[Description("UpdateAsync modifies and persists entity")]
		public async Task UpdateAsync_WithValidRequest_ModifiesAndSaves()
		{
			// ARRANGE
			string id = "app-1";
			var existing = new CopilotApplication { CopilotId = id, Name = "OldName", CurrentVersion = "1.0" };
			var request = new UpdateCopilotApplicationRequest(Name: "NewName");

			_mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
				.ReturnsAsync(existing);
			_mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CopilotApplication>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);
			_mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(1);

			// ACT
			var result = await _service.UpdateAsync(id, request, CancellationToken.None);

			// ASSERT
			Assert.AreEqual("NewName", result.Name);
			_mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}

		[TestMethod]
		[Description("DeleteAsync removes entity from database")]
		public async Task DeleteAsync_RemovesEntity()
		{
			// ARRANGE
			string id = "app-1";
			_mockRepository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);
			_mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(1);

			// ACT
			await _service.DeleteAsync(id, CancellationToken.None);

			// ASSERT
			_mockRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
			_mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
```

---

## ✍️ TEST TEMPLATE - INTEGRATION TEST (Database)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using TubieTools_CopilotStudio_API.Data;
using TubieTools_CopilotStudio_API.Data.Repositories;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace TubieTools_CopilotStudio_API.Tests.Integration.Database
{
	[TestClass]
	public class CopilotApplicationRepositoryIntegrationTests
	{
		private CopilotStudioDbContext _dbContext;
		private CopilotApplicationRepository _repository;

		[TestInitialize]
		public async Task Setup()
		{
			// ARRANGE: Use in-memory database for testing
			var options = new DbContextOptionsBuilder<CopilotStudioDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_dbContext = new CopilotStudioDbContext(options);
			_repository = new CopilotApplicationRepository(_dbContext);

			// Apply migrations/schema
			await _dbContext.Database.EnsureCreatedAsync();
		}

		[TestCleanup]
		public async Task Cleanup()
		{
			// ASSERT cleanup
			await _dbContext.DisposeAsync();
		}

		[TestMethod]
		[Description("AddAsync persists entity to database")]
		public async Task AddAsync_PersistsEntity_ToDatabase()
		{
			// ARRANGE
			var app = new CopilotApplication
			{
				CopilotId = "test-app-1",
				Name = "TestApplication",
				LandingZone = "Zone-A",
				Environment = "Development",
				CreatedDate = DateTime.UtcNow,
				CurrentVersion = "1.0.0"
			};

			// ACT
			await _repository.AddAsync(app, CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ASSERT
			var persisted = await _repository.GetByIdAsync("test-app-1", CancellationToken.None);
			Assert.IsNotNull(persisted);
			Assert.AreEqual("TestApplication", persisted.Name);
		}

		[TestMethod]
		[Description("GetByIdAsync retrieves entity from database")]
		public async Task GetByIdAsync_RetrievesEntity_FromDatabase()
		{
			// ARRANGE
			var app = new CopilotApplication
			{
				CopilotId = "test-app-2",
				Name = "RetrievalTest",
				CurrentVersion = "1.0.0",
				CreatedDate = DateTime.UtcNow
			};
			await _repository.AddAsync(app, CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ACT
			var retrieved = await _repository.GetByIdAsync("test-app-2", CancellationToken.None);

			// ASSERT
			Assert.IsNotNull(retrieved);
			Assert.AreEqual("RetrievalTest", retrieved.Name);
		}

		[TestMethod]
		[Description("UpdateAsync modifies persisted entity")]
		public async Task UpdateAsync_ModifiesEntity_InDatabase()
		{
			// ARRANGE
			var app = new CopilotApplication
			{
				CopilotId = "test-app-3",
				Name = "OriginalName",
				CurrentVersion = "1.0.0",
				CreatedDate = DateTime.UtcNow
			};
			await _repository.AddAsync(app, CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ACT
			var existing = await _repository.GetByIdAsync("test-app-3", CancellationToken.None);
			existing.Name = "UpdatedName";
			await _repository.UpdateAsync(existing, CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ASSERT
			var updated = await _repository.GetByIdAsync("test-app-3", CancellationToken.None);
			Assert.AreEqual("UpdatedName", updated.Name);
		}

		[TestMethod]
		[Description("DeleteAsync removes entity from database")]
		public async Task DeleteAsync_RemovesEntity_FromDatabase()
		{
			// ARRANGE
			var app = new CopilotApplication
			{
				CopilotId = "test-app-4",
				Name = "ToDelete",
				CurrentVersion = "1.0.0",
				CreatedDate = DateTime.UtcNow
			};
			await _repository.AddAsync(app, CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ACT
			await _repository.DeleteAsync("test-app-4", CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ASSERT
			var deleted = await _repository.GetByIdAsync("test-app-4", CancellationToken.None);
			Assert.IsNull(deleted);
		}

		[TestMethod]
		[Description("GetByLandingZoneAsync filters by landing zone")]
		public async Task GetByLandingZoneAsync_FiltersBy_LandingZone()
		{
			// ARRANGE
			await _repository.AddAsync(new CopilotApplication 
			{ 
				CopilotId = "zone-a-1", Name = "App1", LandingZone = "Zone-A", 
				CurrentVersion = "1.0", CreatedDate = DateTime.UtcNow 
			}, CancellationToken.None);
			await _repository.AddAsync(new CopilotApplication 
			{ 
				CopilotId = "zone-b-1", Name = "App2", LandingZone = "Zone-B", 
				CurrentVersion = "1.0", CreatedDate = DateTime.UtcNow 
			}, CancellationToken.None);
			await _repository.SaveChangesAsync(CancellationToken.None);

			// ACT
			var zoneApps = await _repository.GetByLandingZoneAsync("Zone-A", CancellationToken.None);

			// ASSERT
			Assert.AreEqual(1, zoneApps.Count);
			Assert.AreEqual("Zone-A", zoneApps[0].LandingZone);
		}
	}
}
```

---

## ✍️ TEST TEMPLATE - EXTERNAL CONNECTIVITY TEST

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;

namespace TubieTools_CopilotStudio_API.Tests.Integration.External
{
	[TestClass]
	[Description("Tests connectivity to external services and verifies configuration")]
	public class ExternalServiceConnectivityTests
	{
		private readonly HttpClient _httpClient = new();

		[TestMethod]
		[Description("Verify SQL Server LocalDB is accessible")]
		public void SqlServer_IsAccessible()
		{
			// ARRANGE
			string connectionString = "Server=(localdb)\\mssqllocaldb;Database=TubieToolsCopilot;Trusted_Connection=true;";

			// ACT & ASSERT
			try
			{
				using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
				{
					connection.Open();
					Assert.IsTrue(connection.State == System.Data.ConnectionState.Open);
				}
			}
			catch (Exception ex)
			{
				Assert.Fail($"SQL Server not accessible: {ex.Message}");
			}
		}

		[TestMethod]
		[Description("Verify NuGet package restoration succeeds")]
		public void NuGet_PackagesRestored()
		{
			// ARRANGE & ACT & ASSERT: Check if key assemblies loaded
			var assemblyNames = new[]
			{
				"Microsoft.EntityFrameworkCore",
				"Microsoft.EntityFrameworkCore.SqlServer",
				"Swashbuckle.AspNetCore"
			};

			foreach (var name in assemblyNames)
			{
				var assembly = System.Reflection.Assembly.Load(name);
				Assert.IsNotNull(assembly, $"Package {name} not found");
			}
		}

		[TestMethod]
		[Description("Verify API starts and responds to health check")]
		[Ignore("Run manually after 'dotnet run'")]
		public async Task API_HealthCheck_Responds()
		{
			// ARRANGE
			string healthUrl = "https://localhost:7265/health";

			// ACT
			var response = await _httpClient.GetAsync(healthUrl);

			// ASSERT
			Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
		}

		[TestMethod]
		[Description("Verify Swagger endpoint is accessible")]
		[Ignore("Run manually after 'dotnet run'")]
		public async Task Swagger_IsAccessible()
		{
			// ARRANGE
			string swaggerUrl = "https://localhost:7265/swagger";

			// ACT
			var response = await _httpClient.GetAsync(swaggerUrl);

			// ASSERT
			Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
		}
	}
}
```

---

## 🚀 TEST EXECUTION CHECKLIST

Before generating any code:

- [ ] Write unit test first (TDD)
- [ ] Mock external dependencies
- [ ] Use AAA pattern (Arrange-Act-Assert)
- [ ] Add [Description] attributes to tests
- [ ] Ensure test names describe the behavior
- [ ] Run: `dotnet test` - all tests PASS
- [ ] Ensure coverage > 80%
- [ ] Create integration tests for data/external calls
- [ ] Run integration tests with real resources
- [ ] Document test execution output
- [ ] Attach test results to PR

---

## ✅ PROOF CRITERIA

**No PR will be accepted without**:

1. **Unit Tests Pass**
   ```bash
   dotnet test TubieTools_CopilotStudio_API.Tests.csproj
   ```
   Expected: `Passed: X | Failed: 0`

2. **Coverage Report**
   ```bash
   dotnet test /p:CollectCoverage=true
   ```
   Expected: Minimum 80% code coverage

3. **Integration Tests Pass** (with real DB)
   ```bash
   dotnet test --filter "Category=Integration"
   ```
   Expected: `Passed: X | Failed: 0`

4. **Build Succeeds**
   ```bash
   dotnet build -c Release
   ```
   Expected: `Build succeeded`

---

## 📊 FROM NOW ON

**When you request code generation, you will receive**:

✅ Production code  
✅ Unit tests (mocked dependencies)  
✅ Integration tests (real databases/APIs)  
✅ External connectivity tests  
✅ Test execution instructions  
✅ Expected test output  

**No more broken PRs.**  
**Tests are now the proof.**

---

**This is your job protection.**
