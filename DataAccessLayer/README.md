# DataAccessLayer - Centralized Entity Framework Repository

## Overview

The **DataAccessLayer** is the single point of responsibility for all Entity Framework Core contexts and database operations in the TubieTools system. All projects access the database exclusively through this layer.

## Architecture

```
DataAccessLayer/
├── Data/
│   ├── Contexts/
│   │   ├── ApplicationDbContext.cs      (Core business entities)
│   │   ├── CopilotStudioDbContext.cs    (Copilot Studio entities)
│   │   ├── FoundryDbContext.cs          (Foundry entities)
│   │   ├── MapAppDbContext.cs           (Map & Route entities)
│   │   ├── TenantDbContext.cs           (Multi-tenancy)
│   │   ├── TubieDbContext.cs            (Tubie-specific)
│   │   └── KitContext.cs                (Kit entities)
│   └── Entities/
│       ├── AddressEntity.cs
│       ├── OrderEntity.cs
│       ├── CareProviderEntity.cs
│       └── ... (other entities)
├── Migrations/
│   └── ... (EF Core migrations)
└── README.md (this file)
```

## DbContexts

### 1. **CopilotStudioDbContext**
**Purpose**: Manages Copilot Studio application and configuration entities
**DbSets**:
- `CopilotApplications` - Core copilot applications
- `ModelConfigurations` - LLM model configuration
- `KnowledgeTools` - Knowledge base tools
- `GovernancePolicies` - Access policies
- `PerformanceMetrics` - Usage metrics
- `DeploymentConfigs` - Environment deployments
- `Versions` - Version management

**Database**: Dedicated SQL Server database (configured via connection string)

### 2. **MapAppDbContext**
**Purpose**: Manages route planning and mapping entities
**DbSets**:
- `Routes` - Defined routes
- `RouteSegments` - Route segments with coordinates
- `RouteRedirections` - Alternative routes
- `Accounts` - User accounts

**Database**: SQL Server or In-Memory (based on environment)

### 3. **ApplicationDbContext**
**Purpose**: Core business entities (Addresses, Orders, Care Providers, etc.)
**DbSets**: Review current implementation for list

### 4. **FoundryDbContext**
**Purpose**: Foundry-specific data models
**Status**: Requires review for facet mapping

### 5. **TenantDbContext**
**Purpose**: Multi-tenancy support
**DbSets**: Tenant-specific configurations

### 6. **TubieDbContext**
**Purpose**: Tubie-specific entities
**Status**: Requires review for facet mapping

### 7. **KitContext**
**Purpose**: Kit-related entities
**Status**: Requires review for facet mapping

---

## Usage Patterns

### Pattern 1: Dependency Injection in API

```csharp
using DataAccessLayer.Data.Contexts;

public class CopilotController : ControllerBase
{
	private readonly CopilotStudioDbContext _context;

	public CopilotController(CopilotStudioDbContext context)
	{
		_context = context;
	}

	[HttpGet("{id}")]
	public async Task<ActionResult> GetCopilot(string id)
	{
		var entity = await _context.CopilotApplications.FindAsync(id);
		if (entity == null) return NotFound();

		return Ok(entity.ToFacet<CopilotApplicationFacetMap>());
	}
}
```

### Pattern 2: Scoped Repositories

```csharp
public interface ICopilotRepository
{
	Task<CopilotApplicationFacetMap> GetByIdAsync(string id);
	Task<List<CopilotApplicationFacetMap>> GetAllAsync();
	Task CreateAsync(CopilotApplicationFacetMap facet);
	Task UpdateAsync(CopilotApplicationFacetMap facet);
	Task DeleteAsync(string id);
}

public class CopilotRepository : ICopilotRepository
{
	private readonly CopilotStudioDbContext _context;

	public CopilotRepository(CopilotStudioDbContext context)
	{
		_context = context;
	}

	public async Task<CopilotApplicationFacetMap> GetByIdAsync(string id)
	{
		var entity = await _context.CopilotApplications
			.AsNoTracking()
			.FirstOrDefaultAsync(e => e.CopilotId == id);

		return entity == null ? null : CopilotApplicationFacetMap.FromEntity(entity);
	}

	// ... implement other methods
}
```

