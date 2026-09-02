# Entity Framework Refactoring - Visual Architecture Guide

## 🏗️ Complete Architecture Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                      CLIENT APPLICATIONS                              │
│                  (Web Browsers, Mobile Apps, etc.)                    │
└────────────────────────────────┬─────────────────────────────────────┘
								 │
								 ↓
┌──────────────────────────────────────────────────────────────────────┐
│                    API LAYER (Controllers)                            │
│  ┌────────────────────┐  ┌────────────────────┐                      │
│  │ CopilotController  │  │ MapController      │                      │
│  │ ──────────────────  │  │ ──────────────────  │                      │
│  │ GET /copilots/{id} │  │ GET /routes/{id}   │                      │
│  │   └─ Returns DTO   │  │   └─ Returns DTO   │                      │
│  └────────────────────┘  └────────────────────┘                      │
│  📤 Returns: FacetMaps (DTOs) ← NOT Entities                          │
└────────────────────────────────┬─────────────────────────────────────┘
								 │
								 ↓
┌──────────────────────────────────────────────────────────────────────┐
│            SERVICE/BUSINESS LOGIC LAYER                               │
│  ┌────────────────────┐  ┌────────────────────┐                      │
│  │ ICopilotService    │  │ IMapService        │                      │
│  │ ──────────────────  │  │ ──────────────────  │                      │
│  │ - Validates input  │  │ - Calculates route │                      │
│  │ - Business rules   │  │ - Optimization     │                      │
│  │ - Converts to DTO  │  │ - Converts to DTO  │                      │
│  └────────────────────┘  └────────────────────┘                      │
│  🔄 Uses: Repositories                                               │
│  🔄 Converts: Entity ↔ FacetMap                                      │
└────────────────────────────────┬─────────────────────────────────────┘
								 │
								 ↓
┌──────────────────────────────────────────────────────────────────────┐
│         REPOSITORY LAYER (Data Access Pattern)                       │
│  ┌────────────────────┐  ┌────────────────────┐                      │
│  │ ICopilotRepository │  │ IMapRepository     │                      │
│  │ ──────────────────  │  │ ──────────────────  │                      │
│  │ GetById()          │  │ GetById()          │                      │
│  │ GetAll()           │  │ GetAll()           │                      │
│  │ Create()           │  │ Create()           │                      │
│  │ Update()           │  │ Update()           │                      │
│  │ Delete()           │  │ Delete()           │                      │
│  └────────────────────┘  └────────────────────┘                      │
│  🎯 Returns: FacetMaps to services                                   │
│  📊 Uses: DataAccessLayer DbContexts                                 │
└────────────────────────────────┬─────────────────────────────────────┘
								 │
	  ┌──────────────────────────┴──────────────────────────┐
	  │                                                      │
	  ↓                                                      ↓
