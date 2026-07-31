# Multi-Tenant AI Agent Platform with Tiered Subscriptions

## Overview

This implementation provides a complete multi-tenant architecture for the TubieTools AspireAutomation platform, enabling multiple organizations to utilize AI Agents with ChatGPT as the backend, integrated with MCP client for ServiceNow tool invocation, all secured by subscription-based access control.

## Architecture Components

### 1. Multi-Tenant Models (`MultiTenantModels.cs`)

**Core Entities:**
- `SubscriptionTier` - Enum (Free, Starter, Professional, Enterprise)
- `TenantConfig` - Tenant organization information
- `TenantSubscription` - Subscription details with billing
- `TenantQuota` - Usage limits and tracking
- `TenantUsage` - Daily/monthly usage statistics
- `TenantCustomAgent` - Custom AI agents per tenant
- `TenantTeamMember` - Team access management
- `SubscriptionAddOn` - Optional paid add-ons
- `TenantBillingRecord` - Invoice/billing history

### 2. Tenant Context (`TenantContext.cs`)

**Purpose:** Request-scoped context containing:
- Current tenant identity
- Subscription tier and features
- Available tools and models
- Quota information
- Feature flags

**Usage:** Injected into services via `ITenantContextAccessor`

### 3. Tenant Service (`TenantService.cs`)

**Responsibilities:**
- Tenant CRUD operations
- Subscription management
- Quota enforcement
- Usage tracking
- Team member management
- Custom agent management

**Key Methods:**
```csharp
Task<TenantConfig> GetTenantAsync(string tenantId)
Task<bool> IncrementUsageAsync(string tenantId, int apiCallCount)
Task<bool> IsQuotaExceededAsync(string tenantId)
Task<List<TenantCustomAgent>> GetTenantAgentsAsync(string tenantId)
Task<List<TenantUsage>> GetUsageStatsAsync(string tenantId, DateTime start, DateTime end)
```

### 4. Subscription Manager (`SubscriptionManager.cs`)

**Responsibilities:**
- Tier configuration management
- Feature availability checks
- Tool access control
- Model access restrictions
- Add-on management

**Key Methods:**
```csharp
Task<SubscriptionTierConfig> GetTierConfigAsync(SubscriptionTier tier)
Task<List<string>> GetAvailableToolsForTierAsync(SubscriptionTier tier)
Task<bool> UpgradeTierAsync(string tenantId, SubscriptionTier newTier)
Task<ToolFeatureAccess> GetToolAccessAsync(string toolName, SubscriptionTier tier)
```

### 5. Tenant Resolver Middleware (`TenantResolverMiddleware.cs`)

**Process:**
1. Extracts tenant ID from:
   - `X-Tenant-ID` header
   - JWT claims (`tenant_id`)
   - API key header
2. Validates tenant exists and is active
3. Loads subscription and quota data
4. Builds feature flags
5. Sets `TenantContext` for request

**Placement:** Early in pipeline before controllers

```csharp
app.UseTenantResolver();
```

### 6. Multi-Tenant AI Agent (`MultiTenantAIAgent.cs`)

**Wrapper around base AI Agent with:**
- Subscription tier validation
- Tool access control
- Quota enforcement
- Usage tracking
- Feature flag checks

**Key Methods:**
```csharp
Task<AgentResponse> ProcessRequestAsync(string tenantId, string userRequest)
Task<bool> ValidateAccessAsync(string tenantId, string feature, string toolName)
```

### 7. Subscription Tiers Configuration

#### **Free Tier**
- 100 API calls/month, 20/day
- Concurrent requests: 1
- Tools: search_incident only
- No custom prompts, analytics, or API access
- Price: $0

#### **Starter Tier**
- 5,000 API calls/month, 200/day
- Concurrent requests: 5
- All tools available (create, search, close)
- Custom prompts, analytics, API access enabled
- Up to 3 team members
- Price: $29/month

#### **Professional Tier**
- 50,000 API calls/month, 2,000/day
- Concurrent requests: 20
- All tools, multiple agents, workflow orchestration
- Webhooks, priority support enabled
- Up to 10 team members
- Price: $99/month

#### **Enterprise Tier**
- Unlimited API calls
- Concurrent requests: 100
- All features enabled
- SSO, data residency, custom SLA
- Unlimited team members
- Price: Custom pricing

