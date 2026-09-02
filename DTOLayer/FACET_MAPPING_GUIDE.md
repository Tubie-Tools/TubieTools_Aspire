# Facet Mapping Layer - DTOLayer Implementation Guide

## Overview

The **Facet Mapping Layer** provides a clean separation between Entity Framework entities and Data Transfer Objects (DTOs). This architecture ensures:

- ✅ **Loose Coupling** - Projects don't depend directly on EF entities
- ✅ **Simplified APIs** - DTOs expose only necessary fields
- ✅ **Centralized Mapping** - All conversions in one place
- ✅ **Type Safety** - Generic interfaces with compile-time checking
- ✅ **Easy Maintenance** - Changes to entities don't affect API contracts

---

## Architecture

### Layer Structure

```
DataAccessLayer/
  └── Data/Contexts/
	  ├── ApplicationDbContext.cs
	  ├── CopilotStudioDbContext.cs      ← Centralized EF contexts
	  ├── MapAppDbContext.cs
	  └── ... other DbContexts

DTOLayer/
  └── FacetMaps/
	  ├── IFacetMap.cs                   ← Generic mapping interface
	  ├── FacetMapRegistry.cs            ← Central registry with extensions
	  ├── CopilotStudio/
	  │   ├── CopilotApplicationFacetMap.cs
	  │   ├── KnowledgeToolFacetMap.cs
	  │   └── ... other maps
	  ├── MapApp/
	  │   ├── MapRouteFacetMap.cs
	  │   ├── AccountFacetMap.cs
	  │   └── ... other maps
	  └── DataAccess/
		  ├── AddressFacetMap.cs
		  ├── OrderFacetMap.cs
		  └── ... other maps

API Projects/
  └── Program.cs                         ← Use entities from DataAccessLayer
										  ← Return DTOs from facet maps
```

---

## Usage Patterns

### Pattern 1: Basic Entity to Facet Conversion

```csharp
// In a service or repository
using DTOLayer.FacetMaps.CopilotStudio;

public class CopilotApplicationService
{
	private readonly CopilotStudioDbContext _context;

	public CopilotApplicationService(CopilotStudioDbContext context)
	{
		_context = context;
	}

	// Return DTO instead of entity
	public CopilotApplicationFacetMap GetCopilot(string copilotId)
	{
		var entity = _context.CopilotApplications.Find(copilotId);
		if (entity == null) 
			return null;

		return CopilotApplicationFacetMap.FromEntity(entity);
	}

	// List of DTOs
	public List<CopilotApplicationFacetMap> GetAllCopilots()
	{
		return _context.CopilotApplications
			.AsNoTracking()
			.ToList()
			.Select(CopilotApplicationFacetMap.FromEntity)
			.ToList();
	}
}
```

### Pattern 2: Using Extension Methods (FacetMapRegistry)

```csharp
using DTOLayer.FacetMaps;

public class CopilotApplicationService
{
	// After setting up in FacetMapRegistry (via reflection)
	public CopilotApplicationFacetMap GetCopilot(string copilotId)
	{
		var entity = _context.CopilotApplications.Find(copilotId);

		// Using extension method
		return entity?.ToFacet<CopilotApplicationFacetMap>();
	}
}
```

### Pattern 3: API Controller Returning DTOs

```csharp
using DTOLayer.FacetMaps.CopilotStudio;
using DataAccessLayer.Data.Contexts;

[ApiController]
[Route("api/[controller]")]
public class CopilotApplicationController : ControllerBase
{
	private readonly CopilotStudioDbContext _context;

	[HttpGet("{id}")]
	public ActionResult<CopilotApplicationFacetMap> GetCopilot(string id)
	{
		var entity = _context.CopilotApplications.Find(id);
		if (entity == null)
			return NotFound();

		return Ok(CopilotApplicationFacetMap.FromEntity(entity));
	}

	[HttpPost]
	public ActionResult<CopilotApplicationFacetMap> CreateCopilot(
		CopilotApplicationFacetMap facet)
	{
		var entity = new CopilotApplication
		{
			CopilotId = facet.CopilotId,
			Name = facet.Name,
			Description = facet.Description,
			LandingZone = facet.LandingZone,
			IsActive = facet.IsActive
		};

		_context.CopilotApplications.Add(entity);
		_context.SaveChanges();

		return CreatedAtAction(nameof(GetCopilot), 
			new { id = entity.CopilotId }, 
			CopilotApplicationFacetMap.FromEntity(entity));
	}
}
```

### Pattern 4: Repository Pattern with Facet Maps