┌─────────────────────────────────────┐  ┌────────────────────────────┐
│    DataAccessLayer (DbContexts)     │  │    DTOLayer (FacetMaps)    │
│  ┌─────────────────────────────────┐│  │┌──────────────────────────┐│
│  │CopilotStudioDbContext           ││  ││CopilotApplicationFacetMap││
│  │ DbSet<CopilotApplication>        ││  ││ - Id                     ││
│  │ DbSet<CopilotVersion>            ││  ││ - Name                   ││
│  │ DbSet<KnowledgeTool>             ││  ││ - LandingZone            ││
│  │ DbSet<GovernancePolicy>          ││  ││ + FromEntity()           ││
│  │ ... (7 DbSets total)             ││  ││ + ToEntity()             ││
│  └─────────────────────────────────┘│  │└──────────────────────────┘│
│  ┌─────────────────────────────────┐│  │┌──────────────────────────┐│
│  │MapAppDbContext                  ││  ││MapRouteFacetMap          ││
│  │ DbSet<MapRoute>                  ││  ││ - Id                     ││
│  │ DbSet<RouteSegment>              ││  ││ - Name                   ││
│  │ DbSet<RouteRedirection>          ││  ││ - Distance               ││
│  │ DbSet<Account>                   ││  ││ + FromEntity()           ││
│  │ (4 DbSets total)                 ││  ││ + ToEntity()             ││
│  └─────────────────────────────────┘│  │└──────────────────────────┘│
│  ┌─────────────────────────────────┐│  │┌──────────────────────────┐│
│  │ApplicationDbContext             ││  ││+ 12 More FacetMaps       ││
│  │ + Existing entities             ││  ││ (DataAccess domain)      ││
│  │ + 4 more DbContexts             ││  │└──────────────────────────┘│
│  │   (FoundryDbContext, etc.)      ││  │┌──────────────────────────┐│
│  └─────────────────────────────────┘│  ││IFacetMap (Interface)     ││
│  🔌 Provides: Entities              │  ││FacetMapRegistry (Helper) ││
│  📝 Holds: Database Models          │  │└──────────────────────────┘│
└─────────────────────────────────────┘  └────────────────────────────┘
	  │                                           │
	  └───────────────────┬──────────────────────┘
						  │
						  ↓
			┌─────────────────────────────┐
			│    SQL Server Database      │
			│  ┌───────────────────────┐  │
			│  │ CopilotApplications   │  │
			│  │ CopilotVersions       │  │
			│  │ MapRoutes             │  │
			│  │ ... (100+ tables)     │  │
			│  └───────────────────────┘  │
			└─────────────────────────────┘
```

---

## 🔄 Data Flow Examples

### Example 1: Get a Single Copilot

```
┌─────────────────────────────────────────────────────────────────────┐
│ 1. CLIENT REQUEST                                                    │
│    GET /api/copilots/copilot-123                                    │
└─────────────────────────────────────────────────────────────────────┘
							   ↓
┌─────────────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER                                                        │
│    [HttpGet("{id}")]                                                │
│    public async Task<ActionResult<CopilotApplicationFacetMap>>      │
│    GetCopilot(string id)                                            │
│    {                                                                │
│        var facet = await _service.GetCopilotAsync(id);              │
│        return Ok(facet);  // ← Returns FacetMap, NOT Entity        │
│    }                                                                │
└─────────────────────────────────────────────────────────────────────┘
							   ↓
┌─────────────────────────────────────────────────────────────────────┐
│ 3. SERVICE                                                           │
│    public async Task<CopilotApplicationFacetMap>                    │
│    GetCopilotAsync(string id)                                       │
│    {                                                                │
│        var copilot = await _repository.GetByIdAsync(id);            │
│        return copilot;  // ← Already a FacetMap from repository     │
│    }                                                                │
└─────────────────────────────────────────────────────────────────────┘
							   ↓
┌─────────────────────────────────────────────────────────────────────┐
│ 4. REPOSITORY                                                        │
│    public async Task<CopilotApplicationFacetMap>                    │
│    GetByIdAsync(string id)                                          │
│    {                                                                │
│        var entity = await _context.CopilotApplications              │
│            .AsNoTracking()                                          │
│            .FirstOrDefaultAsync(e => e.CopilotId == id);            │
│        return entity == null ? null                                 │
│            : CopilotApplicationFacetMap.FromEntity(entity);         │
│    }                                                                │
└─────────────────────────────────────────────────────────────────────┘
							   ↓
┌─────────────────────────────────────────────────────────────────────┐
│ 5. DATABASE CONTEXT                                                  │
│    _context.CopilotApplications.FirstOrDefaultAsync(...)            │
│    ↓                                                                │
│    SQL Query: SELECT * FROM CopilotApplications                     │
│    WHERE CopilotId = 'copilot-123'                                  │
│    ↓                                                                │
│    Returns ENTITY (CopilotApplication object)                       │
└─────────────────────────────────────────────────────────────────────┘
							   ↓
