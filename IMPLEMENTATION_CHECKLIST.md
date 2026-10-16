# IMPLEMENTATION CHECKLIST & ACTION ITEMS

## Current Status: ✅ ARCHITECTURE VERIFIED - CONFIGURATION NEEDED

Your DI registrations are **100% correct**. Now execute these steps to make it work.

---

## PHASE 1: IMMEDIATE ACTIONS (Next 30 Minutes)

### [ ] 1.1 Set Configuration Values

**Location:** `TubieTools_Aspire.EnterpriseAutomation/appsettings.Development.json`

**Action:** Update with your actual values:

```json
{
  "ChatGPT": {
	"ApiKey": "sk-YOUR-ACTUAL-OPENAI-KEY",
	"Model": "gpt-4-turbo",
	"Temperature": 0.7,
	"MaxTokens": 2000
  },
  "ServiceNow": {
	"Instance": "YOUR-INSTANCE-NAME",
	"Token": "YOUR-OAUTH-BEARER-TOKEN"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning"
	}
  }
}
```

**How to Get Values:**
- **OpenAI Key**: https://platform.openai.com/api-keys (starts with `sk-`)
- **ServiceNow Instance**: Your instance URL: `https://dev12345.service-now.com` (use `dev12345`)
- **ServiceNow Token**: Create OAuth app in ServiceNow, generate bearer token

### [ ] 1.2 Verify Build Compiles

```bash
cd C:\Users\xeque\source\repos\TubieTools_Aspire
dotnet clean
dotnet build
```

**Expected Result:** No errors, should complete in ~30 seconds

**If it fails:**
- Check for syntax errors in appsettings.Development.json
- Verify .NET 10 SDK: `dotnet --version`
- Check for missing NuGet packages

### [ ] 1.3 Run the Application

```bash
dotnet run --project TubieTools_Aspire.EnterpriseAutomation\TubieTools_Aspire.EnterpriseAutomation.csproj
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:7000
	  Now listening on: http://localhost:5000
```

### [ ] 1.4 Test Health Endpoint

```bash
curl http://localhost:5000/health
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "checks": {
	"azure": "Healthy",
	"servicenow": "Unhealthy",
	"kubernetes": "Unhealthy",
	"entra-id": "Unhealthy"
  }
}
```

**Note:** Some may be "Unhealthy" if not configured yet (this is OK for now)

---

## PHASE 2: CORE TESTING (Next Hour)

### [ ] 2.1 Test Basic LLM Request

```bash
curl -X POST http://localhost:5000/api/aiagent/ask \
  -H "Content-Type: application/json" \
  -d '{"message": "Say hello"}'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Hello! I'm here to help...",
  "result": null,
  "executedTools": [],
  "conversationHistory": [...]
}
```

**If it fails:**
- Check OpenAI API key is correct
- Verify internet connectivity
- Check debug output for error details

### [ ] 2.2 Test Conversation History

```bash
curl http://localhost:5000/api/aiagent/history
```

**Expected Response:**
```json
{
  "messages": [
	{
	  "role": "user",
	  "content": "Say hello",
	  "timestamp": "2024-01-15T10:30:00Z"
	},
	{
	  "role": "assistant",
	  "content": "Hello! I'm here to help...",
	  "timestamp": "2024-01-15T10:30:05Z"
	}
  ]
}
```

### [ ] 2.3 Test Context Setting

```bash
curl -X POST http://localhost:5000/api/aiagent/set-context \
  -H "Content-Type: application/json" \
  -d '{
	"messages": [
	  {
		"role": "system",
		"content": "You are an IT support specialist"
	  }
	]
  }'
```

**Expected Response:**
```json
{
  "message": "Conversation context set successfully"
}
```

### [ ] 2.4 Test Clear History

```bash
curl -X POST http://localhost:5000/api/aiagent/clear
```

**Expected Response:**
```json
{
  "message": "Conversation history cleared"
}
```

---

## PHASE 3: SERVICENOW INTEGRATION (Needs ServiceNow Setup)

### [ ] 3.1 Verify ServiceNow Configuration

