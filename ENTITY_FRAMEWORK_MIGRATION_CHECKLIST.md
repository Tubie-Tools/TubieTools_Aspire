# Entity Framework Refactoring - Project Migration Checklist

## Status Summary

| Project | Status | DbContext | Action Required |
|---------|--------|-----------|-----------------|
| TubieTools_CopilotStudio_API | ✅ **DONE** | CopilotStudioDbContext | Moved to DataAccessLayer ✓ |
| TubieTools_Map | ✅ **DONE** | MapAppDbContext | Moved to DataAccessLayer ✓ |
| TubieTools_Aspire.Web | ⏳ **PENDING** | ApplicationDbContext | Check Program.cs |
| TubieTools_PublicAPI | ✅ **DONE** | None (uses DataAccessLayer) | No action needed |
| TubieTools_Forecasting_API | ✅ **DONE** | None (ML-focused) | No action needed |
| MapApp.API (Backend) | ⏳ **PENDING** | MapAppDbContext | Check Program.cs |
| TubieTools_SentimentModel_* | ⏳ **PENDING** | Check for DbContext | Review |
| DataAccessLayer | ✅ **DONE** | Central repository | All contexts moved ✓ |
| DTOLayer | ✅ **DONE** | FacetMaps created | All facet maps created ✓ |

---

## Completed Tasks ✅

### 1. **Moved DbContexts to DataAccessLayer**
- ✅ `CopilotStudioDbContext` → `DataAccessLayer/Data/Contexts/CopilotStudioDbContext.cs`
- ✅ `MapAppDbContext` → `DataAccessLayer/Data/Contexts/MapAppDbContext.cs`

### 2. **Created Facet Maps in DTOLayer**

**CopilotStudio Domain** (7 facet maps):
- ✅ CopilotApplicationFacetMap
- ✅ CopilotModelConfigurationFacetMap
- ✅ KnowledgeToolFacetMap
- ✅ CopilotGovernancePolicyFacetMap
- ✅ CopilotPerformanceMetricsFacetMap
- ✅ CopilotDeploymentConfigFacetMap
- ✅ CopilotVersionFacetMap

**MapApp Domain** (4 facet maps):
- ✅ MapRouteFacetMap
- ✅ RouteSegmentFacetMap
- ✅ RouteRedirectionFacetMap
- ✅ AccountFacetMap

**DataAccess Domain** (3 facet maps):
- ✅ AddressFacetMap
- ✅ OrderFacetMap
- ✅ ProfileFacetMap

### 3. **Created Infrastructure**
- ✅ `IFacetMap.cs` - Generic interface
- ✅ `FacetMapRegistry.cs` - Central registry with extension methods
- ✅ `FACET_MAPPING_GUIDE.md` - Comprehensive usage guide

### 4. **Updated Project References**
- ✅ TubieTools_CopilotStudio_API.csproj → Added DataAccessLayer, DTOLayer
- ✅ TubieTools_CopilotStudio_API/Program.cs → Updated namespaces
- ✅ TubieTools_Map.csproj → Added DataAccessLayer, DTOLayer
- ✅ TubieTools_Map/Program.cs → Updated namespaces

### 5. **Cleaned Up Old Files**
- ✅ Deleted `TubieTools_CopilotStudio_API/Data/CopilotStudioDbContext.cs`
- ✅ Deleted `TubieTools_Map/Data/MapAppDbContext.cs`

---

## Remaining Tasks ⏳

### Phase 1: Verify Completion (This Build)
- [ ] Build solution and verify no deprecation warnings
- [ ] Run TubieTools_CopilotStudio_API and test endpoints
- [ ] Run TubieTools_Map and verify database initialization
- [ ] Verify all facet maps serialize/deserialize correctly

### Phase 2: Review Other DbContexts

#### TubieTools_Aspire.Web
**Status**: Using ApplicationDbContext (in DataAccessLayer)
**Check**:
```bash
grep -r "DbContext" TubieTools_Aspire.Web/
grep -r "AddDbContext" TubieTools_Aspire.Web/Program.cs
```
**Action**: 
- [ ] If using old local DbContext, move to DataAccessLayer
- [ ] Update Program.cs using statements
- [ ] Create facet maps if needed

#### MapApp/Backend/MapApp.API
**Status**: Has its own MapAppDbContext in `/Data/` folder
**Action**:
- [ ] Update to use DataAccessLayer version
- [ ] Update Program.cs
- [ ] Update using statements
- [ ] Test initialization

#### TubieTools_SentimentModel_* Projects
**Status**: Unknown
**Action**:
- [ ] Search for DbContext usage
- [ ] Document findings
- [ ] Apply refactoring if needed

### Phase 3: Create Missing Facet Maps
**For each DbSet in the following DbContexts:**
- [ ] ApplicationDbContext - Create facet maps for all entities
- [ ] FoundryDbContext - Review and create facet maps
- [ ] TenantDbContext - Review and create facet maps
- [ ] TubieDbContext - Review and create facet maps
- [ ] KitContext - Review and create facet maps