┌─────────────────────────────────────────────────────────────────────┐
│ 6. CONVERSION (Repository)                                           │
│    CopilotApplication entity                                        │
│    ↓                                                                │
│    CopilotApplicationFacetMap.FromEntity(entity)                    │
│    ↓                                                                │
│    CopilotApplicationFacetMap (DTO)                                │
└─────────────────────────────────────────────────────────────────────┘
							   ↓
┌─────────────────────────────────────────────────────────────────────┐
│ 7. RESPONSE TO CLIENT                                                │
│    {                                                                │
│      "copilotId": "copilot-123",                                    │
│      "name": "My Copilot",                                          │
│      "landingZone": "production",                                   │
│      "isActive": true                                               │
│    }                                                                │
│    ✅ DTO returned, NO internal fields exposed                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Layer Responsibilities

```
┌─────────────────────────────────────────────────────────────────────┐
│ LAYER                │ RESPONSIBILITY         │ DATABASE ACCESS    │
├─────────────────────┼───────────────────────┼────────────────────┤
│ API (Controllers)   │ - Receive requests    │ ❌ FORBIDDEN       │
│                     │ - Validate input      │ Use Service Layer  │
│                     │ - Return responses    │                    │
│                     │ - Return DTOs only    │                    │
├─────────────────────┼───────────────────────┼────────────────────┤
│ Service (Business)  │ - Implement rules     │ ❌ FORBIDDEN       │
│                     │ - Orchestrate logic   │ Use Repository     │
│                     │ - Call repositories   │                    │
│                     │ - Convert Entity→DTO  │                    │
├─────────────────────┼───────────────────────┼────────────────────┤
│ Repository (CRUD)   │ - Query database      │ ✅ ALLOWED         │
│                     │ - Create entities     │ Direct DbContext   │
│                     │ - Update entities     │ calls              │
│                     │ - Delete entities     │                    │
│                     │ - Convert Entity→DTO  │                    │
├─────────────────────┼───────────────────────┼────────────────────┤
│ DataAccessLayer     │ - Define DbContexts   │ ✅ ONLY PLACE      │
│ (DbContexts)        │ - Entity models       │ DbContexts live    │
│                     │ - Migrations          │                    │
├─────────────────────┼───────────────────────┼────────────────────┤
│ DTOLayer            │ - Define FacetMaps    │ ❌ NO DATABASE     │
│ (FacetMaps)         │ - Entity↔DTO convert  │ Pure conversion    │
│                     │ - Interfaces          │ mappings           │
├─────────────────────┼───────────────────────┼────────────────────┤
│ Database            │ - Store data          │ SQL Server         │
│                     │ - Run queries         │ tables & indexes   │
└─────────────────────┴───────────────────────┴────────────────────┘
```

---

## 🔀 Dependency Arrow Rules

✅ **Allowed**: Arrows ONLY flow downward

```
API → Service → Repository → DataAccessLayer → Database
 ↓      ↓          ↓              ↓
Service Repository DataAccessLayer DTOLayer
```

❌ **NEVER**: Reverse arrows or sideways arrows

```
Database → API (❌ FORBIDDEN)
API → Database (❌ FORBIDDEN)
Service → Database (❌ FORBIDDEN)
Repository ← Service (❌ FORBIDDEN)
```

---

## 👀 Entity vs FacetMap Visibility