```csharp
using DTOLayer.FacetMaps.CopilotStudio;
using DataAccessLayer.Data.Contexts;

public interface ICopilotApplicationRepository
{
	Task<CopilotApplicationFacetMap> GetByIdAsync(string id);
	Task<List<CopilotApplicationFacetMap>> GetAllAsync();
	Task<CopilotApplicationFacetMap> CreateAsync(CopilotApplicationFacetMap facet);
	Task<CopilotApplicationFacetMap> UpdateAsync(CopilotApplicationFacetMap facet);
	Task DeleteAsync(string id);
}

public class CopilotApplicationRepository : ICopilotApplicationRepository
{
	private readonly CopilotStudioDbContext _context;

	public CopilotApplicationRepository(CopilotStudioDbContext context)
	{
		_context = context;
	}

	public async Task<CopilotApplicationFacetMap> GetByIdAsync(string id)
	{
		var entity = await _context.CopilotApplications.FindAsync(id);
		return entity == null ? null : CopilotApplicationFacetMap.FromEntity(entity);
	}

	public async Task<List<CopilotApplicationFacetMap>> GetAllAsync()
	{
		return await _context.CopilotApplications
			.AsNoTracking()
			.Select(e => CopilotApplicationFacetMap.FromEntity(e))
			.ToListAsync();
	}

	public async Task<CopilotApplicationFacetMap> CreateAsync(CopilotApplicationFacetMap facet)
	{
		var entity = new CopilotApplication
		{
			CopilotId = facet.CopilotId ?? Guid.NewGuid().ToString(),
			Name = facet.Name,
			Description = facet.Description,
			LandingZone = facet.LandingZone,
			IsActive = facet.IsActive,
			CreatedDate = DateTime.UtcNow,
			LastModifiedDate = DateTime.UtcNow
		};

		_context.CopilotApplications.Add(entity);
		await _context.SaveChangesAsync();

		return CopilotApplicationFacetMap.FromEntity(entity);
	}

	public async Task<CopilotApplicationFacetMap> UpdateAsync(CopilotApplicationFacetMap facet)
	{
		var entity = await _context.CopilotApplications.FindAsync(facet.CopilotId);
		if (entity == null)
			throw new KeyNotFoundException($"Copilot {facet.CopilotId} not found");

		entity.Name = facet.Name;
		entity.Description = facet.Description;
		entity.LandingZone = facet.LandingZone;
		entity.IsActive = facet.IsActive;
		entity.LastModifiedDate = DateTime.UtcNow;

		_context.CopilotApplications.Update(entity);
		await _context.SaveChangesAsync();

		return CopilotApplicationFacetMap.FromEntity(entity);
	}

	public async Task DeleteAsync(string id)
	{
		var entity = await _context.CopilotApplications.FindAsync(id);
		if (entity == null)
			throw new KeyNotFoundException($"Copilot {id} not found");

		_context.CopilotApplications.Remove(entity);
		await _context.SaveChangesAsync();
	}
}
```

---

## Migration Checklist

### For Each Project Using Entity Framework:

- [ ] **1. Add Project References**
  ```xml
  <ProjectReference Include="../DataAccessLayer/DataAccessLayer.csproj" />
  <ProjectReference Include="../DTOLayer/DTOLayer.csproj" />
  ```

- [ ] **2. Update Using Statements**
  ```csharp
  // OLD: using TubieTools_CopilotStudio_API.Data;
  // NEW:
  using DataAccessLayer.Data.Contexts;
  using DTOLayer.FacetMaps.CopilotStudio;
  ```

- [ ] **3. Update Program.cs DbContext Registration**
  ```csharp
  // Change from: services.AddDbContext<CopilotStudioDbContext>()
  // To:
  builder.Services.AddDbContext<CopilotStudioDbContext>(options =>
	  options.UseSqlServer(connectionString));
  ```

- [ ] **4. Update Repository/Service Methods**
  ```csharp
  // Change return types from entity to facet
  public CopilotApplicationFacetMap GetCopilot(string id)
  {
	  var entity = _context.CopilotApplications.Find(id);
	  return CopilotApplicationFacetMap.FromEntity(entity);
  }
  ```

- [ ] **5. Delete Old DbContext Files**
  - Remove `Project.Data/ProjectDbContext.cs`
  - Keep only DataAccessLayer versions

- [ ] **6. Update API Controllers**
  ```csharp
  [HttpGet("{id}")]
  public ActionResult<CopilotApplicationFacetMap> GetCopilot(string id)
  {
	  var facet = _service.GetCopilot(id);
	  return Ok(facet);
  }
  ```

---

## Creating New Facet Maps

### Template for New Entity

```csharp
using YourEntity;

namespace DTOLayer.FacetMaps.YourDomain;

/// <summary>
/// Facet mapping for YourEntity.
/// </summary>
public class YourEntityFacetMap
{
	// Public properties only for fields you want to expose
	public int Id { get; set; }
	public string? Name { get; set; }
	// ... other properties

	/// <summary>
	/// Maps from entity to facet.
	/// </summary>
	public static YourEntityFacetMap FromEntity(YourEntity entity)
	{
		return new YourEntityFacetMap
		{
			Id = entity.Id,
			Name = entity.Name,
			// ... map other properties
		};
	}

	/// <summary>
	/// Maps from facet to entity.
	/// </summary>
	public YourEntity ToEntity()
	{
		return new YourEntity
		{
			Id = Id,
			Name = Name,
			// ... map other properties
		};
	}
}
```

---

## Benefits of This Architecture

| Benefit | How It's Achieved |
|---------|------------------|
| **API Stability** | DTOs define contracts; entity changes don't break APIs |
| **Security** | Never expose internal fields (e.g., passwords, secrets) |
| **Performance** | Map only necessary fields; lazy load related data |
| **Clear Separation** | Data layer concerns separate from business logic |
| **Reusability** | Facet map logic used across repositories, services, APIs |
| **Testing** | Mock facet maps easily; test mapping logic independently |

---

## Next Steps

1. ✅ **Completed**: Core facet maps for CopilotStudio and MapApp domains
2. ⏭️ **TODO**: Update remaining projects (PublicAPI, Forecasting, etc.)
3. ⏭️ **TODO**: Create Swagger/OpenAPI documentation for facet DTO contracts
4. ⏭️ **TODO**: Add unit tests for facet map conversions
5. ⏭️ **TODO**: Consider AutoMapper integration if mapped properties increase

---

## Support

For questions about facet mapping:
- Check existing maps in `DTOLayer/FacetMaps/`
- Follow the pattern from `CopilotApplicationFacetMap.cs`
- Reference the repository pattern in `Pattern 4` above
