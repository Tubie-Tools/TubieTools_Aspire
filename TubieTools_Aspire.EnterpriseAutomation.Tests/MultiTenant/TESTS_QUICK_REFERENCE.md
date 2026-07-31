# Multi-Tenant Unit Tests - Quick Reference Guide

## What You Just Got

A comprehensive test suite of **114 unit tests** validating the entire multi-tenant ChatGPT-powered automation platform:

| Test File | Tests | What It Tests |
|-----------|-------|---------------|
| **TenantServiceTests.cs** | 21 | Tenant CRUD, quotas, usage, agents, billing |
| **SubscriptionManagerTests.cs** | 32 | Tier config, tool access, feature flags, limits |
| **MultiTenantAIAgentTests.cs** | 15 | ChatGPT integration, access control, tool filtering |
| **TenantResolverMiddlewareTests.cs** | 19 | Request processing, tenant resolution, context setup |
| **MultiTenantControllerTests.cs** | 27 | REST API endpoints, error handling, status codes |

---

## Quick Run Commands

### Run All Tests
```bash
cd path/to/TubieTools_Aspire.EnterpriseAutomation.Tests
dotnet test --verbosity normal
```

### Run Single Test File
```bash
dotnet test --filter "TenantServiceTests"
dotnet test --filter "SubscriptionManagerTests"
dotnet test --filter "MultiTenantAIAgentTests"
dotnet test --filter "TenantResolverMiddlewareTests"
dotnet test --filter "MultiTenantControllerTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName=TubieTools_Aspire.EnterpriseAutomation.Tests.MultiTenant.TenantServiceTests.CreateTenant_WithValidConfig_GeneratesUniqueApiKey"
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[*Tests]*"
```

### Run with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## Test Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│            Multi-Tenant Platform Tests (114)                │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐    ┌──────────────────┐               │
│  │ TenantService    │    │ Subscription     │               │
│  │ (21 tests)       │    │ Manager          │               │
│  │ ✓ CRUD ops      │    │ (32 tests)       │               │
│  │ ✓ Quotas        │    │ ✓ Tier config    │               │
│  │ ✓ Usage track   │    │ ✓ Tool access    │               │
│  │ ✓ Agents        │    │ ✓ Features       │               │
│  │ ✓ Teams         │    │ ✓ Limits         │               │
│  │ ✓ Billing       │    │ ✓ Data retention │               │
│  └──────────────────┘    └──────────────────┘               │
│                                                              │
│  ┌──────────────────┐    ┌──────────────────┐               │
│  │ MultiTenant      │    │ Tenant           │               │
│  │ AIAgent          │    │ ResolverMiddleware│               │
│  │ (15 tests)       │    │ (19 tests)       │               │
│  │ ✓ Access control │    │ ✓ Tenant resolve │               │
│  │ ✓ Tool filtering │    │ ✓ Context setup  │               │
│  │ ✓ Quota enforce  │    │ ✓ Feature flags  │               │
│  │ ✓ Usage tracking │    │ ✓ Quota enforce  │               │
│  └──────────────────┘    └──────────────────┘               │
│                                                              │
│  ┌───────────────────────────────────────────┐              │
│  │ MultiTenantController (27 tests)          │              │
│  │ ✓ Tenant registration      ✓ Upgrade tier │              │
│  │ ✓ Tenant retrieval         ✓ AI agent ask │              │
│  │ ✓ Subscription mgmt        ✓ Usage stats  │              │
│  │ ✓ Custom agents            ✓ Team members │              │
│  │ ✓ Error handling           ✓ HTTP codes   │              │
│  └───────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────┘
```

---

## What Each Test File Validates

### 1️⃣ TenantServiceTests.cs
**Tests the core data layer and business logic**

**Key Scenarios:**
- Creating tenants generates unique API keys ✅
- Deleting tenants removes all data ✅
- Quotas are enforced correctly ✅
- Usage accumulates properly ✅
- Custom agents are scoped to tenants ✅
- Team members are limited by tier ✅
- Billing records are generated ✅

**Run Command:**
```bash
dotnet test --filter "TenantServiceTests"
```

---

### 2️⃣ SubscriptionManagerTests.cs
**Tests subscription tiers and feature gating**

**Key Scenarios by Tier:**

| Feature | Free | Starter | Pro | Enterprise |
|---------|------|---------|-----|------------|
| **Tools** | Search | All 3 | All 3 | All 3 |
| **Models** | GPT-3.5 | GPT-3.5, GPT-4 | All 3 | All 3 |
| **API Access** | ❌ | ✅ | ✅ | ✅ |
| **Custom Prompts** | ❌ | ✅ | ✅ | ✅ |
| **Webhooks** | ❌ | ❌ | ✅ | ✅ |
| **Team Members** | 1 | 3 | 10 | Unlimited |
| **Monthly Calls** | 100 | 5,000 | 50,000 | Unlimited |
| **Monthly Price** | $0 | $29 | $99 | Custom |

**Run Command:**
```bash
dotnet test --filter "SubscriptionManagerTests"
```

---

### 3️⃣ MultiTenantAIAgentTests.cs
**Tests ChatGPT integration with access control**

**Key Scenarios:**
- API access denied for Free tier ✅
- Quota exceeded blocks requests ✅
- Free tier only gets search_incident tool ✅
- Starter+ gets all ServiceNow tools ✅
- Usage tracked after each request ✅
- Errors handled gracefully ✅

**Run Command:**
```bash
dotnet test --filter "MultiTenantAIAgentTests"
```

**Example Test:**
```
ProcessRequestAsync_WithFreeTier_FiltersToolsCorrectly
├─ Free tenant requests AI agent
├─ Middleware restricts to search_incident only
└─ ✅ Agent receives filtered tool list
```

---

### 4️⃣ TenantResolverMiddlewareTests.cs
**Tests ASP.NET Core middleware for request processing**

**Key Scenarios:**
- Tenant ID extracted from X-Tenant-ID header ✅
- Tenant ID extracted from JWT claim ✅
- Tenant ID extracted from URL path (API key) ✅
- Feature flags initialized per tier ✅
- Inactive tenants return 403 Forbidden ✅
- Unknown tenants return 404 Not Found ✅
- Next middleware called after setup ✅

**Request Flow Tested:**
```
HTTP Request
	↓
