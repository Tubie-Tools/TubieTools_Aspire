# Test Execution Guide

**This is your real proof.** Tests validate NuGet packages, API surfaces, and connectivity.

---

## 📋 Quick Start

### Run All Tests
```bash
cd TubieTools_CopilotStudio_API.Tests
dotnet test
```

**Expected Output**:
```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 27, Skipped: 0, Duration: X.XXs
```

---

## 🧪 Test Categories

### Unit Tests (Mocked Dependencies)
**Location**: `Unit/Controllers/` and `Unit/Services/`  
**Purpose**: Validate business logic without database  
**Runs in**: ~2 seconds  

```bash
dotnet test --filter "TestCategory=Unit"
```

**What they prove**:
- ✅ Controller methods compile and accept correct parameters
- ✅ Service methods execute without exceptions
- ✅ DTO mapping works correctly
- ✅ Null handling is proper

### Integration Tests (Real Database)
**Location**: `Integration/Database/`  
**Purpose**: Validate DbContext and repository operations  
**Runs in**: ~5-10 seconds  

```bash
dotnet test --filter "TestCategory=Integration"
```

**What they prove**:
- ✅ EF Core DbContext initializes correctly
- ✅ In-memory database schema matches model
- ✅ CRUD operations work end-to-end
- ✅ Filtering and querying functions properly

### External Connectivity Tests
**Location**: `Integration/External/`  
**Purpose**: Validate assembly loading and configuration  
**Runs in**: ~2-3 seconds  

```bash
dotnet test --filter "ClassName=ExternalConnectivityTests"
```

**What they prove**:
- ✅ All NuGet packages are correctly installed
- ✅ .NET 10.0 framework is accessible
- ✅ All API types are loadable
- ✅ Dependency injection can be configured
- ✅ Models from EnterpriseAutomation are accessible

---

## 📊 Test Breakdown

### Unit Tests (16 tests)

#### CopilotApplicationsControllerTests (9 tests)
```
✓ GetAll returns list when applications exist
✓ GetAll returns empty list when no applications exist
✓ GetById returns application when found
✓ GetById returns null when application not found
✓ GetByLandingZone returns filtered applications
✓ Create returns created application with DTO
✓ Update modifies application
✓ Delete removes application
✓ Create with null request handles gracefully
```

#### CopilotApplicationServiceTests (7 tests)
```
✓ CreateAsync persists entity and returns DTO
✓ GetByIdAsync returns DTO when entity found
✓ GetByIdAsync returns null when entity not found
✓ GetAllAsync returns list of DTOs
✓ GetAllAsync returns empty list when no applications exist
✓ GetByLandingZoneAsync filters results
✓ UpdateAsync modifies and saves entity
✓ DeleteAsync removes entity
```

### Integration Tests (8 tests)

#### CopilotApplicationRepositoryIntegrationTests (8 tests)
```
✓ AddAsync persists entity to database
✓ GetByIdAsync retrieves entity from database
✓ GetByIdAsync returns null for nonexistent entity
✓ UpdateAsync modifies persisted entity
✓ DeleteAsync removes entity from database
✓ GetAllAsync retrieves all entities
✓ GetByLandingZoneAsync filters by landing zone
✓ GetActiveAsync returns only active applications
```

### External Connectivity Tests (11 tests)

```
✓ NuGet required assemblies are loaded
✓ .NET Framework version is net10.0
✓ API types are accessible
✓ DbContext can be instantiated
✓ All repository interfaces are implemented
✓ Service interfaces are properly defined
✓ DTOs can be instantiated
✓ Configuration can be loaded
✓ Dependency injection container can be configured
✓ EF Core migrations infrastructure exists
✓ HTTP client factory is available
✓ All Models are accessible
```

---

## ✅ Verification Checklist

Before committing, run this sequence:

### Step 1: Build the Test Project
```bash
cd TubieTools_CopilotStudio_API.Tests
dotnet build
```
**Expected**: `Build succeeded`