```bash
# Check if ServiceNow is accessible
curl -X GET https://YOUR-INSTANCE.service-now.com/api/now/incident \
  -H "Authorization: Bearer YOUR-TOKEN" \
  -H "Accept: application/json"
```

**Expected Response:**
```json
{
  "result": [
	{
	  "number": "INC0010001",
	  "short_description": "...",
	  "state": "1"
	}
  ]
}
```

### [ ] 3.2 Test Tool Discovery

```bash
# Manually check if tools are discovered
curl -X POST http://localhost:5000/api/aiagent/ask \
  -H "Content-Type: application/json" \
  -d '{"message": "What tools are available?"}'
```

**Expected Response:** Should mention incident management tools

### [ ] 3.3 Test Search Incident Tool

```bash
curl -X POST http://localhost:5000/api/aiagent/ask \
  -H "Content-Type: application/json" \
  -d '{"message": "Search for open incidents"}'
```

**Expected Response:**
```json
{
  "success": true,
  "executedTools": ["search_incident"],
  "result": {
	"incidents": [...],
	"count": 5
  }
}
```

### [ ] 3.4 Test Create Incident Tool

```bash
curl -X POST http://localhost:5000/api/aiagent/ask \
  -H "Content-Type: application/json" \
  -d '{
	"message": "Create an incident with title \"Database Server Down\" and priority 1"
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "executedTools": ["create_incident"],
  "result": {
	"number": "INC0010123",
	"state": "1",
	"priority": "1"
  }
}
```

---

## PHASE 4: MULTI-TENANT VALIDATION (Optional - For Testing)

### [ ] 4.1 Test Free Tier Quota

**Simulation:** Free tier allows only search_incident

```bash
curl -X POST http://localhost:5000/api/aiagent/ask \
  -H "X-Tenant-Id: tenant-free-001" \
  -H "Content-Type: application/json" \
  -d '{"message": "Create an incident"}'
```

**Expected Result:** Should either:
- Fail to create incident (tool not available for tier), OR
- Respond with message that tool is not available for tier

### [ ] 4.2 Test Quota Enforcement

**Simulation:** Make 101+ requests in one month with free tier

**Expected Result:** 101st request should fail with quota exceeded message

---

## PHASE 5: PRODUCTION PREPARATION (Before Deploying)

### [ ] 5.1 Set Up Azure Key Vault

```bash
# Create Key Vault
az keyvault create \
  --name tubieteools-kv \
  --resource-group your-rg

# Add secrets
az keyvault secret set \
  --vault-name tubieteools-kv \
  --name "ChatGPT--ApiKey" \
  --value "sk-your-actual-key"

az keyvault secret set \
  --vault-name tubieteools-kv \
  --name "ServiceNow--Token" \
  --value "your-bearer-token"
```

### [ ] 5.2 Update Program.cs for Production

```csharp
// In Program.cs, update configuration loading:
if (!builder.Environment.IsDevelopment())
{
	var keyVaultUrl = new Uri(builder.Configuration["KeyVault:Url"]);
	var credential = new DefaultAzureCredential();
	builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
}
```

### [ ] 5.3 Enable HTTPS Only

**Add to Program.cs:**
```csharp
if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}
app.UseHttpsRedirection();
```

### [ ] 5.4 Configure Rate Limiting

**Add to Program.cs:**
```csharp
builder.Services.AddRateLimiter(options =>
{
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: context.User?.FindFirst("tenant_id")?.Value ?? "anonymous",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 100,
				Window = TimeSpan.FromMinutes(1)
			}));
});

app.UseRateLimiter();
```

### [ ] 5.5 Enable Structured Logging

**Update appsettings.json:**
```json
{
  "Serilog": {
	"MinimumLevel": "Information",
	"WriteTo": [
	  {
		"Name": "Console"
	  },
	  {
		"Name": "File",
		"Args": {
		  "path": "logs/app-.txt",
		  "rollingInterval": "Day"
		}
	  }
	]
  }
}
```

### [ ] 5.6 Set Up Monitoring

**Add Application Insights:**
```bash
# Create App Insights
az monitor app-insights component create \
  --app tubieteools-ai \
  --resource-group your-rg

# Add instrumentation key to Key Vault
```

---

## PHASE 6: DEPLOYMENT (To Azure)

