# Power Platform Implementation Handbook
## Hands-On Technical Guide

---

## Section 1: Dataverse Table Creation (Step-by-Step)

### Create Base Table: CopilotApplication

**In Power Apps (power.apps.com):**

1. Go to Solutions → Create New Solution
   - Name: "CopilotStudio"
   - Publisher: Your organization

2. Create Table: Copilot Application
   - Display Name: "Copilot Application"
   - Plural: "Copilot Applications"
   - Enable audit logging

3. Add These Columns:
   ```
   - Name (auto-created, searchable)
   - LandingZone (Text, 100 chars, searchable)
   - Description (Memo, max 2000)
   - BusinessObjective (Text, 500)
   - ModelConfiguration (Lookup → Model Config table)
   - GovernancePolicy (Lookup → Governance Policy table)
   - Owner (Lookup → User)
   - Status (Choice: Dev/Test/Prod/Deprecated)
   - Capabilities (Text, JSON format)
   - CurrentVersion (Text, 50)
   - IsActive (Yes/No choice)
   ```

4. Create Relationships
   - 1:N to KnowledgeTools
   - 1:N to CopilotVersions
   - N:1 from GovernancePolicy

### Conventions for the Remaining 6 Tables

Same pattern as CopilotApplication, plus:

- Schema names use the `copilot_` publisher prefix (e.g. `copilot_modelconfiguration`)
- Complex .NET objects (nested config classes) → **Memo column, JSON format** (same convention as `Capabilities`)
- `long` counters → **Whole Number (Big)**; money → **Currency**; `decimal` → **Decimal**
- Dataverse auto-adds `createdby`, `createdon`, `modifiedby`, `modifiedon`, `ownerid`, `statecode`, `statuscode` — do NOT create these
- Enable audit logging on every table

---

### Table 2: CopilotModelConfiguration

Schema name: `copilot_modelconfiguration` — dual-write (reference)

1. Create Table:
   - Display Name: "Copilot Model Configuration"
   - Plural: "Copilot Model Configurations"
   - Primary column: "Configuration Name" (Text, 200, searchable)

2. Add These Columns:
   ```
   - ModelProvider (Choice: OpenAI/Azure OpenAI/Anthropic/Custom, searchable)
   - ModelName (Text, 100 chars, searchable)
   - ModelVersion (Text, 50)
   - Temperature (Decimal, min 0, max 2, precision 2, default 0.7)
   - TopP (Decimal, min 0, max 1, precision 2, default 0.9)
   - MaxTokens (Whole Number, default 2000)
   - FrequencyPenalty (Decimal, min -2, max 2, precision 2, default 0)
   - PresencePenalty (Decimal, min -2, max 2, precision 2, default 0)
   - SystemPrompt (Memo, max 10000)
   - CustomParameters (Memo, JSON format)
   - SafetySettings (Memo, JSON format)
   - ContextWindowSize (Whole Number)
   - SupportsFunctionCalling (Yes/No)
   ```

3. Create Relationships
   - 1:N to CopilotApplications (via `ModelConfiguration` lookup on CopilotApplication)

---

### Table 3: KnowledgeTool

Schema name: `copilot_knowledgetool` — dual-write (1:N child)

1. Create Table:
   - Display Name: "Knowledge Tool"
   - Plural: "Knowledge Tools"
   - Primary column: "Tool Name" (Text, 200, searchable)

2. Add These Columns:
   ```
   - CopilotApplication (Lookup → Copilot Application, required)
   - Description (Memo, max 2000)
   - Pattern (Choice: VectorSearch/RAG/StructuredQuery/Hybrid)
   - DataSource (Memo, JSON format — endpoint, index, auth ref)
   - RetrievalConfig (Memo, JSON format)
   - EmbeddingConfig (Memo, JSON format — null if not vector search)
   - ContextWindowSize (Whole Number, default 2000)
   - RelevanceThreshold (Decimal, min 0, max 1, precision 2, default 0.7)
   - MaxResults (Whole Number, default 5)
   - CacheConfig (Memo, JSON format)
   - AccessControl (Memo, JSON format — roles/groups allowed)
   - FreshnessRequirement (Choice: RealTime/Daily/Weekly/Monthly)
   - IsEnabled (Yes/No, default Yes)
   - PerformanceMetrics (Memo, JSON format — hit rate, latency snapshot)
   ```

3. Create Relationships
   - N:1 to CopilotApplication (parental, cascade delete)

---

### Table 4: CopilotGovernancePolicy

Schema name: `copilot_governancepolicy` — dual-write (reference)

1. Create Table:
   - Display Name: "Copilot Governance Policy"
   - Plural: "Copilot Governance Policies"
   - Primary column: "Policy Name" (Text, 200, searchable)

