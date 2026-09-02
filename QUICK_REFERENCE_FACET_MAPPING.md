# Quick Reference Card - Facet Mapping Architecture

## ⚡ Quick Start (30 seconds)

### 1. Get Entity from Database
```csharp
var entity = await _context.CopilotApplications.FindAsync(id);
```

### 2. Convert to Facet (DTO)
```csharp
var facet = CopilotApplicationFacetMap.FromEntity(entity);
```

### 3. Return from API
```csharp
return Ok(facet);
```

---

## 📁 File Structure

```
Project Structure After Refactoring:
├── DataAccessLayer/
│   └── Data/Contexts/  ← All DbContexts here
├── DTOLayer/
│   └── FacetMaps/      ← All DTO mappings here
├── API Projects/
│   └── Use DataAccessLayer + DTOLayer
└── Service/Repository/
	└── Convert entities to facets
```

---

## 🔄 Common Patterns

| Need | Code |
|------|------|
| **Get single** | `CopilotApplicationFacetMap.FromEntity(entity)` |
| **Get list** | `entities.Select(CopilotApplicationFacetMap.FromEntity).ToList()` |
| **Create DTO** | `new CopilotApplicationFacetMap { ... }` |
| **To Entity** | `facet.ToEntity()` |
| **In controller** | Return `facet` not `entity` |

---

## ✅ Checklist Before API Call

- [ ] Return type is `FacetMap`, not Entity
- [ ] Using `CopilotApplicationFacetMap`, not `CopilotApplication`
- [ ] DbContext from `DataAccessLayer.Data.Contexts`
- [ ] DTO from `DTOLayer.FacetMaps.{Domain}`
- [ ] Serialization is JSON-safe
- [ ] No sensitive data exposed

---

## 🚀 Create New Facet Map in 2 Minutes

```csharp
public class MyEntityFacetMap
{
	// Public properties only
	public int Id { get; set; }
	public string? Name { get; set; }

	// From entity
	public static MyEntityFacetMap FromEntity(MyEntity entity)
	{
		return new MyEntityFacetMap
		{
			Id = entity.Id,
			Name = entity.Name
		};
	}

	// To entity
	public MyEntity ToEntity()
	{
		return new MyEntity { Id = Id, Name = Name };
	}
}
```

---

## 🔍 Troubleshooting

| Problem | Solution |
|---------|----------|
| "DbContext not found" | `using DataAccessLayer.Data.Contexts;` |
| "FacetMap doesn't exist" | Create it in `DTOLayer/FacetMaps/{Domain}/` |
| "Type mismatch" | Ensure properties match entity type |
| "Circular reference" | Don't include navigation properties |
| "Null reference" | Check `if (entity == null)` |

---

## 📚 Key Files to Know

| File | Purpose |
|------|---------|
| `DataAccessLayer/Data/Contexts/*.cs` | All database contexts |
| `DTOLayer/FacetMaps/*/FacetMap.cs` | All DTO conversions |
| `DTOLayer/FACET_MAPPING_GUIDE.md` | Detailed patterns & examples |
| `DataAccessLayer/README.md` | Architecture & best practices |

---

## 💡 Pro Tips

```csharp
// 1. Always use AsNoTracking() for read-only queries
var entities = _context.Set<T>().AsNoTracking().ToList();

// 2. Project only needed fields
var names = _context.CopilotApplications
	.Select(c => new { c.Id, c.Name })
	.ToList();

// 3. Use List() to convert IEnumerable<Entity> to IEnumerable<FacetMap>
var facets = entities.Select(CopilotApplicationFacetMap.FromEntity).ToList();

// 4. Error handling in repositories
public async Task<CopilotApplicationFacetMap> GetAsync(string id)
{
	var entity = await _context.CopilotApplications.FindAsync(id);
	return entity == null ? null : CopilotApplicationFacetMap.FromEntity(entity);
}
```

---

## 🎯 DOs and DON'Ts

✅ **DO:**
- Return facet maps from API controllers
- Use entities only in repositories
- Keep facet maps lean and simple
- Add comments for non-obvious mappings
- Test facet map conversions

❌ **DON'T:**
- Return entities from APIs
- Make facet maps depend on other entities
- Include navigation properties in DTOs
- Expose internal fields
- Create circular dependencies

---

## 🔗 Quick Navigation

| Question | Go To |
|----------|-------|
| "How do I use this?" | `DTOLayer/FACET_MAPPING_GUIDE.md` |
| "I need code examples" | `DTOLayer/FACET_MAPPING_GUIDE.md` → Pattern 1-4 |
| "Architecture overview" | `ENTITY_FRAMEWORK_REFACTORING_SUMMARY.md` |
| "Step-by-step migration" | `ENTITY_FRAMEWORK_MIGRATION_CHECKLIST.md` |
| "DbContext details" | `DataAccessLayer/README.md` |
| "Generate new facet map" | `DTOLayer/FACET_MAP_GENERATOR.cs` |

---

## 📞 Help Commands

```bash
# Find all facet maps
find . -name "*FacetMap.cs"

# Check for entity usage in API
grep -r "DbSet\|DbContext" TubieTools_CopilotStudio_API

# Build specific project
dotnet build TubieTools_CopilotStudio_API.csproj

# Add migration
dotnet ef migrations add MigrationName -p DataAccessLayer
```

---

## 🎓 Learning Path (15 min)

1. **5 min** - Read this quick reference
2. **5 min** - Review `CopilotApplicationFacetMap.cs` example
3. **5 min** - Read Pattern 1 from `FACET_MAPPING_GUIDE.md`
4. **Done!** - You're ready to code

---

## Version

- **Last Updated**: 2024
- **Status**: Production Ready (Phase 1)
- **Applicable To**: All TubieTools API Projects