### [ ] 6.1 Create Docker Image

**Dockerfile already exists in project**

```bash
docker build -t tubieteools:v1 .
docker tag tubieteools:v1 youracr.azurecr.io/tubieteools:v1
docker push youracr.azurecr.io/tubieteools:v1
```

### [ ] 6.2 Deploy to Azure Container Instances

```bash
az container create \
  --resource-group your-rg \
  --name tubieteools-ai \
  --image youracr.azurecr.io/tubieteools:v1 \
  --registry-username USERNAME \
  --registry-password PASSWORD \
  --environment-variables \
	KeyVault__Url=https://tubieteools-kv.vault.azure.net/ \
  --cpu 2 --memory 4
```

### [ ] 6.3 Configure App Service (Alternative)

```bash
az appservice plan create \
  --name tubieteools-plan \
  --resource-group your-rg \
  --sku B2

az webapp create \
  --resource-group your-rg \
  --plan tubieteools-plan \
  --name tubieteools-ai
```

---

## TROUBLESHOOTING GUIDE

### Build Fails: "Project 'X' does not exist"
```bash
# Ensure solution file is closed in file explorer
# Restart Visual Studio
# Run: dotnet restore
```

### "AllowAnonymousUser() doesn't exist"
```csharp
// Replace with:
options.AddPolicy("AllowAnonymous", policy =>
	policy.RequireAssertion(_ => true));
```

### OpenAI API 401 Unauthorized
```
Check: Is your API key correct?
Check: Has your API key expired?
Check: Do you have a payment method on file?
```

### ServiceNow 401 Unauthorized
```
Check: Is your OAuth token valid?
Check: Has your token expired?
Check: Does the service account have API permissions?
```

### "HTTP 500: Internal Server Error"
```bash
# Check debug output:
- Review the actual exception message
- Check if external services (OpenAI, ServiceNow) are reachable
- Verify configuration values are correctly set
- Check /health endpoint for service status
```

### Quota Enforcement Not Working
```csharp
// Verify MultiTenantAIAgent is filtering:
public async Task<AgentResponse> ProcessRequestAsync(string userRequest)
{
	// Should check subscription tier
	// Should decrement quota
	var availableTools = await _subscriptionManager.GetAvailableToolsForTenant(...);
	// Then call base IAIAgent
}
```

---

## QUICK REFERENCE

### Essential Configuration Keys
```json
{
  "ChatGPT:ApiKey",
  "ChatGPT:Model",
  "ChatGPT:Temperature",
  "ChatGPT:MaxTokens",
  "ServiceNow:Instance",
  "ServiceNow:Token"
}
```

### Essential Endpoints
```
POST /api/aiagent/ask
GET /api/aiagent/history
POST /api/aiagent/clear
POST /api/aiagent/set-context
GET /health
```

### Essential Files Modified/Created
```
ENTERPRISE_ARCHITECTURE_DEEP_ANALYSIS.md
DI_REGISTRATION_VERIFICATION_REPORT.md
QUICK_START_GUIDE.md  
CONFIGURATION_TEMPLATE.json
VERIFICATION_SUMMARY.md
IMPLEMENTATION_CHECKLIST_AND_ACTION_ITEMS.md (this file)
```

---

## SUCCESS CRITERIA

You're done when:

✅ `dotnet build` completes without errors
✅ `curl http://localhost:5000/health` returns Healthy
✅ `curl -X POST http://localhost:5000/api/aiagent/ask -H "Content-Type: application/json" -d '{"message": "Hello"}'` returns AgentResponse with success=true
✅ Conversation history persists across requests
✅ Tool invocation works end-to-end (search_incident at minimum)
✅ Multi-tenant isolation is testable (different responses for different tiers)

---

## TIMELINE

- **Today (30 mins):** Phase 1 - Set config & build
- **Today (1 hour):** Phase 2 - Core testing  
- **Tomorrow:** Phase 3 - ServiceNow integration
- **This week:** Phase 4 - Multi-tenant validation
- **Next week:** Phase 5 - Production prep
- **Week after:** Phase 6 - Azure deployment

---

**You're 80% done. Just need configuration + testing. Let's complete this! 🚀**