```
┌──────────────────────────────────────────────────────────────────────┐
│ ENTITY (Database Model) - INTERNAL                                    │
├──────────────────────────────────────────────────────────────────────┤
│ public class CopilotApplication                                       │
│ {                                                                     │
│     public string CopilotId { get; set; }            ✅ Public       │
│     public string Name { get; set; }                  ✅ Public       │
│     public string InternalPassword { get; set; }      ✅ Public ⚠️     │
│     public Dictionary<string, string> Secrets { get; set; } ✅ Public │
│     public List<CopilotVersion> Versions { get; set; } ✅ Nav prop  │
│     internal byte[] EncryptionKey { get; set; }       ❌ Internal    │
│ }                                                                     │
│                                                                       │
│ ⚠️ PROBLEM: Exposed sensitive data                                   │
└──────────────────────────────────────────────────────────────────────┘
							   ↓↓↓
		  FACET MAP (Public API Contract)
							   ↓↓↓
┌──────────────────────────────────────────────────────────────────────┐
│ FACET MAP (DTO) - What Client Sees                                    │
├──────────────────────────────────────────────────────────────────────┤
│ public class CopilotApplicationFacetMap                               │
│ {                                                                     │
│     public string CopilotId { get; set; }            ✅ Exposed      │
│     public string Name { get; set; }                  ✅ Exposed      │
│     public bool IsActive { get; set; }                ✅ Exposed      │
│     // NO InternalPassword                            ❌ Hidden      │
│     // NO Secrets                                      ❌ Hidden      │
│     // NO EncryptionKey                                ❌ Hidden      │
│     // NO Navigation properties                        ❌ Hidden      │
│ }                                                                     │
│                                                                       │
│ ✅ SOLUTION: Only expose what's needed                               │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 📈 Request-Response Lifecycle

```
TIME ┬
	 │
	 │  Client: GET /api/copilots/123
	 │
	 ├──→ Controller receives request
	 │    └─→ Validates AuthZ/N
	 │
	 ├──→ Calls Service.GetCopilotAsync(id)
	 │    └─→ Service calls Repository.GetByIdAsync(id)
	 │
	 ├──→ Repository queries database
	 │    ├─→ DbContext executes SQL
	 │    ├─→ Receives CopilotApplication (ENTITY)
	 │    ├─→ Converts via FromEntity()
	 │    └─→ Returns CopilotApplicationFacetMap (DTO)
	 │
	 ├──→ Service receives FacetMap
	 │    └─→ Returns unchanged to Controller
	 │
	 ├──→ Controller receives FacetMap
	 │    ├─→ Serializes to JSON
	 │    └─→ Returns to client
	 │
	 └──→ Client receives JSON response
		  (No database details exposed)
```

---

## 🎯 Pattern Templates

### Template: Complete Request Cycle

```csharp
// 1. CLIENT REQUEST
GET /api/resource/{id}

// 2. CONTROLLER
[HttpGet("{id}")]
public async Task<ActionResult<ResourceFacetMap>> Get(int id)
{
	var facet = await _service.GetAsync(id);
	return Ok(facet);  // ← FacetMap only
}

// 3. SERVICE
public async Task<ResourceFacetMap> GetAsync(int id)
{
	return await _repository.GetByIdAsync(id);
}

// 4. REPOSITORY
public async Task<ResourceFacetMap> GetByIdAsync(int id)
{
	var entity = await _context.Resources.FindAsync(id);
	return entity == null ? null : ResourceFacetMap.FromEntity(entity);
}

// 5. FACET MAP
public class ResourceFacetMap
{
	public int Id { get; set; }
	public string Name { get; set; }

	public static ResourceFacetMap FromEntity(Resource entity)
	{
		return new ResourceFacetMap
		{
			Id = entity.Id,
			Name = entity.Name
		};
	}
}

// 6. RESPONSE
{
	"id": 123,
	"name": "Resource Name"
}
```

---

## 🛡️ Security Boundaries

```
┌─────────────────────────────────────────────────────────────────────┐
│                        🌐 EXTERNAL WORLD                            │
│              (Untrusted Clients, Internet, APIs)                    │
└────────────────────────────┬────────────────────────────────────────┘
							 │
					🚧 SECURITY BOUNDARY 🚧
							 │
┌────────────────────────────┴────────────────────────────────────────┐
│                    ✅ SAFE ZONE (Application)                       │
│                                                                      │
│  API Layer:  Accepts only FacetMaps from client                     │
│  ↓           Returns only FacetMaps to client                       │
│  Service: Orchestrates business logic                              │
│  ↓        Never exposes internal state                             │
│  Repository: Queries database, converts Entity→FacetMap            │
│  ↓          Entity never leaves this layer                         │
│  Database: Stores raw data                                         │
│                                                                      │
│  🔒 Key Rule: Entity ≠ FacetMap                                    │
│     Entities NEVER cross the API boundary                          │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Project Structure After Refactoring