### Step 2: Run Unit Tests
```bash
dotnet test --filter "ClassName=CopilotApplicationsControllerTests"
dotnet test --filter "ClassName=CopilotApplicationServiceTests"
```
**Expected**: `Passed: 16 | Failed: 0`

### Step 3: Run Integration Tests (Database)
```bash
dotnet test --filter "ClassName=CopilotApplicationRepositoryIntegrationTests"
```
**Expected**: `Passed: 8 | Failed: 0`

### Step 4: Run Connectivity Tests
```bash
dotnet test --filter "ClassName=ExternalConnectivityTests"
```
**Expected**: `Passed: 11 | Failed: 0`

### Step 5: Run All Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```
**Expected**: 
- `Passed: 35 | Failed: 0`
- Coverage: > 80%

---

## 🔍 Interpreting Test Results

### If All Tests Pass ✅
You have machine-verified proof:
- Package dependencies are correctly installed
- Code compiles without errors
- Business logic executes properly
- Database operations work
- All types are loadable
- Configuration is valid

**Safe to commit.**

### If a Unit Test Fails ❌
Example: `CopilotApplicationsControllerTests.GetAll_WhenApplicationsExist_ReturnsList FAILED`

**Likely causes**:
- Service interface changed signature
- DTO properties missing
- Business logic error

**Action**: Review error message and fix code.

### If an Integration Test Fails ❌
Example: `CopilotApplicationRepositoryIntegrationTests.AddAsync_PersistsEntity_ToDatabase FAILED`

**Likely causes**:
- DbContext mapping incorrect
- Entity property type mismatch
- Database constraint violation

**Action**: Check DbContext configuration and entity definitions.

### If a Connectivity Test Fails ❌
Example: `ExternalConnectivityTests.NuGet_RequiredAssemblies_AreLoaded FAILED`

**Likely causes**:
- Package not restored
- Package version incompatible
- Missing assembly reference

**Action**: Run `dotnet restore` and check `.csproj` for version issues.

---

## 🚀 CI/CD Integration

### GitHub Actions Example
```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  test:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v2
	  - uses: actions/setup-dotnet@v1
		with:
		  dotnet-version: 10.0.x

	  - name: Restore
		run: dotnet restore TubieTools_CopilotStudio_API.Tests/TubieTools_CopilotStudio_API.Tests.csproj

	  - name: Build
		run: dotnet build TubieTools_CopilotStudio_API.Tests/TubieTools_CopilotStudio_API.Tests.csproj

	  - name: Test
		run: dotnet test TubieTools_CopilotStudio_API.Tests/TubieTools_CopilotStudio_API.Tests.csproj --no-build --logger "trx" --collect:"XPlat Code Coverage"

	  - name: Upload Coverage
		uses: codecov/codecov-action@v2
```

---

## ⚠️ Test Failures Aren't Negative

If a test fails:
- It caught a real problem **before production**
- You know exactly what's broken (test name + assertion)
- You can fix it immediately
- You have proof it's fixed when test passes

**This is what you want.**

---

## 📝 Adding New Tests

When generating new code, tests are created automatically.

To add your own test:

1. Create file in appropriate folder:
   - `Unit/Controllers/` for controller tests
   - `Unit/Services/` for service tests
   - `Integration/Database/` for data tests
   - `Integration/External/` for connectivity tests

2. Use AAA pattern:
   ```csharp
   [TestMethod]
   [Description("What this test validates")]
   public async Task Method_Scenario_ExpectedResult()
   {
	   // ARRANGE

	   // ACT

	   // ASSERT
   }
   ```

3. Run tests:
   ```bash
   dotnet test
   ```

---

## 🎯 This Is Your Protection

- ✅ Tests prove code works before PR
- ✅ Tests catch NuGet package issues
- ✅ Tests validate database operations
- ✅ Tests document expected behavior
- ✅ Tests prevent regressions

**No PR without passing tests.**