2. Add These Columns:
   ```
   - LandingZone (Text, 100 chars, searchable)
   - Description (Memo, max 2000)
   - DataResidency (Memo, JSON format — allowed regions)
   - SecurityRequirements (Memo, JSON format)
   - ComplianceRequirements (Memo, JSON format — array of policies)
   - DataHandling (Memo, JSON format — PII rules, retention)
   - ModelGovernance (Memo, JSON format — allowed models/providers)
   - AuditRequirements (Memo, JSON format)
   - CostManagement (Memo, JSON format — budgets, alerts)
   - IncidentResponse (Memo, JSON format — escalation paths)
   - EnforcementMode (Choice: Strict/Moderate/Advisory, default Strict)
   - RequiresAttestation (Yes/No)
   - LastReviewDate (Date Only)
   - NextReviewDate (Date Only)
   - IsActive (Yes/No, default Yes)
   ```

3. Create Relationships
   - 1:N to CopilotApplications (via `GovernancePolicy` lookup on CopilotApplication)

---

### Table 5: CopilotPerformanceMetrics

Schema name: `copilot_performancemetrics` — event-driven (streaming, one row per measurement period)

1. Create Table:
   - Display Name: "Copilot Performance Metrics"
   - Plural: "Copilot Performance Metrics"
   - Primary column: "Metrics Name" (Text, 200 — e.g. "{copilot} – {period end}")

2. Add These Columns:
   ```
   - CopilotApplication (Lookup → Copilot Application, required, searchable)
   - MeasurementPeriodDays (Whole Number, default 30)
   - TotalInteractions (Whole Number (Big))
   - SuccessfulCompletions (Whole Number (Big))
   - FailedInteractions (Whole Number (Big))
   - SuccessRate (Decimal, 0-100, precision 2)
   - AvgResponseTimeMs (Decimal, precision 0)
   - P95ResponseTimeMs (Decimal, precision 0)
   - ActiveUsers (Whole Number)
   - AvgSessionDurationMinutes (Decimal, precision 2)
   - UserSatisfactionScore (Decimal, 0-5, precision 2)
   - KnowledgeHitRate (Decimal, 0-100, precision 2)
   - ActionSuccessRate (Decimal, 0-100, precision 2)
   - EvaluationPassRate (Decimal, 0-100, precision 2)
   - UptimePercentage (Decimal, 0-100, precision 3)
   - TokensUsed (Whole Number (Big))
   - CostPerInteraction (Currency)
   - TokenEfficiency (Decimal, precision 4)
   - LastUpdated (DateTime, user local)
   ```

3. Create Relationships
   - N:1 to CopilotApplication

4. Note: This is the table the Power BI dashboard (Section 6) connects to.

---

### Table 6: CopilotDeploymentConfig

Schema name: `copilot_deploymentconfig` — dual-write (operational)

**Already scaffolded for the pilot:** `solution/schema/CopilotDeploymentConfig.table.json` (Deployment Name, Environment, Status, Timestamp, Rollback*, Latency, QueueDepth, LastError, ResponseCode). Create the table from that schema first, then add the EF-mirror columns below.

1. Create Table (if not using the pilot schema):
   - Display Name: "Copilot Deployment Config"
   - Plural: "Copilot Deployment Configs"
   - Primary column: "Deployment Name" (Text, 200, searchable)

2. Add These Columns (in addition to the pilot audit columns):
   ```
   - CopilotApplication (Lookup → Copilot Application, required, searchable)
   - Environment (Choice: Dev/Staging/Prod)  ← already in pilot schema
   - DeploymentStrategy (Choice: BlueGreen/Canary/RollingUpdate)
   - CanaryPercentageUsers (Whole Number, min 0, max 100)
   - CanaryDurationHours (Whole Number)
   - DeploymentFrequency (Choice: OnDemand/Daily/Weekly/Monthly)
   - MaintenanceWindow (Text, 100 — e.g. "Sunday 2:00-4:00 AM UTC")
   - AutoScalingEnabled (Yes/No)
   - MinInstances (Whole Number, default 1)
   - MaxInstances (Whole Number, default 10)
   - LoadDistribution (Choice: RoundRobin/LeastConnections/IPHash)
   - RollbackCapabilityEnabled (Yes/No, default Yes)
   - HealthCheck (Memo, JSON format — interval, timeout, thresholds, endpoint)
   - ZeroDowntimeEnabled (Yes/No)
   - SwitchoverTimeoutMinutes (Whole Number, default 30)
   ```

3. Create Relationships
   - N:1 to CopilotApplication

4. Note: Section 3's notification flow triggers on this table's `Status` column.

---

### Table 7: CopilotVersion

Schema name: `copilot_version` — time-based (snapshot)

1. Create Table:
   - Display Name: "Copilot Version"
   - Plural: "Copilot Versions"
   - Primary column: "Version Number" (Text, 50, searchable — semver)