### Pattern 3: Service Layer Using Repositories

```csharp
public interface ICopilotService
{
	Task<CopilotApplicationFacetMap> GetCopilotAsync(string id);
	Task<List<CopilotApplicationFacetMap>> GetAllCopilotsAsync();
}

public class CopilotService : ICopilotService
{
	private readonly ICopilotRepository _repository;

	public CopilotService(ICopilotRepository repository)
	{
		_repository = repository;
	}

	public async Task<CopilotApplicationFacetMap> GetCopilotAsync(string id)
	{
		if (string.IsNullOrEmpty(id))
			throw new ArgumentException("ID cannot be empty", nameof(id));

		return await _repository.GetByIdAsync(id);
	}

	public async Task<List<CopilotApplicationFacetMap>> GetAllCopilotsAsync()
	{
		return await _repository.GetAllAsync();
	}
}
```

---

## Dependency Injection Registration

### In Program.cs

```csharp
// Register all DbContexts
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string not found");

// CopilotStudio Context
builder.Services.AddDbContext<CopilotStudioDbContext>(options =>
	options.UseSqlServer(connectionString));

// Map Context
builder.Services.AddDbContext<MapAppDbContext>(options =>
{
	if (builder.Environment.IsDevelopment())
		options.UseInMemoryDatabase("MapAppDb");
	else
		options.UseSqlServer(connectionString);
});

// Application Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped<ICopilotRepository, CopilotRepository>();
builder.Services.AddScoped<IMapRepository, MapRepository>();

// Register Services
builder.Services.AddScoped<ICopilotService, CopilotService>();
builder.Services.AddScoped<IMapService, MapService>();
```

---

## Entity Framework Migrations

### Create Migration

```bash
# From DataAccessLayer project directory
dotnet ef migrations add <MigrationName> -c <DbContextName>

# Example:
dotnet ef migrations add AddCopilotVersion -c CopilotStudioDbContext
```

### Apply Migrations

```bash
# Automatic (on startup)
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<CopilotStudioDbContext>();
	await db.Database.MigrateAsync();
}

# Manual
dotnet ef database update -c <DbContextName>
```

### Remove Last Migration

```bash
dotnet ef migrations remove -c <DbContextName>
```

---

## Convention and Best Practices

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| DbContext Class | `{Domain}DbContext` | `CopilotStudioDbContext` |
| DbSet Property | Plural noun | `CopilotApplications` |
| Entity Class | `{EntityName}Entity` OR just `{EntityName}` | `CopilotApplication` |
| Migration | `{YYYYMMDDHHmmss}_{Description}` | `20240115120000_AddCopilotVersion` |
| Table Name | Plural form | `CopilotApplications` |
| Primary Key | `Id` or `{Entity}Id` | `CopilotId` |
| Foreign Key | `{Entity}Id` | `CopilotId` (in related entity) |

### Entity Configuration

```csharp
// In OnModelCreating
modelBuilder.Entity<CopilotApplication>(entity =>
{
	// Primary Key
	entity.HasKey(e => e.CopilotId);

	// Required properties
	entity.Property(e => e.Name)
		.IsRequired()
		.HasMaxLength(255);

	// Defaults
	entity.Property(e => e.CreatedDate)
		.HasDefaultValueSql("GETUTCDATE()");

	// Indexes
	entity.HasIndex(e => e.Name).IsUnique();
	entity.HasIndex(e => e.IsActive);

	// Relationships
	entity.HasMany(e => e.Versions)
		.WithOne()
		.HasForeignKey("CopilotId")
		.OnDelete(DeleteBehavior.Cascade);
});
```

