# ⚠️ VERIFICATION CHECKLIST - RUN THIS BEFORE COMMITTING

**Do NOT commit this code without completing all steps in this checklist.**

This document provides TESTABLE verification steps YOU can run to ensure the build actually works.

---

## ✅ PHASE 1: Pre-Build Validation (5 minutes)

### Step 1.1: Verify .NET Version
```bash
dotnet --version
```
**Expected**: 10.0.x or higher  
**If NOT**: Stop here. You need .NET 10.0 installed.

### Step 1.2: Check Project File Syntax
```bash
cd TubieTools_CopilotStudio_API
cat TubieTools_CopilotStudio_API.csproj
```
**Verify these exact versions appear**:
- Swashbuckle.AspNetCore 6.10.0
- Microsoft.EntityFrameworkCore 9.0.0
- Microsoft.EntityFrameworkCore.SqlServer 9.0.0
- Microsoft.EntityFrameworkCore.Tools 9.0.0
- Serilog.AspNetCore 9.0.0

**If versions don't match**: STOP. Copy exact versions from this checklist.

### Step 1.3: Check File Completeness
```bash
ls -la Data/
ls -la Services/
ls -la Controllers/
```
**Required files**:
- ✅ Data/CopilotStudioDbContext.cs
- ✅ Data/Repositories/IRepositories.cs
- ✅ Data/Repositories/RepositoryImplementations.cs
- ✅ Services/CopilotApplicationService.cs
- ✅ Controllers/CopilotApplicationsController.cs
- ✅ Controllers/HealthController.cs
- ✅ Program.cs

**If ANY file missing**: STOP. Copy missing files from the specification.

---

## ✅ PHASE 2: Restore & Dependency Check (10 minutes)

### Step 2.1: Clean Previous Build
```bash
dotnet clean
rm -rf bin/
rm -rf obj/
```

### Step 2.2: Restore NuGet Packages
```bash
dotnet restore TubieTools_CopilotStudio_API.csproj
```
**Expected output**:
```
Determining projects to restore...
Restoring TubieTools_CopilotStudio_API.csproj ...
Looking for compatible restore assets: <list of packages>
Restore completed in X.XXs for TubieTools_CopilotStudio_API.csproj.
```

**If you see errors like**:
- `NU1100: Unable to resolve Swashbuckle.AspNetCore (>= 7.0.0)` → Update .csproj to 6.10.0
- `NU1100: Unable to resolve Microsoft.EntityFrameworkCore (>= 10.0.0)` → Update .csproj to 9.0.0
- Connection timeout → Check internet connection and NuGet source

**STOP if restore fails.**

### Step 2.3: Verify Package Versions Downloaded
```bash
ls ~/.nuget/packages/ | grep -i swashbuckle
ls ~/.nuget/packages/ | grep -i entityframework
ls ~/.nuget/packages/ | grep -i serilog
```

**Expected**: Each package folder contains version 6.10.0, 9.0.0, or 9.0.0 respectively

---

## ✅ PHASE 3: Compilation (5 minutes)

### Step 3.1: Full Build
```bash
dotnet build TubieTools_CopilotStudio_API.csproj -c Release
```

**Expected success output**:
```
Microsoft (R) Build Engine version 17.x.x
...
TubieTools_CopilotStudio_API -> .../bin/Release/net10.0/TubieTools_CopilotStudio_API.dll

Build succeeded.
	0 Warning(s)
	0 Error(s)

Time Elapsed 00:XX:XX
```

**⚠️ CRITICAL - If you see ANY of these errors, STOP and record them:**

1. **CS0246 (Type not found)**
   ```
   error CS0246: The type or namespace name 'X' could not be found
   ```
   → Missing using statement or type doesn't exist

2. **CS1061 (No member named)**
   ```
   error CS1061: 'Type' does not contain a definition for 'Member'
   ```
   → EF Core API mismatch, check DbContext configuration