Read X-Tenant-ID header (or JWT, or path)
	↓
Load Tenant Config
	↓
Load Subscription & Quota
	↓
Build Feature Flags
	↓
Set TenantContext
	↓
✅ Next Middleware
```

**Run Command:**
```bash
dotnet test --filter "TenantResolverMiddlewareTests"
```

---

### 5️⃣ MultiTenantControllerTests.cs
**Tests REST API endpoints**

**Endpoints Tested:**

| Method | Endpoint | Expected Status | Tested Scenarios |
|--------|----------|-----------------|------------------|
| POST | `/api/tenant` | 201 Created | ✅ Valid data, ❌ Missing ID |
| GET | `/api/tenant/{id}` | 200 OK / 404 | ✅ Exists, ❌ Not found |
| POST | `/api/tenant/{id}/upgrade` | 200 OK | ✅ Upgrade, ❌ Not found |
| POST | `/api/agent/ask` | 200 OK / 429 / 403 | ✅ Success, 🚫 Quota, 🚫 Access |
| GET | `/api/subscription/{id}` | 200 OK / 404 | ✅ Exists, ❌ Not found |
| GET | `/api/usage/{id}` | 200 OK | ✅ With data, ✅ Empty list |
| GET | `/api/tiers` | 200 OK | ✅ All 4 tiers |
| POST | `/api/agents/{id}` | 201 Created | ✅ Valid agent |
| GET | `/api/agents/{id}` | 200 OK | ✅ List agents |
| POST | `/api/team/{id}` | 200 OK / 400 | ✅ Add member, ❌ Limit exceeded |

**Run Command:**
```bash
dotnet test --filter "MultiTenantControllerTests"
```

---

## Expected Test Results

All 114 tests should **PASS** ✅

```
Test run completed successfully
Total: 114
Passed: 114
Failed: 0
Skipped: 0
Duration: ~5-10 seconds
```

---

## Common Test Scenarios

### Scenario 1: Free Tier User Creates Incident
```
User (Free Tier)
	↓
X-Tenant-ID: free-tenant-001
	↓
Middleware resolves tenant
	↓
Features: api_access=false, tools=[search_incident]
	↓
Request: Create Incident
	↓
MultiTenantAIAgent checks access
	↓
❌ Access Denied (403)
```

### Scenario 2: Professional Tier User Creates Incident
```
User (Professional Tier)
	↓
X-Tenant-ID: pro-tenant-001
	↓
Middleware resolves tenant
	↓
Features: api_access=true, tools=[all three]
	↓
Request: Create Incident
	↓
MultiTenantAIAgent validates quota (5000 limit)
	↓
Usage: 450/5000
	↓
✅ Access Granted
	↓
ChatGPT + MCP Invoke create_incident
	↓
Usage updated: 451/5000
```

### Scenario 3: Monthly Quota Reset
```
Starter Tier: 5,000 calls/month
	↓
Month End (e.g., May 31)
	↓
Usage: 5000/5000 (exhausted)
	↓
June 1st begins
	↓
Quota Reset: 0/5000
	↓
