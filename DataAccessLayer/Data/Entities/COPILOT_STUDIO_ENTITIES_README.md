# CopilotStudioDbContext Entity Models

## Overview
This document describes the entity models created in `DataAccessLayer/Data/Entities/` for the `CopilotStudioDbContext`. All entity models are now centralized in the DataAccessLayer following the layered architecture pattern.

## Entity Models

### 1. CopilotApplication
**Table:** CopilotApplications  
**Location:** `DataAccessLayer/Data/Entities/CopilotApplication.cs`

**Purpose:** Primary aggregate representing a Copilot deployment with complete governance and lifecycle management.

**Key Properties:**
- `CopilotId` (PK, string, 36 chars) - Unique identifier
- `Name` (required, string, 255 chars) - Human-readable name
- `Description` (string, 2000 chars) - Detailed description
- `LandingZone` (required, string, 100 chars) - Enterprise landing zone
- `BusinessObjective`, `PrimaryUseCase`, `TargetAudience` - Context metadata
- `MaturityLevel`, `CurrentVersion` - Version tracking
- `Owner`, `ContactEmail` - Ownership and contact information
- `CreatedDate`, `LastModifiedDate`, `IsActive` - Audit fields

**Foreign Keys:**
- `ModelConfigurationId` → CopilotModelConfiguration
- `GovernancePolicyId` → CopilotGovernancePolicy
- `PerformanceMetricsId` → CopilotPerformanceMetrics
- `DeploymentConfigId` → CopilotDeploymentConfig

**Navigation Properties:**
- `ModelConfiguration` (1:1)
- `GovernancePolicy` (1:1)
- `PerformanceMetrics` (1:1)
- `DeploymentConfig` (1:1)
- `KnowledgeTools` (1:Many)
- `VersionHistory` (1:Many)

---

### 2. CopilotModelConfiguration
**Table:** CopilotModelConfigurations  
**Location:** `DataAccessLayer/Data/Entities/CopilotModelConfiguration.cs`

**Purpose:** Stores LLM provider settings, model selection, and inference parameters.

**Key Properties:**
- `ConfigId` (PK, string, 36 chars)
- `ModelProvider` (string, 100 chars) - OpenAI, Anthropic, Custom, etc.
- `ModelName` (required, string, 255 chars) - e.g., "gpt-4-turbo"
- `Temperature`, `TopP` (decimal, 0-1 range) - Sampling parameters
- `MaxTokens` (int, default 2000) - Response limit
- `SystemPrompt` (string, 3000 chars) - Model instruction prompt
- `CustomParameters` (JSON string) - Provider-specific settings
- `SafetySettings` (JSON string) - Content filtering and guardrails
- `ContextWindowSize` (int) - Max input tokens
- `SupportsFunctionCalling` (bool) - Tool use capability

**JSON Serialized Fields:**
- `CustomParameters` - Dictionary<string, object>
- `SafetySettings` - Custom safety configuration

---

### 3. KnowledgeTool
**Table:** KnowledgeTools  
**Location:** `DataAccessLayer/Data/Entities/KnowledgeTool.cs`

**Purpose:** Represents retrieval and context sources for the Copilot (RAG, vector search, structured queries).

**Key Properties:**
- `ToolId` (PK, string, 36 chars)
- `Name` (required, string, 255 chars)
- `Pattern` (string, 100 chars) - VectorSearch, RAG, StructuredQuery, etc.
- `CopilotApplicationId` (FK) - Parent Copilot
- `ContextWindowSize` (int, default 2000)
- `RelevanceThreshold` (decimal, 0-1) - Minimum match score
- `MaxResults` (int, default 5) - Top-K results
- `FreshnessRequirement` (string) - RealTime, Daily, Weekly, Monthly
- `IsEnabled` (bool)

**JSON Serialized Fields:**
- `DataSourceConfig` - Data source type and location
- `RetrievalConfig` - Search/retrieval strategy
- `EmbeddingConfig` - Vector embedding settings
- `CacheConfig` - Performance caching
- `AccessControl` - Security and permissions
- `PerformanceMetrics` - Usage and performance data

**Navigation:**
- `CopilotApplication` (1:Many reverse)

---

### 4. CopilotGovernancePolicy
**Table:** CopilotGovernancePolicies  
**Location:** `DataAccessLayer/Data/Entities/CopilotGovernancePolicy.cs`

**Purpose:** Encodes enterprise compliance, security, and data residency requirements aligned with landing zones.

**Key Properties:**
- `PolicyId` (PK, string, 36 chars)
- `LandingZone` (required, string, 100 chars) - Applicable zone
- `PolicyName` (required, string, 255 chars)
- `Description` (string, 2000 chars)
- `EnforcementMode` (string, default "Strict") - Strict/Moderate/Advisory
- `RequiresAttestation` (bool) - Attestation needed
- `LastReviewDate`, `NextReviewDate` - Compliance review cycle

**JSON Serialized Fields (Enterprise Compliance):**
- `DataResidency` - Geographic/region requirements
- `SecurityRequirements` - Encryption, MFA, authentication
- `ComplianceRequirements` - GDPR, HIPAA, SOC2, NIST, etc.
- `DataHandling` - PII handling, anonymization
- `ModelGovernance` - AI model evaluation and bias
- `AuditRequirements` - Logging and audit trails
- `CostManagement` - Cost allocation and controls
- `IncidentResponse` - Escalation and incident procedures

**Navigation:**
- `CopilotApplications` (1:Many reverse)

---

### 5. CopilotPerformanceMetrics
**Table:** CopilotPerformanceMetrics  
**Location:** `DataAccessLayer/Data/Entities/CopilotPerformanceMetrics.cs`