2. Add These Columns:
   ```
   - CopilotApplication (Lookup → Copilot Application, required, searchable)
   - ReleaseType (Choice: Major/Minor/Patch/Beta/RC)
   - ReleaseDate (Date Only)
   - ReleaseNotes (Memo, max 4000)
   - Changes (Memo, JSON format — typed change list)
   - BreakingChanges (Memo, JSON format — string array)
   - Deprecations (Memo, JSON format — string array)
   - MigrationGuideUrl (Text, 500, URL format)
   - VersionMetrics (Memo, JSON format — metrics snapshot at release)
   - DeploymentStatus (Choice: Development/Staging/Production/Archived)
   - RollbackPath (Lookup → Copilot Version, self-referential, optional)
   - SupportEndDate (Date Only, optional)
   ```

3. Create Relationships
   - N:1 to CopilotApplication
   - 1:N self-referential via RollbackPath (previous version)

---

### Relationship Map (verify after all 7 tables exist)

```
CopilotGovernancePolicy   (1) ──→ (N) CopilotApplication
CopilotModelConfiguration (1) ──→ (N) CopilotApplication
CopilotApplication        (1) ──→ (N) KnowledgeTool          (parental, cascade)
CopilotApplication        (1) ──→ (N) CopilotVersion         (parental, cascade)
CopilotApplication        (1) ──→ (N) CopilotPerformanceMetrics
CopilotApplication        (1) ──→ (N) CopilotDeploymentConfig
CopilotVersion            (1) ──→ (N) CopilotVersion         (self, RollbackPath)
```

**Total time:** ~2-3 hours for complete schema

---

## Section 2: Dual-Write Service Implementation

### Code: DataverseService.cs

```csharp
public class DataverseService : IDataverseService
{
	private readonly ServiceClient _serviceClient;
	private readonly ILogger<DataverseService> _logger;

	public async Task<Guid> CreateCopilotApplicationAsync(
		CopilotApplication copilot)
	{
		try
		{
			var entity = new Entity("copilot_copilotapplications")
			{
				Id = Guid.Parse(copilot.CopilotId),
				["copilot_name"] = copilot.Name,
				["copilot_landingzone"] = copilot.LandingZone,
				["copilot_description"] = copilot.Description,
				["copilot_objective"] = copilot.BusinessObjective,
				["copilot_capabilities"] = copilot.Capabilities,
				["statecode"] = copilot.IsActive ? 1 : 0
			};

			var id = await _serviceClient.CreateAsync(entity);
			_logger.LogInformation($"Synced to Dataverse: {id}");
			return id;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Dataverse sync failed: {ex.Message}");
			// Queue for retry, but don't fail the request
			await QueueForRetryAsync(copilot.CopilotId);
			throw;
		}
	}
}
```

**Register in Startup:**
```csharp
var dataverseUri = new Uri(configuration["Dataverse:InstanceUrl"]);
var serviceClient = new ServiceClient(dataverseUri, token);
services.AddScoped<ServiceClient>(_ => serviceClient);
services.AddScoped<IDataverseService, DataverseService>();
services.AddScoped<DualWriteService>();
```

---

## Section 3: Build First Flow (Deployment Notification)

**Steps in Power Automate:**

1. Create Automated Flow
2. Trigger: "When a row is updated" (Dataverse)
   - Table: Copilot Deployment Configs
   - Change column: deployment_status
   - Changes to: "Active"

3. Add Actions:
   ```
   Get row → Teams message → Email notification
   ```

4. Teams message format:
   ```
   ✅ Deployed: {name}
   📊 Env: {environment}
   👤 Owner: {owner}
   ⏰ Time: {timestamp}
   [View Logs] [Monitor] [Rollback]
   ```

**Expected Result:** Notification within 2 seconds of deployment

---

## Section 4: Create Interactive Copilot

**In Copilot Studio:**

1. New copilot → "Copilot Support Assistant"
2. Add knowledge: Upload documentation
3. Add data connection: Dataverse tables
4. Add actions: Call your APIs
5. Create topics: Conversation flows
6. Test → Publish → Add to Teams

**Sample Conversation:**
```
User: "What's the status of HR copilot?"
Copilot: "Checking... HR copilot is active, 99.8% uptime"
```

---

## Section 5: Create Management App (Model-driven)

**In Power Apps:**

1. New app → Model-driven
2. Select table: Copilot Applications
3. Auto-generated:
   - Responsive grid
   - Create/Edit forms
   - Related records views
4. Customize:
   - Add dashboard
   - Configure charts
   - Set up quick actions

**Result:** Full CRUD management interface

---

## Section 6: Power BI Dashboard

**Connection:**
```
Power BI → Dataverse → CopilotPerformanceMetrics
Create charts:
- Uptime by copilot
- Cost trends
- Error rates
- User adoption
```

---

## Success Checklist

- [ ] Dataverse environment created
- [ ] 7 tables defined with relationships
- [ ] Dual-write service compiling
- [ ] First flow deployed (deployment notifications)
- [ ] Interactive copilot working in Teams
- [ ] Management app live
- [ ] Performance dashboard showing data

**Timeline:** 2-4 weeks with team
**Cost:** $100-500/month (depends on volume)
**ROI:** 3-5 months (labor savings)

---

Ready to begin! Start with Dataverse setup this week.