## API Endpoints

### Tenant Management

```http
POST /api/v1/tenants/register
Content-Type: application/json

{
  "tenantName": "Acme Corp",
  "description": "Enterprise automation"
}

Response:
{
  "tenantId": "uuid",
  "apiKey": "sk_...",
  "message": "Tenant registered successfully"
}
```

### AI Agent Operations

```http
POST /api/v1/tenants/{tenantId}/agent/ask
Headers:
  X-Tenant-ID: {tenantId}
Content-Type: application/json

{
  "message": "Create a ServiceNow incident for database outage"
}

Response:
{
  "success": true,
  "message": "Incident INC0123456 created successfully",
  "result": { ... },
  "executedTools": ["create_incident"],
  "conversationHistory": [...]
}
```

### Subscription Management

```http
GET /api/v1/tenants/{tenantId}/subscription
POST /api/v1/tenants/{tenantId}/upgrade
GET /api/v1/tenants/tiers
```

### Usage & Analytics

```http
GET /api/v1/tenants/{tenantId}/usage?daysBack=30

Response:
{
  "quota": {
	"monthlyApiCallLimit": 5000,
	"monthlyApiCallsUsed": 2345,
	"dailyApiCallsUsed": 45
  },
  "usage": [
	{
	  "date": "2024-01-15",
	  "apiCallsUsed": 120,
	  "toolUsageStats": {
		"create_incident": 10,
		"search_incident": 50,
		"close_incident": 5
	  }
	}
  ]
}
```

### Agent Management

```http
POST /api/v1/tenants/{tenantId}/agents
Content-Type: application/json

{
  "agentName": "Custom ServiceNow Agent",
  "systemPrompt": "You are a specialized incident management assistant...",
  "assignedTools": ["create_incident", "search_incident", "close_incident"],
  "preferredModel": "gpt-4"
}
```

### Team Management

```http
POST /api/v1/tenants/{tenantId}/team
Content-Type: application/json

{
  "email": "user@acmecorp.com",
  "role": "admin|user|viewer"
}
```

## Configuration

### appsettings.json

```json
{
  "ChatGPT": {
	"ApiKey": "sk-...",
	"Model": "gpt-4",
	"Temperature": 0.7,
	"MaxTokens": 2000
  },
  "MultiTenant": {
	"EnableTenantIsolation": true,
	"DefaultTier": "Free"
  }
}
```

### Dependency Injection Registration

```csharp
// Multi-Tenant Services
builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddScoped<IMultiTenantAIAgent, MultiTenantAIAgent>();

// Middleware
app.UseTenantResolver();
```

## Feature Matrix

| Feature | Free | Starter | Pro | Enterprise |
|---------|------|---------|-----|-----------|
| Monthly API Calls | 100 | 5K | 50K | Unlimited |
| Tools (Create/Search/Close) | ❌/✅/❌ | ✅/✅/✅ | ✅/✅/✅ | ✅/✅/✅ |
| Custom Prompts | ❌ | ✅ | ✅ | ✅ |
| Multiple Agents | ❌ | ❌ | ✅ | ✅ |
| Workflow Orchestration | ❌ | ❌ | ✅ | ✅ |
| Analytics | ❌ | ✅ | ✅ | ✅ |
| API Access | ❌ | ✅ | ✅ | ✅ |
| Webhooks | ❌ | ❌ | ✅ | ✅ |
| Priority Support | ❌ | ❌ | ✅ | ✅ |
| Team Members Limit | 1 | 3 | 10 | Unlimited |
| Custom Integrations | 0 | 1 | 5 | Unlimited |
| SLA | Best Effort | 99% | 99.5% | 99.99% |

## Usage Flow

```
1. Tenant Registration
   ↓
2. Receive API Key & Tenant ID
   ↓
3. Make Request with X-Tenant-ID header
   ↓
4. Middleware resolves tenant context
   ↓
5. Validate subscription tier
   ↓
6. Check quota enforcement
   ↓
7. Filter available tools based on tier
   ↓
8. Process with Multi-Tenant AI Agent
   ↓
9. Execute via MCP Client
   ↓
10. Track usage & update quota
   ↓
11. Return results
```

## Security Considerations