**Purpose:** Captures observability metrics: latency, throughput, cost, quality, and uptime.

**Key Properties:**
- `MetricsId` (PK, string, 36 chars)
- `CopilotId` (FK, string, 36 chars)
- `AvgResponseTimeMs` (decimal)
- `P95ResponseTimeMs`, `P99ResponseTimeMs` (decimal) - Percentiles
- `TotalInvocations`, `SuccessfulInvocations`, `FailedInvocations` (long)
- `AvgTokensUsed`, `TotalCost`, `AvgCostPerInvocation` (decimal)
- `UserSatisfactionRating` (decimal, 0-100)
- `ErrorRate`, `UptimePercentage` (decimal, 0-100)
- `LastUpdated` (DateTime)

**JSON Serialized Fields:**
- `DetailedMetrics` - Additional observability data

---

### 6. CopilotDeploymentConfig
**Table:** CopilotDeploymentConfigs  
**Location:** `DataAccessLayer/Data/Entities/CopilotDeploymentConfig.cs`

**Purpose:** Infrastructure and deployment environment configuration.

**Key Properties:**
- `ConfigId` (PK, string, 36 chars)
- `CopilotId` (FK, string, 36 chars)
- `Environment` (required, string, 50 chars) - Dev, Staging, Prod
- `DeploymentEndpoint` (string, 500 chars) - API URL
- `DeploymentRegion` (string, 100 chars) - Cloud region
- `ContainerRegistry`, `ImageTag` (string) - Container details
- `DeploymentStatus` (string, 50 chars) - Pending, Active, Failed, etc.
- `DeployedDate`, `LastHealthCheckDate` (DateTime)
- `IsProductionReady` (bool)

**JSON Serialized Fields:**
- `ScalingConfig` - Auto-scale settings
- `ResourceAllocation` - CPU, memory, disk
- `HealthCheck` - Liveness/readiness probes
- `LoadBalancing` - Distribution strategy
- `SecurityConfig` - TLS/mTLS settings
- `EnvironmentVariables` - Encrypted config
- `RollbackInfo` - Rollback metadata
- `FeatureFlags` - Feature toggles

---

### 7. CopilotVersion
**Table:** CopilotVersions  
**Location:** `DataAccessLayer/Data/Entities/CopilotVersion.cs`

**Purpose:** Version history, release notes, and breaking change tracking.

**Key Properties:**
- `VersionId` (PK, string, 36 chars)
- `CopilotId` (FK, string, 36 chars)
- `VersionNumber` (required, string, 50 chars) - Semantic versioning
- `ReleaseNotes` (string, 2000 chars)
- `ReleaseDate` (required, DateTime)
- `RequiresMigration`, `IsBackwardCompatible` (bool)
- `IsPrerelease`, `IsReleaseCandidate` (bool)
- `ReleasedBy` (string, 255 chars)

**JSON Serialized Fields:**
- `Changes` - Array of VersionChange objects
- `BreakingChanges` - Array of breaking changes
- `Deprecations` - Array of deprecated features
- `DeploymentInstructions` - Step-by-step deployment guide
- `RollbackInstructions` - Step-by-step rollback guide

**Supporting Class:**
- `VersionChange` - Describes individual changes with category and impact level

---

## Key Design Decisions

### JSON Serialization for Complex Objects
Complex nested objects (governance policies, configurations, metrics) are serialized as JSON strings rather than creating additional tables. This provides:
- **Flexibility:** Schema evolution without migrations
- **Performance:** Single query retrieves complete aggregate
- **Maintainability:** Reduced normalization complexity

### Null-Safe JSON Conversion
EF Core JSON conversion is configured to safely handle null and missing data, deserializing to empty collections or defaults.

### Audit Fields
All entities include:
- `CreatedDate` - Set to GETUTCDATE() at insert
- `LastModifiedDate` - Updated on every modification
- `IsActive` - Soft delete / status tracking

### Relationships
- **1:1 relationships** (CopilotApplication → Config entities) are modeled with foreign keys and navigation properties
- **1:Many relationships** (CopilotApplication → KnowledgeTools, Versions) use collections
- Navigation properties are virtual to support lazy loading where appropriate

## Migration Path

To create these tables in the database:

```bash
# Add migration
dotnet ef migrations add CreateCopilotStudioEntities --project DataAccessLayer

# Apply migration
dotnet ef database update --project DataAccessLayer
```

## DTOLayer Integration

Facet maps for these entities should be created in `DTOLayer/FacetMaps/CopilotStudio/`:
- `CopilotApplicationFacetMap.cs` - API-safe DTO for Copilot
- `CopilotModelConfigurationFacetMap.cs` - Model config DTO
- `KnowledgeToolFacetMap.cs` - Knowledge tool DTO
- `CopilotGovernancePolicyFacetMap.cs` - Policy DTO
- `CopilotPerformanceMetricsFacetMap.cs` - Metrics DTO
- `CopilotDeploymentConfigFacetMap.cs` - Deployment DTO
- `CopilotVersionFacetMap.cs` - Version DTO

Each facet map implements `FromEntity()` and `ToEntity()` for safe API model conversions.

## Service Layer Considerations

Services consuming `CopilotStudioDbContext` should:
1. Depend only on `DataAccessLayer` and `DTOLayer` projects
2. Use facet maps to convert entities to DTOs before returning to callers
3. Handle JSON deserialization for complex fields using centralized utilities
4. Implement optimistic concurrency with timestamp/version fields as needed

---

**Generated:** Current Session  
**Status:** All 7 core entity models created and integrated with CopilotStudioDbContext