### Data Seeding

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	base.OnModelCreating(modelBuilder);

	// Seed data
	modelBuilder.Entity<CopilotApplication>().HasData(
		new CopilotApplication
		{
			CopilotId = "default-copilot",
			Name = "Default Copilot",
			LandingZone = "production",
			IsActive = true
		}
	);
}
```

---

## Connection Strings

### Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=TubieTools;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

### Development (.NET User Secrets)

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TubieTools_Dev;User Id=sa;Password=MyPassword123;TrustServerCertificate=true;"
```

---

## Common Issues & Solutions

### Issue: "A navigation property is not configured"
**Solution**: Configure relationships in `OnModelCreating`:
```csharp
entity.HasOne(e => e.RelatedEntity)
	.WithMany(e => e.MainEntities)
	.HasForeignKey(e => e.RelatedEntityId);
```

### Issue: "An error occurred while using the connection to the database"
**Solution**: Verify connection string and SQL Server is running:
```bash
dotnet ef database update -c <DbContextName> --verbose
```

### Issue: "Cannot create a DbSet for type that is not part of the model"
**Solution**: Ensure entity is registered in DbContext:
```csharp
public DbSet<MyEntity> MyEntities { get; set; }
```

### Issue: "Migrations pending"
**Solution**: Run migrations in startup or seed method:
```csharp
await context.Database.MigrateAsync();
```

---

## Testing with In-Memory Database

```csharp
[TestFixture]
public class CopilotRepositoryTests
{
	private CopilotStudioDbContext _context;
	private ICopilotRepository _repository;

	[SetUp]
	public void Setup()
	{
		var options = new DbContextOptionsBuilder<CopilotStudioDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new CopilotStudioDbContext(options);
		_repository = new CopilotRepository(_context);
	}

	[Test]
	public async Task GetByIdAsync_WithValidId_ReturnsEntity()
	{
		// Arrange
		var copilot = new CopilotApplication 
		{ 
			CopilotId = "test-1",
			Name = "Test Copilot"
		};
		_context.CopilotApplications.Add(copilot);
		await _context.SaveChangesAsync();

		// Act
		var result = await _repository.GetByIdAsync("test-1");

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual("Test Copilot", result.Name);
	}
}
```

---

## Performance Optimization Tips

1. **Use AsNoTracking() for read-only queries**:
   ```csharp
   var copilots = await _context.CopilotApplications
	   .AsNoTracking()
	   .ToListAsync();
   ```

2. **Use Select() to project only needed columns**:
   ```csharp
   var names = await _context.CopilotApplications
	   .Select(c => new { c.CopilotId, c.Name })
	   .ToListAsync();
   ```

3. **Add indexes for frequently filtered columns**:
   ```csharp
   entity.HasIndex(e => e.Name);
   entity.HasIndex(e => e.IsActive);
   ```

4. **Use pagination for large result sets**:
   ```csharp
   var page = await _context.CopilotApplications
	   .Skip((pageNumber - 1) * pageSize)
	   .Take(pageSize)
	   .ToListAsync();
   ```

5. **Include related entities only when needed**:
   ```csharp
   var copilot = await _context.CopilotApplications
	   .Include(c => c.Versions)
	   .FirstOrDefaultAsync(c => c.CopilotId == id);
   ```

---

## Support & Documentation

- **EF Core Docs**: https://docs.microsoft.com/en-us/ef/core/
- **Connection Strings**: https://www.connectionstrings.com/
- **Migration Guide**: See `ENTITY_FRAMEWORK_MIGRATION_CHECKLIST.md`
- **Facet Mapping**: See `DTOLayer/FACET_MAPPING_GUIDE.md`

---

## Version History

- **v1.0** (Current): Centralized DbContexts for CopilotStudio and MapApp
- **v1.1** (Planned): Facet maps for all remaining DbContexts
- **v1.2** (Planned): Query optimization and performance profiling