1. **Tenant Isolation:** Each tenant's data is isolated through context
2. **API Key Management:** Secure key generation and validation
3. **Quota Enforcement:** Hard limits on API usage
4. **Feature Access Control:** Tier-based tool and model availability
5. **Rate Limiting:** Per-tenant, per-tier rate limits
6. **Audit Logging:** All operations logged with tenant context
7. **RBAC:** Role-based access control (admin, user, viewer)

## Database Structure (Future Implementation)

```sql
-- Tenants
CREATE TABLE Tenants (
  TenantId NVARCHAR(MAX),
  TenantName NVARCHAR(MAX),
  ApiKey NVARCHAR(MAX),
  CurrentTier INT,
  IsActive BIT,
  CreatedDate DATETIME
);

-- Subscriptions
CREATE TABLE Subscriptions (
  SubscriptionId NVARCHAR(MAX),
  TenantId NVARCHAR(MAX),
  Tier INT,
  StartDate DATETIME,
  EndDate DATETIME,
  Status NVARCHAR(MAX),
  BillingAmount DECIMAL,
  FOREIGN KEY (TenantId) REFERENCES Tenants
);

-- Usage Tracking
CREATE TABLE UsageTracking (
  UsageId NVARCHAR(MAX),
  TenantId NVARCHAR(MAX),
  Date DATETIME,
  ApiCallsUsed INT,
  TokensUsed INT,
  FOREIGN KEY (TenantId) REFERENCES Tenants
);

-- Quotas
CREATE TABLE Quotas (
  TenantId NVARCHAR(MAX),
  MonthlyLimit INT,
  MonthlyUsed INT,
  ResetDate DATETIME,
  PRIMARY KEY (TenantId),
  FOREIGN KEY (TenantId) REFERENCES Tenants
);

-- Team Members
CREATE TABLE TeamMembers (
  MemberId NVARCHAR(MAX),
  TenantId NVARCHAR(MAX),
  Email NVARCHAR(MAX),
  Role NVARCHAR(MAX),
  IsActive BIT,
  FOREIGN KEY (TenantId) REFERENCES Tenants
);

-- Custom Agents
CREATE TABLE CustomAgents (
  AgentId NVARCHAR(MAX),
  TenantId NVARCHAR(MAX),
  AgentName NVARCHAR(MAX),
  SystemPrompt NVARCHAR(MAX),
  IsActive BIT,
  FOREIGN KEY (TenantId) REFERENCES Tenants
);
```

## Monitoring & Alerting

Key metrics to monitor per tenant:
- API call usage vs. quota
- Concurrent request count
- Average response time
- Error rate
- Token consumption
- Subscription expiration

## Future Enhancements

1. **Database Integration:** Replace in-memory dictionaries with SQL Server
2. **Payment Gateway:** Stripe/PayPal integration for billing
3. **SSO Support:** Azure AD, Okta integration
4. **Advanced Analytics:** Dashboard with usage insights
5. **Webhooks:** Event-based notifications
6. **Custom Rate Limiting:** More granular per-route limits
7. **Request Signing:** HMAC-SHA256 request validation
8. **Data Residency:** Regional data storage options
9. **Compliance:** SOC2, GDPR, HIPAA attestations
10. **Usage Forecasting:** ML-based quota recommendations

## Testing

Example test scenarios:
```csharp
// Test quota enforcement
var tenant = await _tenantService.GetTenantAsync("test-tenant");
Assert.Equal(SubscriptionTier.Free, tenant.CurrentTier);

var quotaExceeded = await _tenantService.IsQuotaExceededAsync("test-tenant"); 
Assert.False(quotaExceeded);

// Test tool access
var canAccess = await _agent.ValidateAccessAsync("test-tenant", "tool", "create_incident");
Assert.False(canAccess); // Free tier can't create

// Test tier upgrade
await _tenantService.UpdateTierAsync("test-tenant", SubscriptionTier.Starter);
canAccess = await _agent.ValidateAccessAsync("test-tenant", "tool", "create_incident");
Assert.True(canAccess); // Now available
```

## Support & Documentation

- API Documentation: `/swagger/ui`
- JSON Schema: `subscription-tiers.json`, `multi-tenant-api-schema.json`
- Issue Reporting: GitHub Issues
- Contact: support@example.com
