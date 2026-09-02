# CopilotStudioDbContext - Entity Relationship Diagram

## Database Schema Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CopilotStudioDbContext - SQL Server                    │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────┐
│      CopilotApplications Table       │
├──────────────────────────────────────┤
│ PK: CopilotId (GUID)                 │
│ - Name (255) [UNIQUE INDEX]          │
│ - Description (2000)                 │
│ - LandingZone (100) [INDEX]          │
│ - BusinessObjective                  │
│ - PrimaryUseCase                     │
│ - TargetAudience                     │
│ - MaturityLevel                      │
│ - Capabilities (JSON)                │
│ - CurrentVersion                     │
│ - Owner                              │
│ - ContactEmail                       │
│ - ModelConfigurationId (FK, NULL)    │
│ - GovernancePolicyId (FK, NULL) ─────┐
│ - PerformanceMetricsId (FK, NULL) ─┐ │
│ - DeploymentConfigId (FK, NULL) ─┐ │ │
│ - GuidelinesAdherence (JSON)     │ │ │ 
│ - CreatedDate (DEFAULT GETUTCDATE)  │ │ │
│ - LastModifiedDate (DEFAULT)        │ │ │
│ - IsActive (DEFAULT true) [INDEX]   │ │ │
└──────────────────────────────────────┘ │ │ │
		   ▲           │ │ 1:Many         │ │ │
		   │ 1:1        │ │  ┌────────────┐ │ │
		   │            │ │  │            │ │ │
		   │            │ │  │            │ │ │
		   │            │ │  │ ┌──────────┘ │ │
		   │            ▼ ▼  ▼ │            │ │
		   │
	 ┌─────────────────────────────────────────┐
	 │   CopilotModelConfiguration Table       │
	 ├─────────────────────────────────────────┤
	 │ PK: ConfigId (GUID)                     │
	 │ - ModelProvider (100) [INDEX]           │
	 │ - ModelName (255)                       │
	 │ - ModelVersion (100)                    │
	 │ - Temperature (decimal 0-1)             │
	 │ - TopP (decimal 0-1)                    │
	 │ - MaxTokens (int)                       │
	 │ - SystemPrompt (3000)                   │
	 │ - CustomParameters (JSON)               │
	 │ - SafetySettings (JSON)                 │
	 │ - ContextWindowSize                     │
	 │ - SupportsFunctionCalling               │
	 │ - CreatedDate, LastModifiedDate         │
	 └─────────────────────────────────────────┘


┌─────────────────────────────────────────┐
│    CopilotGovernancePolicy Table        │◄─┐
├─────────────────────────────────────────┤  │
│ PK: PolicyId (GUID)                     │  │ 1:Many
│ - LandingZone (100) [INDEX]             │  │
│ - PolicyName (255)                      │  │
│ - Description (2000)                    │  │
│ - DataResidency (JSON)                  │  │
│ - SecurityRequirements (JSON)           │  │
│ - ComplianceRequirements (JSON Array)   │  │
│ - DataHandling (JSON)                   │  │
│ - ModelGovernance (JSON)                │  │
│ - AuditRequirements (JSON)              │  │
│ - CostManagement (JSON)                 │  │
│ - IncidentResponse (JSON)               │  │
│ - EnforcementMode (50)                  │  │
│ - RequiresAttestation (bool)            │  │
│ - LastReviewDate                        │  │
│ - NextReviewDate                        │  │
│ - CreatedDate, LastModifiedDate ────────┘
│ - IsActive [INDEX]
└─────────────────────────────────────────┘