3. **NU1100 (Package version doesn't exist)**
   ```
   error NU1100: Unable to resolve 'X (>= Y.Y.Y)'
   ```
   → Wrong package version in .csproj

4. **CS0019 (Operator cannot be applied)**
   ```
   error CS0019: Operator '+' cannot be applied to operands of type 'X' and 'Y'
   ```
   → Type mismatch in code

**If build fails**: Do NOT proceed. Document exact error and file name.

### Step 3.2: Verify DLL Created
```bash
ls -lh bin/Release/net10.0/TubieTools_CopilotStudio_API.dll
```
**Expected**: File exists and is > 100KB

---

## ✅ PHASE 4: EF Core Tools Verification (5 minutes)

### Step 4.1: Install EF Core CLI (if needed)
```bash
dotnet tool install --global dotnet-ef
```

### Step 4.2: Verify DbContext
```bash
dotnet ef dbcontext info
```
**Expected output**:
```
Project 'TubieTools_CopilotStudio_API' uses database provider 'Microsoft.EntityFrameworkCore.SqlServer' with connection string 'Server=(localdb)\mssqllocaldb;Database=CopilotStudioDb;Trusted_Connection=true;TrustServerCertificate=true;'.
```

**If you see errors**: 
- `Unable to find a compatible DbContext` → DbContext class not found
- Connection string error → appsettings.json not configured

---

## ✅ PHASE 5: Database Migration Validation (5 minutes)

### Step 5.1: Create Migration
```bash
dotnet ef migrations add InitialCreate
```
**Expected output**:
```
Building project...
To undo this action, use 'ef migrations remove'
```

**If you see errors**:
- `No DbContext named` → DbContext file structure wrong
- `Unable to connect to database` → SQL Server not running (can ignore for now)

### Step 5.2: Verify Migration File Created
```bash
ls -la Migrations/
```
**Expected**: 
- Migrations/ folder exists
- Files like `YYYYMMDDHHMMSS_InitialCreate.cs` present
- `CopilotStudioDbContextModelSnapshot.cs` exists

---

## ✅ PHASE 6: Runtime Validation (10 minutes)

### Step 6.1: Check SQL Server Availability
```bash
# Windows
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"

# Or check if service is running
```

**Expected**: SQL Server connection succeeds or clear error message

### Step 6.2: Apply Migration (if SQL Server ready)
```bash
dotnet ef database update
```
**Expected**:
```
Applying migration '20240101000000_InitialCreate'.
Done.
```

**If you see errors**:
- `Unable to connect to database` → SQL Server not running/configured
- `The metadata is not valid` → DbContext configuration error

### Step 6.3: Start Application
```bash
dotnet run
```
**Expected output**:
```
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:7265
info: Microsoft.Hosting.Lifetime[0]
	  Application started. Press Ctrl+C to exit.
```

**If application starts successfully**:
- Leave it running
- Open browser: `https://localhost:7265/swagger`
- Verify Swagger UI loads without errors

---

## ❌ TROUBLESHOOTING GUIDE

### Build Error: Package Not Found
```
error NU1100: Unable to resolve 'Swashbuckle.AspNetCore (>= 7.0.0)'
```
**Fix**:
1. Open `TubieTools_CopilotStudio_API.csproj`
2. Find the line with incorrect package
3. Change to: `<PackageReference Include="Swashbuckle.AspNetCore" Version="6.10.0" />`
4. Run `dotnet restore`

### Build Error: Type Not Found
```
error CS0246: The type or namespace name 'CopilotStudioDbContext' could not be found
```
**Fix**:
1. Verify file exists: `Data/CopilotStudioDbContext.cs`
2. Check namespace: Should be `namespace TubieTools_CopilotStudio_API.Data;`
3. Verify using statement: `using TubieTools_CopilotStudio_API.Data;`

### Build Error: Method Not Found
```
error CS1061: 'CopilotDeploymentConfig' does not contain a definition for 'HealthCheck'
```
**Fix**:
1. Open `CopilotApplicationModels.cs` in EnterpriseAutomation project
2. Verify the property name in CopilotDeploymentConfig class
3. Update DbContext to match actual property name

### Migration Error: DbContext Not Found
```
error : Could not execute because the application is not idle and has not been configured to handle requests outside of the web root.
```
**Fix**:
```bash
dotnet ef migrations add InitialCreate --project TubieTools_CopilotStudio_API
```

### Runtime Error: Cannot Connect to Database
```
Exception: A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```
**Fix**:
1. Verify SQL Server is running: `sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT 1"`
2. If LocalDB not installed, install from: `https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb`
3. Or change connection string to real SQL Server in `appsettings.json`

---

## 📊 SUCCESS CRITERIA

**Your build is ONLY successful if ALL these pass:**

| Step | Command | Expected | Status |
|------|---------|----------|--------|
| 1 | `dotnet --version` | 10.0+ | ✅ or ❌ |
| 2 | `dotnet restore` | Exit code 0 | ✅ or ❌ |
| 3 | `dotnet build -c Release` | 0 errors, 0 warnings | ✅ or ❌ |
| 4 | `dotnet ef dbcontext info` | Connects to DbContext | ✅ or ❌ |
| 5 | `dotnet ef migrations add InitialCreate` | Migration file created | ✅ or ❌ |
| 6 | `dotnet run` | App listens on port 7265 | ✅ or ❌ |
| 7 | `curl https://localhost:7265/swagger` (with -k) | Swagger UI loads | ✅ or ❌ |

**If ANY are ❌: DO NOT COMMIT. Document error and fix first.**

---

## 🚨 WHAT NOT TO DO

❌ Commit code with "I think it will compile"  
❌ Push to main without running the full build  
❌ Skip the migration test (it catches DbContext errors)  
❌ Assume "it compiled on my machine" = "it will compile everywhere"  

---

## ✅ WHAT TO DO NOW

1. **Run Phase 1 (5 min)** - Verify files exist and versions correct
2. **Run Phase 2 (10 min)** - Restore packages
3. **Run Phase 3 (5 min)** - Actually compile
4. **Run Phase 4-6 (15 min)** - Full validation

**Total time: ~35 minutes**

**If ALL phases pass**: You have confidence to commit  
**If ANY phase fails**: Document the exact error and we fix it before committing

---

## 📝 Documentation to Attach to PR

When you create the pull request, include:

```markdown
## Build Verification

This PR has been verified with:

- [x] Phase 1: Pre-Build Validation - PASSED
- [x] Phase 2: Restore & Dependency Check - PASSED
- [x] Phase 3: Compilation - PASSED
- [x] Phase 4: EF Core Tools - PASSED
- [x] Phase 5: Database Migration - PASSED
- [x] Phase 6: Runtime Validation - PASSED

Build output:
```
dotnet build -c Release
Build succeeded. 0 Warning(s) 0 Error(s)
```

Database migration:
```
dotnet ef migrations add InitialCreate
✅ Migration created: <timestamp>_InitialCreate.cs
```

Application startup:
```
dotnet run
✅ Now listening on: https://localhost:7265
✅ Swagger UI: https://localhost:7265/swagger
```
```

---

**DO NOT skip this checklist. Your job depends on it.**