### Phase 4: API Contract Updates
- [ ] Update API controllers to return facet maps instead of entities
- [ ] Update Swagger/OpenAPI documentation
- [ ] Update API response models in client libraries
- [ ] Create API versioning strategy if needed

### Phase 5: Integration Testing
- [ ] End-to-end tests for each API endpoint
- [ ] Verify facet map conversion integrity
- [ ] Test error handling for NULL entities
- [ ] Performance test facet map serialization

---

## Quick Migration Template

Use this template for each project that needs refactoring:

### Step 1: Update .csproj
```xml
<ItemGroup>
  <ProjectReference Include="../DataAccessLayer/DataAccessLayer.csproj" />
  <ProjectReference Include="../DTOLayer/DTOLayer.csproj" />
  <!-- Keep existing references -->
</ItemGroup>
```

### Step 2: Update Program.cs
```csharp
// OLD:
using ProjectName.Data;

// NEW:
using DataAccessLayer.Data.Contexts;
using DTOLayer.FacetMaps.YourDomain;

// Keep the DbContext registration the same:
builder.Services.AddDbContext<YourDbContext>(options =>
	options.UseSqlServer(connectionString));
```

### Step 3: Update Repository/Service
```csharp
// OLD:
public YourEntity GetEntity(int id)
{
	return _context.YourEntities.Find(id);
}

// NEW:
public YourEntityFacetMap GetEntity(int id)
{
	var entity = _context.YourEntities.Find(id);
	return YourEntityFacetMap.FromEntity(entity);
}
```

### Step 4: Update Controller
```csharp
[HttpGet("{id}")]
public ActionResult<YourEntityFacetMap> Get(int id)
{
	var facet = _service.GetEntity(id);
	if (facet == null) return NotFound();
	return Ok(facet);
}
```

### Step 5: Delete Old DbContext
```bash
rm YourProject/Data/YourDbContext.cs
```

### Step 6: Build and Test
```bash
dotnet build
dotnet run
```

---

## Validation Checklist for Each Project

After migration, verify:

- [ ] **Compilation**: `dotnet build` succeeds with no warnings
- [ ] **Dependencies**: No circular dependencies between layers
- [ ] **Using Statements**: All EF namespaces from DataAccessLayer
- [ ] **API Responses**: Controllers return facet maps, not entities
- [ ] **Database Operations**: CRUD operations work correctly
- [ ] **Migrations**: `dotnet ef migrations add` works as expected
- [ ] **Serialization**: JSON responses deserialize correctly on client
- [ ] **Performance**: No N+1 queries or excessive mapping overhead
- [ ] **Error Handling**: NULL entities handled gracefully
- [ ] **Documentation**: Code comments updated for new architecture

---

## Project Dependencies After Refactoring

```
(API Projects)
	↓
	├── DataAccessLayer (DbContexts, Entities)
	├── DTOLayer (FacetMaps, DTOs)
	├── ServiceLayer (Business Logic)
	└── TubieTools_Aspire.ServiceDefaults (Common utilities)

(No Reverse Dependencies!)
	↑
DataAccessLayer, DTOLayer, ServiceLayer
	(these only depend on ModelLayer if needed)
```

---

## Build and Test Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build TubieTools_CopilotStudio_API/TubieTools_CopilotStudio_API.csproj

# Run unit tests
dotnet test

# Run specific API
cd TubieTools_CopilotStudio_API
dotnet run

# Check for EF issues
dotnet ef migrations check -p DataAccessLayer

# Add new migration
dotnet ef migrations add MigrationName -p DataAccessLayer
```

---

## Known Issues & Resolutions

### Issue 1: "DbContext not found" compilation error
**Resolution**:
```csharp
using DataAccessLayer.Data.Contexts;  // ← Add this
```

### Issue 2: "Type or namespace name does not exist"
**Resolution**: Ensure project references in .csproj:
```xml
<ProjectReference Include="../DataAccessLayer/DataAccessLayer.csproj" />
<ProjectReference Include="../DTOLayer/DTOLayer.csproj" />
```

### Issue 3: Circular dependency warning
**Resolution**: Verify dependency hierarchy - no project should reference itself or create cycles.

### Issue 4: Facet map properties not matching entity
**Resolution**: Review entity definition and add/remove facet map properties accordingly.

---

## Success Criteria

✅ Project successfully migrated when:
1. Project compiles without warnings or errors
2. Original DbContext file deleted from project folder
3. Program.cs uses DataAccessLayer DbContext
4. All APIs return facet maps (DTOs) instead of entities
5. Swagger/OpenAPI documentation reflects DTO contracts
6. All CRUD operations work as before
7. Tests pass (if applicable)
8. No deprecation warnings in build output

---

## Questions & Support

**When in doubt, reference**:
- `DTOLayer/FACET_MAPPING_GUIDE.md` - Usage patterns and examples
- `DTOLayer/FacetMaps/CopilotStudio/CopilotApplicationFacetMap.cs` - Reference implementation
- `DataAccessLayer/Data/Contexts/CopilotStudioDbContext.cs` - DbContext structure

