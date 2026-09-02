# CopilotStudioDbContext - Migration & Next Steps Guide

## 🎯 What Was Just Created

### Entity Models (7 files)
1. **CopilotApplication.cs** - Primary aggregate root
2. **CopilotModelConfiguration.cs** - LLM configuration
3. **KnowledgeTool.cs** - Knowledge/retrieval sources
4. **CopilotGovernancePolicy.cs** - Enterprise governance
5. **CopilotPerformanceMetrics.cs** - Observability metrics
6. **CopilotDeploymentConfig.cs** - Deployment settings
7. **CopilotVersion.cs** - Version & release history

### DbContext Updates
- **CopilotStudioDbContext.cs** - Updated imports and OnModelCreating configuration
- All entities properly configured with:
  - Primary keys and indexes
  - Foreign key relationships with cascade/null behaviors
  - JSON string mappings for complex fields
  - Default values and SQL defaults
  - Navigation properties

### Documentation
- **IMPLEMENTATION_SUMMARY.md** - Complete feature overview
- **ENTITY_RELATIONSHIP_DIAGRAM.md** - Schema visualization
- **COPILOT_STUDIO_ENTITIES_README.md** - Detailed entity documentation

---

## 🔄 Next Steps: Create EF Core Migration

### Step 1: Open Package Manager Console

In Visual Studio, open **Package Manager Console** (Tools → NuGet Package Manager → Package Manager Console)

### Step 2: Generate Initial Migration

Run the following command:

```powershell
Add-Migration CreateCopilotStudioEntities -Project DataAccessLayer
```

**Expected Output:**
```
Build succeeded.
To undo this action, use Remove-Migration.
```

This creates `DataAccessLayer\Migrations\<timestamp>_CreateCopilotStudioEntities.cs`

### Step 3: Review Generated Migration

Open the migration file and verify:
- ✅ All 7 tables are created
- ✅ Primary keys are GUIDs
- ✅ Foreign keys with correct ON DELETE behavior
- ✅ Indexes on Name, LandingZone, Status fields
- ✅ DEFAULT constraints for timestamps (GETUTCDATE())

**Key Generated SQL:**
```sql
CREATE TABLE [CopilotApplications] (
	[CopilotId] NVARCHAR(36) NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[Description] NVARCHAR(2000) NULL,
	[LandingZone] NVARCHAR(100) NOT NULL,
	[ModelConfigurationId] NVARCHAR(36) NULL,
	[GovernancePolicyId] NVARCHAR(36) NULL,
	[PerformanceMetricsId] NVARCHAR(36) NULL,
	[DeploymentConfigId] NVARCHAR(36) NULL,
	[CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
	[LastModifiedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
	[IsActive] BIT NOT NULL DEFAULT 1,
	CONSTRAINT [PK_CopilotApplications] PRIMARY KEY ([CopilotId]),
	CONSTRAINT [FK_CopilotApplications_CopilotModelConfigurations] 
		FOREIGN KEY ([ModelConfigurationId]) REFERENCES [CopilotModelConfigurations] ([ConfigId]) ON DELETE SET NULL,
	...
);

CREATE UNIQUE INDEX [IX_CopilotApplications_Name] ON [CopilotApplications] ([Name]);
CREATE INDEX [IX_CopilotApplications_LandingZone] ON [CopilotApplications] ([LandingZone]);
```

### Step 4: Apply Migration to Database

```powershell
Update-Database -Project DataAccessLayer
```

**Expected Output:**
```
Applying migration '20240115120000_CreateCopilotStudioEntities'.
Done.
```

### Step 5: Verify Database Schema

Connect to SQL Server and verify:

```sql
-- Check tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'Copilot%';

-- Check indexes
SELECT TABLE_NAME, INDEX_NAME 
FROM INFORMATION_SCHEMA.STATISTICS 
WHERE TABLE_NAME LIKE 'Copilot%' AND INDEX_Name IS NOT NULL;

-- Sample query
SELECT TOP 5 * FROM CopilotApplications;
```

---

## 🏗️ Build & Compile Check

After migration, build the solution to ensure everything compiles:

```bash
# Clean build
dotnet clean
dotnet build

# OR in Package Manager Console
# Make sure no compile errors
```

**Expected Issues to Address:**

1. **Missing VersionChange in DbContext**
   - Solution: The VersionChange class is in CopilotVersion.cs, EF Core handles JSON serialization automatically

2. **Namespace Conflicts**
   - Solution: All entities are in `DataAccessLayer.Data.Entities`
   - Check that no other projects have conflicting type names

3. **JSON Serialization**
   - Solution: EF Core 10.0+ handles JSON string fields automatically
   - No explicit value converters needed for basic JSON strings

---

## 📊 Database Schema Validation

### Table Verification Queries

```sql
-- Verify all tables
SELECT name FROM sys.tables WHERE name LIKE 'Copilot%';
-- Expected: 7 tables

-- Verify foreign keys
SELECT OBJECT_NAME(constid) AS ForeignKey,
	   OBJECT_NAME(fkeyid) AS ParentTable,
	   OBJECT_NAME(rkeyid) AS ReferencedTable
FROM sys.sysforeignkeys
WHERE OBJECT_NAME(rkeyid) LIKE 'Copilot%'
   OR OBJECT_NAME(fkeyid) LIKE 'Copilot%';

-- Verify indexes exist
SELECT TABLE_NAME, INDEX_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_NAME LIKE 'Copilot%'
ORDER BY TABLE_NAME, INDEX_NAME;

-- Check default values
SELECT OBJECT_NAME(OBJECT_ID) AS TableName,
	   name AS ColumnName,
	   default_object_id
FROM sys.columns
WHERE OBJECT_ID IN (
	SELECT OBJECT_ID FROM sys.tables 
	WHERE name LIKE 'Copilot%'
) AND default_object_id != 0;
-- Expected: CreatedDate, LastModifiedDate, IsActive defaults
```

