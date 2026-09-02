# CopilotStudioDbContext Integration Checklist

## ✅ Phase 1: Entity Models & DbContext (COMPLETED)

### Core Entities Created
- [x] CopilotApplication.cs - Primary aggregate (80 lines)
- [x] CopilotModelConfiguration.cs - LLM configuration (45 lines)
- [x] KnowledgeTool.cs - Knowledge sources (60 lines)
- [x] CopilotGovernancePolicy.cs - Governance & compliance (60 lines)
- [x] CopilotPerformanceMetrics.cs - Observability (55 lines)
- [x] CopilotDeploymentConfig.cs - Deployment settings (70 lines)
- [x] CopilotVersion.cs - Version history (75 lines)

### DbContext Updates
- [x] Import changed from `TubieTools_Aspire...Models` → `DataAccessLayer.Data.Entities`
- [x] All DbSets properly defined
- [x] OnModelCreating configuration complete
- [x] Relationships configured (1:1 SetNull, 1:Many Cascade)
- [x] Indexes created (Name, LandingZone, Status, Environment)
- [x] JSON property mappings
- [x] Default values for audit fields

### Documentation Created
- [x] IMPLEMENTATION_SUMMARY.md
- [x] ENTITY_RELATIONSHIP_DIAGRAM.md
- [x] MIGRATION_GUIDE.md
- [x] COPILOT_STUDIO_ENTITIES_README.md
- [x] README.md (in Entities folder)

---

## 🔄 Phase 2: EF Core Migration (NEXT STEP)

### Pre-Migration Tasks
- [ ] Ensure DataAccessLayer project compiles without errors
- [ ] Verify all entity classes are in correct namespace
- [ ] Check that DbContext references are correct
- [ ] Browse to solution directory

### Generate Migration
```powershell
# In Package Manager Console
Add-Migration CreateCopilotStudioEntities -Project DataAccessLayer
```

**Expected Outputs:**
- [ ] New migration file in `DataAccessLayer\Migrations\`
- [ ] Migration timestamp format: `<timestamp>_CreateCopilotStudioEntities.cs`
- [ ] No compiler errors

### Review Migration
- [ ] Verify all 7 tables in migration
- [ ] Check primary keys are GUID
- [ ] Verify foreign keys with ON DELETE behavior
- [ ] Confirm indexes on Name, LandingZone, Status
- [ ] Check DEFAULT constraints for timestamps

### Apply Migration
```powershell
Update-Database -Project DataAccessLayer
```

**Expected Results:**
- [ ] "Applying migration..." message
- [ ] "Done." success message
- [ ] No errors in Package Manager Console

### Verify Database
```sql
-- In SQL Server Management Studio
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'Copilot%';
-- Expected: 7 tables
```

---

## 🏗️ Phase 3: Solution Build & Compilation

### Project Build
```bash
cd <SolutionRoot>
dotnet clean
dotnet build
```

**Success Criteria:**
- [ ] No compilation errors in any project
- [ ] No compiler warnings about namespaces
- [ ] All project references resolve correctly
- [ ] DataAccessLayer.csproj recognizes all entities

### Specific Checks
- [ ] No "CS0246: The type or namespace name 'X' could not be found" errors
- [ ] No null reference issues at compile time
- [ ] All using statements resolve correctly
- [ ] No circular dependency warnings

### Test Project Compilation
```bash
dotnet build DataAccessLayer/DataAccessLayer.csproj
```

---

## 🧪 Phase 4: Unit Tests

### Create Test Data Factory
- [ ] Create builder pattern for CopilotApplication
- [ ] Create builder pattern for KnowledgeTool
- [ ] Create builder pattern for CopilotVersion
- [ ] Test JSON serialization/deserialization

### Test Database Context
```csharp
[Test]
public async Task CreateCopilotApplication_Succeeds()
{
	var copilot = new CopilotApplication 
	{ 
		CopilotId = Guid.NewGuid().ToString(),
		Name = "Test Copilot",
		LandingZone = "Test-Zone"
	};

	context.CopilotApplications.Add(copilot);
	var result = await context.SaveChangesAsync();

	Assert.That(result, Is.EqualTo(1));
}
```

**Tests to Implement:**
- [ ] Create entity without relationships
- [ ] Create entity with 1:1 relationships
- [ ] Create entity with 1:Many relationships
- [ ] Update JSON fields
- [ ] Cascade delete
- [ ] Load with eager loading (Include)
- [ ] Filter by IsActive
- [ ] Query by LandingZone

---

## 📦 Phase 5: DTOLayer Facet Maps

### Create Facet Map Files
Location: `DTOLayer/FacetMaps/CopilotStudio/`

- [ ] CopilotApplicationFacetMap.cs
- [ ] CopilotModelConfigurationFacetMap.cs
- [ ] KnowledgeToolFacetMap.cs
- [ ] CopilotGovernancePolicyFacetMap.cs
- [ ] CopilotPerformanceMetricsFacetMap.cs
- [ ] CopilotDeploymentConfigFacetMap.cs
- [ ] CopilotVersionFacetMap.cs

### Each Facet Map Must Include
```csharp
public class CopilotApplicationFacetMap
{
	public static CopilotApplicationDto FromEntity(CopilotApplication entity)
	{
		// Map entity to safe DTO, excluding sensitive fields
	}