```
TubieTools_Aspire/
│
├── 📁 DataAccessLayer/                 ← DbContexts live here
│   └── Data/
│       ├── Contexts/
│       │   ├── CopilotStudioDbContext.cs
│       │   ├── MapAppDbContext.cs
│       │   ├── ApplicationDbContext.cs
│       │   ├── FoundryDbContext.cs
│       │   ├── TenantDbContext.cs
│       │   ├── TubieDbContext.cs
│       │   └── KitContext.cs
│       └── Entities/
│           └── (all entity models)
│
├── 📁 DTOLayer/                        ← FacetMaps live here
│   └── FacetMaps/
│       ├── CopilotStudio/
│       │   ├── CopilotApplicationFacetMap.cs
│       │   ├── CopilotVersionFacetMap.cs
│       │   └── ... (7 total)
│       ├── MapApp/
│       │   ├── MapRouteFacetMap.cs
│       │   └── ... (4 total)
│       ├── DataAccess/
│       │   ├── AddressFacetMap.cs
│       │   └── ... (3 total)
│       ├── IFacetMap.cs
│       └── FacetMapRegistry.cs
│
├── 📁 TubieTools_CopilotStudio_API/    ← Uses DataAccessLayer
│   ├── Controllers/
│   │   └── CopilotController.cs
│   ├── Services/
│   │   └── CopilotService.cs
│   └── Repositories/
│       └── CopilotRepository.cs
│
├── 📁 TubieTools_Map/                  ← Uses DataAccessLayer
│   ├── Components/
│   └── Services/
│
├── 📁 ServiceLayer/
│   └── (Shared services)
│
└── 📁 Documentation/
	├── FACET_MAPPING_GUIDE.md
	├── ENTITY_FRAMEWORK_MIGRATION_CHECKLIST.md
	├── DataAccessLayer/README.md
	└── ... (8 docs total)
```

---

## ✨ Key Improvements Visualized

### Before ❌
```
Project A: DbContext   → Entity → API Response (Entity exposed!)
Project B: DbContext   → (duplicate code)
Project C: DbContext   → (more duplicates)
Result: Tight coupling, redundancy, security issues
```

### After ✅
```
All Projects → DataAccessLayer: DbContext → Entity
			  ↓
			  DTOLayer: FacetMap (DTO)  ← Consistent interface
			  ↓
			  API Response (Safe DTO, no internals)

Result: Single source of truth, loose coupling, secure APIs
```

---

## 🎓 Learning: Visual Summary

```
Learn This...          In This Document          Time

┌─ Concepts ─────────────────────────────────────────────┐
│ "Why this architecture?" → Summary.md                  │ 10 min
│ "How does it work?" → This visual guide + README.md    │ 15 min
└─────────────────────────────────────────────────────────┘
		↓
┌─ Patterns ──────────────────────────────────────────────┐
│ "Show me patterns" → FACET_MAPPING_GUIDE.md (Patterns   │ 20 min
│                       1-4 with code examples)           │
└─────────────────────────────────────────────────────────┘
		↓
┌─ Implementation ────────────────────────────────────────┐
│ "How do I do this?" → MIGRATION_CHECKLIST.md            │ 15 min
│                   → Code examples                       │
└─────────────────────────────────────────────────────────┘
		↓
┌─ Practice ──────────────────────────────────────────────┐
│ "Let me try..."  → Follow the patterns                  │ 30 min
│                   → Build your code                     │
└─────────────────────────────────────────────────────────┘
		↓
✅ YOU'RE READY! (Total: ~1 hour)
```

---

**This visual guide pairs with all documentation files.  
Reference this together with `DataAccessLayer/README.md` and `DTOLayer/FACET_MAPPING_GUIDE.md` for complete understanding.**