┌─────────────────────────────────────────┐
│ CopilotPerformanceMetrics Table         │◄─┐
├─────────────────────────────────────────┤  │
│ PK: MetricsId (GUID)                    │  │ 1:1
│ FK: CopilotId (INDEX)                   │  │
│ - AvgResponseTimeMs                     │  │
│ - P95ResponseTimeMs                     │  │
│ - P99ResponseTimeMs                     │  │
│ - TotalInvocations                      │  │
│ - SuccessfulInvocations                 │  │
│ - FailedInvocations                     │  │
│ - AvgTokensUsed                         │  │
│ - TotalCost                             │  │
│ - AvgCostPerInvocation                  │  │
│ - UserSatisfactionRating (0-100)        │  │
│ - ErrorRate (0-100)                     │  │
│ - UptimePercentage (0-100)              │  │
│ - DetailedMetrics (JSON)                │  │
│ - LastUpdated (DEFAULT GETUTCDATE)      │  │
│ - CreatedDate, LastModifiedDate ────────┘
└─────────────────────────────────────────┘


┌──────────────────────────────────────────┐
│  CopilotDeploymentConfig Table           │◄─┐
├──────────────────────────────────────────┤  │
│ PK: ConfigId (GUID)                      │  │ 1:1
│ FK: CopilotId                            │  │
│ - Environment (50) [INDEX]               │  │
│ - DeploymentEndpoint (500)               │  │
│ - DeploymentRegion (100)                 │  │
│ - ContainerRegistry (100)                │  │
│ - ImageTag (100)                        │  │
│ - ScalingConfig (JSON)                  │  │
│ - ResourceAllocation (JSON)             │  │
│ - HealthCheck (JSON)                    │  │
│ - LoadBalancing (JSON)                  │  │
│ - SecurityConfig (JSON)                 │  │
│ - EnvironmentVariables (JSON, encrypted)│  │
│ - DeploymentStatus (50) [INDEX]         │  │
│ - DeployedDate                          │  │
│ - LastHealthCheckDate                   │  │
│ - RollbackInfo (JSON)                   │  │
│ - FeatureFlags (JSON)                   │  │
│ - IsProductionReady                     │  │
│ - CreatedDate, LastModifiedDate ────────┘
└──────────────────────────────────────────┘


┌─────────────────────────────────────────────┐
│        KnowledgeTools Table                 │
├─────────────────────────────────────────────┤
│ PK: ToolId (GUID)                           │
│ FK: CopilotApplicationId [INDEX] ───────────┐
│ - Name (255) [INDEX]                        │ 1:Many (Cascade Delete)
│ - Description (1000)                        │
│ - Pattern (100)                             │
│ - DataSourceConfig (JSON)                   │
│ - RetrievalConfig (JSON)                    │
│ - EmbeddingConfig (JSON)                    │
│ - ContextWindowSize (default 2000)          │
│ - RelevanceThreshold (decimal 0-1)          │
│ - MaxResults (int)                          │
│ - CacheConfig (JSON)                        │
│ - AccessControl (JSON)                      │
│ - FreshnessRequirement (50)                 │
│ - PerformanceMetrics (JSON)                 │
│ - IsEnabled                                 │
│ - CreatedDate, LastModifiedDate             │
└─────────────────────────────────────────────┘
		 ▲
		 │
		 └──────────────────────────┐
									│ 1:Many (Cascade Delete)
									│
┌─────────────────────────────────────────────────────────┐
│            CopilotVersions Table                        │
├─────────────────────────────────────────────────────────┤
│ PK: VersionId (GUID)                                    │
│ FK: CopilotId [INDEX] ←────────────────────┐            │
│ - VersionNumber (50) [INDEX]               │ 1:Many     │
│ - ReleaseNotes (2000)                      │ (Cascade   │
│ - ReleaseDate [INDEX]                      │  Delete)   │
│ - Changes (JSON Array<VersionChange>)      │            │
│ - BreakingChanges (JSON Array<string>)     │            │
│ - Deprecations (JSON Array<string>)        │            │
│ - RequiresMigration (bool)                 │            │
│ - IsBackwardCompatible (bool)              │            │
│ - PrereleaseName (100)                     │            │
│ - IsPrerelease (bool)                      │            │
│ - IsReleaseCandidate (bool)                │            │
│ - DeploymentInstructions (JSON)            │            │
│ - RollbackInstructions (JSON)              │            │
│ - ReleasedBy (255)                         │            │
│ - CreatedDate, LastModifiedDate ───────────┘
│ - IsActive (bool)
└─────────────────────────────────────────────────────────┘
```

## Entity Relationships Summary

### One-to-One Relationships (Optional)
```
CopilotApplication ──┐
					 ├──→ CopilotModelConfiguration
					 │    DELETE: SetNull (can exist independently)
					 │
					 ├──→ CopilotGovernancePolicy
					 │    DELETE: SetNull (policy applies to multiple copilots)
					 │
					 ├──→ CopilotPerformanceMetrics
					 │    DELETE: SetNull (metrics can be deleted independently)
					 │
					 └──→ CopilotDeploymentConfig
						  DELETE: SetNull (can be updated without copilot)
