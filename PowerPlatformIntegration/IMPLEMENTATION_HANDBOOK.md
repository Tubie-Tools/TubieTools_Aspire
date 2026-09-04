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

### Repeat for Remaining 6 Tables

Follow same pattern for:
- CopilotModelConfiguration
- KnowledgeTool
- CopilotGovernancePolicy
- CopilotPerformanceMetrics
- CopilotDeploymentConfig
- CopilotVersion

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