---

## 🔧 Troubleshooting

### Issue: Migration Won't Apply

**Symptom:** Error: "There is already an open transaction"

**Solution:**
```powershell
# Close existing connections, then:
Update-Database -Project DataAccessLayer -Force
```

### Issue: Foreign Key Constraint Fails

**Symptom:** "The DELETE statement conflicted with a FOREIGN KEY constraint"

**Solution:** Check delete cascade behavior in DbContext:
- Ensure `OnDelete(DeleteBehavior.SetNull)` for optional references
- Ensure `OnDelete(DeleteBehavior.Cascade)` for owned collections

### Issue: JSON Column Data Missing

**Symptom:** JSON properties return null or empty strings

**Solution:** 
- Ensure application initializes JSON fields before saving:
```csharp
var copilot = new CopilotApplication 
{ 
	Capabilities = "[]", // Empty JSON array
	GuidelinesAdherence = "{}", // Empty JSON object
};
```

### Issue: Null Reference Exception on Navigation Properties

**Symptom:** "Object reference not set to an instance"

**Solution:** All navigation properties auto-initialize:
```csharp
// This is automatic in entity definitions
public virtual ICollection<KnowledgeTool> KnowledgeTools 
{ 
	get; 
	set; 
} = new List<KnowledgeTool>();
```

---

## 🎓 Entity Usage Examples

### Creating a New Copilot Application

```csharp
var copilot = new CopilotApplication
{
	CopilotId = Guid.NewGuid().ToString(),
	Name = "HR Assistant Copilot",
	Description = "Handles HR policy inquiries and leave requests",
	LandingZone = "HR-Zone-1",
	MaturityLevel = "Production",
	Capabilities = "[\"QuestionAnswering\",\"DocumentProcessing\",\"ApprovalWorkflows\"]",
	Owner = "HR Team",
	ContactEmail = "hr-copilot@company.com",
	IsActive = true
};

await context.CopilotApplications.AddAsync(copilot);
await context.SaveChangesAsync();
```

### Querying with Relationships

```csharp
// Load copilot with all related entities
var copilot = await context.CopilotApplications
	.Include(c => c.ModelConfiguration)
	.Include(c => c.GovernancePolicy)
	.Include(c => c.PerformanceMetrics)
	.Include(c => c.KnowledgeTools)
	.Include(c => c.VersionHistory)
	.FirstOrDefaultAsync(c => c.Name == "HR Assistant Copilot");
```

### Updating Performance Metrics

```csharp
var metrics = await context.PerformanceMetrics
	.FirstOrDefaultAsync(m => m.CopilotId == copilotId);

metrics.AvgResponseTimeMs = 1250m;
metrics.TotalInvocations = 5432;
metrics.SuccessfulInvocations = 5401;
metrics.ErrorRate = ((5432 - 5401) / 5432) * 100;
metrics.LastUpdated = DateTime.UtcNow;

context.PerformanceMetrics.Update(metrics);
await context.SaveChangesAsync();
```

### Versioning

```csharp
var version = new CopilotVersion
{
	VersionId = Guid.NewGuid().ToString(),
	CopilotId = copilotId,
	VersionNumber = "1.2.0",
	ReleaseDate = DateTime.UtcNow,
	ReleaseNotes = "Added RAG capabilities",
	Changes = JsonSerializer.Serialize(new List<VersionChange> 
	{
		new()
		{
			ChangeId = Guid.NewGuid().ToString(),
			Category = "Feature",
			Description = "Integrated vector search",
			ImpactLevel = "High",
			ModifiedDate = DateTime.UtcNow
		}
	}),
	IsBackwardCompatible = true,
	RequiresMigration = false
};

await context.Versions.AddAsync(version);
await context.SaveChangesAsync();
```

---

## 📋 Pre-Deployment Checklist

Before deploying to production:

- [ ] Migration applies successfully to dev database
- [ ] All entities load without null reference exceptions
- [ ] Foreign key relationships work correctly
- [ ] JSON serialization/deserialization works as expected
- [ ] Pagination works with large datasets
- [ ] Unit tests pass for DataAccessLayer
- [ ] Documentation is updated
- [ ] Performance tests for complex queries pass
- [ ] Backup strategy for production database is in place
- [ ] Connection string is properly configured

---

## 📚 Additional Resources

### EF Core Documentation
- [Entity Framework Core - JSON Columns](https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions#json-columns)
- [Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships)
- [Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations)

### SQL Server
- [JSON Support in SQL Server](https://docs.microsoft.com/en-us/sql/relational-databases/json/json-data-sql-server)
- [Indexes Best Practices](https://docs.microsoft.com/en-us/sql/relational-databases/indexes/indexes)

---

## ✅ Success Criteria

After completing all steps, verify:

1. **Database Tables Created:** 7 tables exist in SQL Server
2. **Relationships Configured:** All foreign keys with correct delete behaviors
3. **Indexes Created:** Covering indexes on frequently queried columns
4. **Default Values:** Audit fields have SQL defaults
5. **Solution Compiles:** No compile errors or warnings
6. **No Circular Dependencies:** All dependencies resolve correctly
7. **Tests Pass:** Unit tests for entity creation succeed

---

**Generated:** Current Session  
**Status:** Ready for Migration  
**Priority:** Check Entity Compilation First

Next Action: Run `Add-Migration CreateCopilotStudioEntities` in Package Manager Console