```

### One-to-Many Relationships (Required)
```
CopilotApplication ──┐
					 ├──→ KnowledgeTools (0..*)
					 │    DELETE: Cascade (delete copilot → delete tools)
					 │
					 └──→ CopilotVersions (0..*)
						  DELETE: Cascade (delete copilot → delete versions)
```

### Reverse Many-to-One
```
CopilotGovernancePolicy ──→ CopilotApplications (0..*)
							One policy can govern multiple copilots
							in same landing zone
```

## Key Design Patterns

### 1. JSON Serialization for Complex Objects
Instead of creating separate tables, we store complex nested objects as JSON:

**Benefit:** Schema flexibility + reduced normalization + single query retrieves complete aggregate

**Example:** CopilotApplication.Capabilities
```json
[
  "NaturalLanguageProcessing",
  "DocumentProcessing",
  "CodeGeneration",
  "ConversationalAI"
]
```

### 2. Audit Trail Fields (All Entities)
```
- CreatedDate       (DEFAULT: GETUTCDATE())
- LastModifiedDate  (DEFAULT: GETUTCDATE())
- IsActive          (DEFAULT: true) - Soft delete support
```

### 3. Unique and Non-Clustered Indexes
```
CopilotApplication:
  - Name (UNIQUE) - No two copilots with same name
  - LandingZone (INCLUDE) - Query by zone
  - IsActive (INCLUDE) - Filter active copilots

CopilotDeploymentConfig:
  - Environment (INCLUDE) - Filter by env
  - DeploymentStatus (INCLUDE) - Monitor deployments
```

### 4. JSON Arrays for Version Changes
```csharp
// CopilotVersion.Changes as JSON
[
  {
	"ChangeId": "guid",
	"Category": "Feature",           // Feature|BugFix|Performance|Security
	"Description": "Added RAG integration",
	"ImpactLevel": "High",           // Critical|High|Medium|Low
	"ModifiedDate": "2024-01-15"
  }
]
```

## NULL-Safe Defaults

All collection navigation properties initialize to empty lists to prevent null reference issues:

```csharp
public virtual ICollection<KnowledgeTool> KnowledgeTools { get; set; } 
	= new List<KnowledgeTool>();

public virtual ICollection<CopilotVersion> VersionHistory { get; set; } 
	= new List<CopilotVersion>();
```

## Performance Considerations

### Indexes Defined
- Unique index on CopilotApplication.Name (query by name)
- Covering indexes on LandingZone, IsActive (filtering)
- Foreign key indexes for joins (KnowledgeTools, Versions)
- Status indexes on DeploymentConfig (monitoring queries)

### JSON Query Support
For SQL Server 2016+, JSON functions support efficient querying:
```sql
-- Find copilots with specific capability
SELECT * FROM CopilotApplications
WHERE JSON_VALUE(Capabilities, '$[0]') = 'NaturalLanguageProcessing'

-- Get compliance requirement
SELECT * FROM CopilotGovernancePolicies
WHERE JSON_VALUE(ComplianceRequirements, '$.RegulationName') = 'GDPR'
```

---

**Generated:** Current Session  
**Database:** SQL Server (2016+)  
**EF Core:** Version 10.0.x