✅ Tenant can make new requests
```

---

## Debugging Tips

### Test Failed? Check These:

1. **Mock Setup Issues**
   ```csharp
   _mockService.Setup(s => s.Method()).ReturnsAsync(expectedValue);
   // Verify this returns the exact type expected
   ```

2. **Async Timing**
   ```csharp
   // Use ReturnsAsync() not Returns() for Task<T>
   .ReturnsAsync(value) // ✅ Correct
   .Returns(Task.FromResult(value)) // ✅ Also correct
   .Returns(value) // ❌ Wrong!
   ```

3. **Status Code Assertions**
   ```csharp
   // Controller actions return IActionResult
   var result = await controller.Method();
   Assert.Equal(200, ((OkObjectResult)result).StatusCode);
   ```

4. **Collection Assertions**
   ```csharp
   Assert.Equal(expectedCount, collection.Count);
   Assert.Contains(expectedItem, collection);
   Assert.Empty(collection); // For empty checks
   ```

---

## Test Organization by Feature

### **Tier Progression Testing**
```
Free (100 calls) 
  ↓ Upgrade
Starter (5k calls)
  ↓ Upgrade
Professional (50k calls)
  ↓ Upgrade
Enterprise (unlimited)
```
**Tests:** SubscriptionManagerTests, MultiTenantControllerTests

### **Feature Flag Progression**
```
Free: Minimal (search only)
  ↓
Starter: + Custom prompts, Analytics
  ↓
Professional: + Webhooks, Multiple agents
  ↓
Enterprise: Everything
```
**Tests:** SubscriptionManagerTests

### **Quota Enforcement**
```
Request → Check Quota → Under limit? → Yes → Process → Increment
							↓
						   No → Reject (429)
```
**Tests:** TenantServiceTests, MultiTenantAIAgentTests, TenantResolverMiddlewareTests

---

## Integration Test Flow

```
HTTP Request
	↓
TenantResolverMiddleware
├─ Extract tenant ID
├─ Load config & quota
├─ Build feature flags
└─ Set TenantContext
	↓
MultiTenantController
├─ Validate request input
├─ Check authorization
└─ Call MultiTenantAIAgent
	↓
MultiTenantAIAgent
├─ Validate API access
├─ Check quota
├─ Filter tools by tier
└─ Call base ChatGPTAgent
	↓
ChatGPTAgent
├─ Call OpenAI API
├─ Parse tool calls
└─ Invoke MCP ServiceNow tools
	↓
TenantService
├─ Increment usage
└─ Log request
	↓
HTTP Response (200 OK with result)
```

**Tested by:** All 5 test files collectively

---

## Key Assertions to Understand

### Status Code Assertions
```csharp
Assert.Equal(200, ((OkObjectResult)result).StatusCode);        // 200 OK
Assert.IsType<CreatedAtActionResult>(result);                  // 201 Created
Assert.IsType<BadRequestObjectResult>(result);                 // 400
Assert.IsType<ObjectResult>(result) && o.StatusCode == 403;    // 403 Forbidden
Assert.IsType<NotFoundResult>(result);                         // 404
```

### Quota Assertions
```csharp
Assert.True(isExceeded);   // Quota is at/over limit
Assert.False(isExceeded);  // Quota has room
Assert.Equal(5000, limit); // Check exact limit value
```

### Tier Assertions
```csharp
Assert.Equal(SubscriptionTier.Professional, tier);
Assert.True(config.AllowWebhooks);
Assert.Contains("create_incident", tools);
Assert.Single(freeTierTools); // Only search_incident for Free
```

---

## Performance Characteristics

| Test | Typical Duration | Notes |
|------|------------------|-------|
| Tenant tests | <100ms | In-memory operations |
| Subscription tests | <50ms | Config lookups |
| Agent tests | <150ms | Multiple service calls |
| Middleware tests | <100ms | Context setup |
| Controller tests | <200ms | Full request simulation |
| **Total Suite** | **5-10s** | All 114 tests |

---

## Next Steps

1. **Run Tests**
   ```bash
   dotnet test
   ```

2. **Review Results**
   - All 114 should pass ✅
   - Review coverage report

3. **Integrate into CI/CD**
   - Add to GitHub Actions / Azure Pipelines
   - Run before every merge

4. **Expand Coverage**
   - Add integration tests
   - Add performance tests
   - Add load tests

5. **Maintain Tests**
   - Update when features change
   - Add tests for new scenarios
   - Keep documentation current

---

## Support & References

- **Test Framework:** xUnit (attributes: `[Fact]`, `[Theory]`)
- **Mocking:** Moq (Setup, Verify patterns)
- **Async:** Task, ReturnsAsync()
- **Assertions:** Assert.* methods

See **TEST_COVERAGE_REPORT.md** for detailed breakdown of all 114 tests.