	public static CopilotApplication ToEntity(CopilotApplicationDto dto)
	{
		// Map DTO to entity
	}
}
```

**Mapping Responsibilities:**
- [ ] Exclude internal/sensitive fields from DTOs
- [ ] Include audit trail fields (CreatedDate, etc.)
- [ ] Handle JSON deserialization for complex objects
- [ ] Safely handle null references
- [ ] Version the API models

---

## 🔌 Phase 6: Service Layer Integration

### Create Repository Services
- [ ] ICopilotApplicationRepository interface
- [ ] Implementation with CRUD operations
- [ ] Include advanced queries (ByLandingZone, ByStatus, etc.)
- [ ] Implement async/await pattern

### Create Application Services
- [ ] CopilotApplicationService with business logic
- [ ] Dependency injection setup
- [ ] Error handling and logging
- [ ] Transaction management

### Service Implementation Example
```csharp
public interface ICopilotApplicationService
{
	Task<CopilotApplicationDto> CreateAsync(CreateCopilotApplicationDto dto);
	Task<CopilotApplicationDto> GetByIdAsync(string copilotId);
	Task<IEnumerable<CopilotApplicationDto>> GetByLandingZoneAsync(string landingZone);
	Task UpdateAsync(string copilotId, UpdateCopilotApplicationDto dto);
	Task DeleteAsync(string copilotId);
}
```

**Tests for Services:**
- [ ] Create happy path test
- [ ] Test validation errors
- [ ] Test not found scenario
- [ ] Test concurrent updates
- [ ] Test transaction rollback

---

## 📱 Phase 7: API Endpoints

### Create Controllers
- [ ] CopilotApplicationsController
- [ ] Endpoints: Get, GetAll, Create, Update, Delete
- [ ] Apply facet maps for request/response
- [ ] Add authorization/authentication

### API Endpoints
- [ ] GET /api/copilots/{copilotId}
- [ ] GET /api/copilots?landingZone={zone}
- [ ] POST /api/copilots
- [ ] PUT /api/copilots/{copilotId}
- [ ] DELETE /api/copilots/{copilotId}

### OpenAPI/Swagger
- [ ] Update Swagger documentation
- [ ] Add parameter descriptions
- [ ] Add response schemas
- [ ] Add example requests/responses

### Tests
- [ ] Integration tests for endpoints
- [ ] HTTP status code verification
- [ ] JSON serialization/deserialization
- [ ] Error response format

---

## 🔐 Phase 8: Security & Compliance

### Data Protection
- [ ] EnvironmentVariables encryption/decryption
- [ ] PII data masking (emails in CopilotApplications)
- [ ] Audit trail for sensitive changes
- [ ] Access control by LandingZone

### Validation
- [ ] Email format validation
- [ ] Required field checks
- [ ] Name uniqueness validation
- [ ] Enum value validation (EnforcementMode, Environment)

### Compliance
- [ ] GDPR: Right to delete (soft delete via IsActive)
- [ ] Audit Trail: All creations/updates tracked with timestamps
- [ ] Data Residency: Captured in CopilotGovernancePolicy
- [ ] Encryption: EnvironmentVariables stored encrypted

---

## 📊 Phase 9: Performance & Optimization

### Database Optimization
- [ ] Review query plans for complex searches
- [ ] Test pagination with large datasets
- [ ] Verify index usage statistics
- [ ] Add missing indexes if needed

### Caching Strategy
- [ ] Redis cache for frequently accessed policies
- [ ] ETag support for API responses
- [ ] Cache policy by governance requirements

### Performance Tests
- [ ] Load test: 1000+ concurrent reads
- [ ] Stress test: 100+ concurrent writes
- [ ] Benchmark query performance
- [ ] Memory profiling

---

## 🚀 Phase 10: Deployment Preparation

### Environment Configuration
- [ ] Dev environment: DbContext configured
- [ ] Staging environment: Connection string setup
- [ ] Production environment: Backups configured
- [ ] Disaster recovery plan: Documentation

### Documentation
- [ ] Entity relationship diagram (COMPLETED)
- [ ] API documentation (Swagger)
- [ ] Database schema documentation
- [ ] Deployment runbook

### Release Checklist
- [ ] Code review completed
- [ ] All tests passing (unit + integration)
- [ ] Performance benchmarks acceptable
- [ ] Security audit completed
- [ ] Documentation complete
- [ ] Backup strategy verified
- [ ] Rollback plan documented

---

## 📋 Deployment Gates

### Pre-Production Sign-Off
- [ ] **Architecture Review** - COMPLETED ✓
- [ ] **Code Quality** - Pending
- [ ] **Security** - Pending
- [ ] **Performance** - Pending
- [ ] **Operations** - Pending

### Production Deployment
- [ ] Database migration successful
- [ ] Zero data loss verification
- [ ] Rollback procedure tested
- [ ] Monitoring/alerts configured
- [ ] Support team trained

---

## Quick Reference: Commands

### Build & Test
```bash
dotnet clean
dotnet build
dotnet test
```

### Database Migration
```powershell
# In Package Manager Console
Add-Migration CreateCopilotStudioEntities -Project DataAccessLayer
Update-Database -Project DataAccessLayer
```

### Rollback (if needed)
```powershell
Update-Database -Verbose -Project DataAccessLayer
Remove-Migration -Project DataAccessLayer -Force
```

### Verify Database
```sql
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE 'Copilot%';
```

---

## 🎯 Success Metrics

| Metric | Target | Status |
|--------|--------|--------|
| Entities Created | 7 | ✅ Complete |
| DbContext Updated | 1 | ✅ Complete |
| Migration Created | Yes | ⏳ Pending |
| Database Tables | 7 | ⏳ Pending |
| Unit Tests | 50+ | ⏳ Pending |
| Integration Tests | 20+ | ⏳ Pending |
| API Endpoints | 5+ | ⏳ Pending |
| Documentation | 100% | ✅ 80% Complete |

---

## 📞 Contact & Escalation

### Support Points
- **Architecture Questions:** Review `IMPLEMENTATION_SUMMARY.md`
- **Database Schema:** Check `ENTITY_RELATIONSHIP_DIAGRAM.md`
- **Migration Issues:** Follow `MIGRATION_GUIDE.md`
- **Entity Details:** See individual entity README files

### Escalation Path
1. Check documentation
2. Review entity code comments
3. Run diagnostic queries
4. Escalate to architecture team

---

**Checklist Version:** 1.0  
**Last Updated:** Current Session  
**Status:** Phase 1 Complete, Ready for Phase 2  
**Next Step:** Run `Add-Migration CreateCopilotStudioEntities`

---

> **Note:** This checklist is comprehensive. Organizations may choose to parallelize some phases, skip non-essential items, or adapt to their specific needs. The critical path is: Phase 1 ✓ → Phase 2 → Phase 4 → Phase 6 → Phase 10.

