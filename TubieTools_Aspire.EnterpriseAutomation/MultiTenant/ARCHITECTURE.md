## Multi-Tenant AI Agent Platform - Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           CLIENT APPLICATIONS                                │
│  (Web UI, Mobile, API Clients, CLI Tools)                                   │
└──────────────────────────┬──────────────────────────────────────────────────┘
						   │
					HTTP/HTTPS Requests
						   │
┌──────────────────────────▼──────────────────────────────────────────────────┐
│                        ASP.NET CORE PIPELINE                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ TenantResolverMiddleware                                             │   │
│ │  • Extract Tenant ID (Header/JWT/API Key)                           │   │
│ │  • Load Tenant Configuration                                        │   │
│ │  • Load Subscription & Quota                                        │   │
│ │  • Build Feature Flags                                              │   │
│ │  • Set TenantContext                                                │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│                           │                                                  │
└───────────────────────────┼──────────────────────────────────────────────────┘
							│
			┌───────────────┴─────────────┐
			│                             │
	┌───────▼────────┐         ┌─────────▼──────────┐
	│ Controllers    │         │ Middleware Chain   │
	├────────────────┤         ├────────────────────┤
	│ MultiTenant    │         │ • Auth             │
	│ AIAgent        │         │ • CORS             │
	│ ServiceNow     │         │ • Logging          │
	│ Tenant Mgmt    │         │ • Error Handling   │
	└────────┬───────┘         └────────────────────┘
			 │
			 └────────────────────────┬────────────────────────┐
									  │                        │
					┌─────────────────▼──────────────┐  ┌──────▼──────────┐
					│ IMultiTenantAIAgent             │  │ ITenantService  │
					├─────────────────────────────────┤  ├─────────────────┤
					│ • Validate subscription tier    │  │ • CRUD Tenants  │
					│ • Check quota enforcement       │  │ • Manage quotas │
					│ • Filter available tools        │  │ • Track usage   │
					│ • Track usage                   │  │ • Manage agents │
					│ • Increment counters            │  │ • Team mgmt     │
					└────────────┬──────────────────┘  └────────┬─────────┘
								 │                             │
					┌────────────▼──────────────┐  ┌──────────▼────────┐
					│ IAIAgent                   │  │ ISubscriptionMgr  │
					│ (ChatGPT Agent)            │  ├───────────────────┤
					├────────────────────────────┤  │ • Tier configs    │
					│ • Process requests         │  │ • Feature matrix  │
					│ • Call ChatGPT API         │  │ • Tool access     │
					│ • Manage conversation      │  │ • Add-ons         │
					│ • Parse tool calls         │  │ • Upgrades        │
					└────────────┬───────────────┘  └───────────────────┘
								 │
					┌────────────▼──────────────┐
					│ IMCPClient                 │
					├────────────────────────────┤
					│ • Invoke ServiceNow tools  │
					│ • Tool discovery           │
					│ • Parameter validation     │
					│ • Error handling           │
					└────────────┬───────────────┘
								 │
					┌────────────▼──────────────┐
					│ ServiceNow Tools Factory   │
					├────────────────────────────┤
					│ • CreateIncidentTool       │
					│ • SearchIncidentTool       │
					│ • CloseIncidentTool        │
					└────────────┬───────────────┘
								 │
					┌────────────▼──────────────┐
					│ ServiceNow Service         │
					├────────────────────────────┤
					│ • HTTP Client to SNOW      │
					│ • API Integration          │
					│ • Error handling           │
					└────────────────────────────┘
```

## Tier-Based Feature Access Flow

```
┌─────────────────────────────┐
│  Request from Tenant        │
│ (with X-Tenant-ID header)   │
└────────────────┬────────────┘
				 │
┌────────────────▼────────────────────┐
│ Load TenantContext                  │
│ • Tenant ID                         │
│ • Current Subscription Tier         │
│ • Available Tools & Models          │
│ • Quota Information                 │
└────────────────┬────────────────────┘
				 │
		 ┌───────┴───────┬────────────┬──────────┐
		 │               │            │          │
	┌────▼────┐    ┌─────▼────┐ ┌───▼────┐ ┌───▼────┐
	│   Free   │    │ Starter  │ │ Pro    │ │Enter-  │
	│   Tier   │    │  Tier    │ │ Tier   │ │ prise  │
	└────┬─────┘    └─────┬────┘ └───┬────┘ └───┬────┘
		 │                │           │         │
	┌────┴────────┐   ┌───┴─────┐ ┌──┴──────┐ ┌─┴──────┐
	│ Tools:      │   │ Tools:  │ │ Tools:  │ │ Tools: │
	│ Search ✓    │   │ All ✓   │ │ All ✓   │ │ All ✓  │
	│ Create ✗    │   │ Create ✓│ │ Create ✓│ │ All    │
	│ Close ✗     │   │ Close ✓ │ │ Close ✓ │ │ +SSO   │
	│             │   │         │ │         │ │ +DR    │
	│ Quota:      │   │ Quota:  │ │ Quota:  │ │ Quota: │
	│ 100/mo      │   │ 5K/mo   │ │ 50K/mo  │ │ ∞      │
	│             │   │ Models: │ │ Models: │ │ Models│
	│ Models:     │   │ 3.5,4 ✓ │ │ All ✓   │ │ All ✓  │
	│ 3.5-turbo ✓ │   │         │ │         │ │        │
	│             │   │ Features:   │ Features:   │        │
	│ Features:   │   │ • Custom    │• Webhooks   │ Features:    │
	│ None        │   │ • Analytics │• Multi-     │ • ALL        │
	│             │   │             │ agent       │ • Unlimited  │
	└─────────────┘   └─────────────┘└────────────┘└────────────┘
							 │
							 ▼
					┌──────────────────┐
					│ Permission Check │
					└────────┬─────────┘
							 │
					┌────────┴───────┐
					│        ✓        │
					│    Allowed      │
					└────────┬────────┘
							 │
					┌────────▼────────┐
					│ Process Request │
					│ with MCP Client │
					└────────┬────────┘
							 │
					┌────────▼────────┐
					│ Update Quota    │
					│ Track Usage     │
					└─────────────────┘
```

## Tenant Lifecycle

```
┌───────────────────────────────────────────────────────────────┐
│                     TENANT LIFECYCLE                           │
└───────────────────────────────────────────────────────────────┘

	   Register Tenant
			  │
			  ▼
	┌─────────────────┐
	│ Free Tier       │
	│ (Default)       │
	│ • 100 API/mo    │
	│ • Search only   │
	│ • No features   │
	└────────┬────────┘
			 │
	┌────────▼─────────┐
	│ Upgrade Path 1   │
	└────────┬─────────┘
			 │
	┌────────▼────────────────┐
	│ Starter Tier ($29/mo)   │
	│ • 5K API calls/mo       │
	│ • All tools             │
	│ • Custom prompts        │
	│ • Up to 3 team members  │
	└────────┬────────────────┘
			 │
	┌────────▼─────────────────┐
	│ Professional ($99/mo)    │
	│ • 50K API calls/mo       │
	│ • Multiple agents        │
	│ • Workflow orch.         │
	│ • Up to 10 team members  │
	└────────┬──────────────────┘
			 │
	┌────────▼────────────────────┐
	│ Enterprise (Custom)         │
	│ • Unlimited usage           │
	│ • All features              │
	│ • SSO, Data Residency       │
	│ • Unlimited team members    │
	│ • Dedicated support         │
	└────────┬─────────────────────┘
			 │
			 ▼
	┌─────────────────────┐
	│ Active Subscription │
	│ (Auto-renewal)      │
	└────────┬────────────┘
			 │
	┌────────▼─────────────────┐
	│ End of Billing Period    │
	└──────────┬────────────────┘
			   │
		┌──────┴──────┐
		│             │
   ┌────▼────┐   ┌────▼────┐
   │ Renew    │   │ Cancel  │
   └────┬─────┘   └────┬────┘
		│              │
		▼              ▼
	┌────────┐    ┌────────────┐
	│ Active │    │ Suspended  │
	└────────┘    │ (No access)│
				  └────────────┘
```

## Data Flow - Processing a Request

```
1. CLIENT REQUEST
   └─> POST /api/v1/tenants/{tenantId}/agent/ask
	   Headers: X-Tenant-ID, Authorization
	   Body: { "message": "Create incident" }


2. TENANT RESOLVER MIDDLEWARE
   └─> Extract tenant ID from header
   └─> Query database for TenantConfig
   └─> Query database for Subscription
   └─> Query database for Quota
   └─> Build TenantContext with feature flags


3. CONTROLLER RECEIVES REQUEST
   └─> Verify tenant context exists
   └─> Route to MultiTenantAIAgent


4. MULTITENANT AI AGENT
   └─> Validate subscription tier has API access
	   • Check: tier.AllowAPIAccess == true
   └─> Check quota not exceeded
	   • Check: monthlyUsed < monthlyLimit
   └─> Filter available tools based on tier
	   • For each tool in request:
		 - Get MinimumTier required
		 - Compare with tenant tier
		 - Only include if accessible
   └─> Call base IAIAgent.ProcessRequestAsync()


5. BASE AI AGENT
   └─> Add message to conversation history
   └─> Call ChatGPT API with:
	   • System prompt
	   • Conversation history
	   • Filtered tool definitions
   └─> Parse ChatGPT response
   └─> Extract tool calls


6. MCP CLIENT EXECUTION
   └─> For each tool call:
	   • Validate parameters
	   • Invoke tool (create/search/close incident)
	   • Collect results


7. RESPONSE BUILD-UP
   └─> Add assistant message to history
   └─> Compile tool execution results
   └─> Return to MultiTenantAIAgent


8. USAGE TRACKING
   └─> Increment usage counter
   └─> Store daily stats
   └─> Check if quota exceeded


9. RESPONSE TO CLIENT
   └─> Return AgentResponse
	   • Success status
	   • Message from AI
	   • Tool results
	   • Update conversation history
```

## Database Schema (Normalized)

```
Tenants
├─ TenantId (PK)
├─ TenantName
├─ ApiKey
├─ SecretKey
├─ CurrentTier (FK → SubscriptionTiers)
├─ IsActive
└─ CreatedDate

Subscriptions
├─ SubscriptionId (PK)
├─ TenantId (FK → Tenants)
├─ Tier (FK → SubscriptionTiers)
├─ StartDate
├─ EndDate
├─ RenewalDate
├─ BillingAmount
├─ Status (active|suspended|expired|cancelled)
└─ AutoRenew

Quotas
├─ TenantId (PK, FK → Tenants)
├─ MonthlyLimit
├─ MonthlyUsed
├─ DailyLimit
├─ DailyUsed
└─ ResetDate

UsageTracking (Daily)
├─ UsageId (PK)
├─ TenantId (FK → Tenants)
├─ Date
├─ ApiCallsUsed
├─ TokensUsed
├─ ToolUsageStats (JSON)
├─ ModelUsageStats (JSON)
└─ ConversationsCreated

CustomAgents
├─ AgentId (PK)
├─ TenantId (FK → Tenants)
├─ AgentName
├─ SystemPrompt
├─ AssignedTools (array)
├─ PreferredModel
├─ IsActive
├─ CreatedDate
└─ UpdatedDate

TeamMembers
├─ MemberId (PK)
├─ TenantId (FK → Tenants)
├─ Email
├─ Role (admin|user|viewer)
├─ IsActive
└─ JoinedDate

BillingRecords
├─ BillingId (PK)
├─ TenantId (FK → Tenants)
├─ SubscriptionId (FK → Subscriptions)
├─ Amount
├─ BillingDate
├─ DueDate
├─ Status (pending|paid|overdue|cancelled)
├─ PaymentMethod
└─ LineItems (JSON)
```

## Security Layers

```
┌────────────────────────────────────────────────────────────┐
│ Layer 1: Authentication                                    │
│ • API Key validation                                       │
│ • JWT claim verification (tenant_id)                       │
│ • Header validation (X-Tenant-ID)                          │
└────────────────────────────────────────────────────────────┘
						 │
						 ▼
┌────────────────────────────────────────────────────────────┐
│ Layer 2: Tenant Isolation                                  │
│ • Request routed by TenantId                               │
│ • Middleware sets context per request                      │
│ • Data queries filtered by TenantId                        │
└────────────────────────────────────────────────────────────┘
						 │
						 ▼
┌────────────────────────────────────────────────────────────┐
│ Layer 3: Feature Access Control                            │
│ • Tool availability by tier                                │
│ • Model availability restrictions                          │
│ • Feature flag checks                                      │
└────────────────────────────────────────────────────────────┘
						 │
						 ▼
┌────────────────────────────────────────────────────────────┐
│ Layer 4: Quota Enforcement                                 │
│ • Daily API call limits                                    │
│ • Monthly quota checks                                     │
│ • Concurrent request limits                                │
├─ Hard fail when exceeded                                  │
└────────────────────────────────────────────────────────────┘
						 │
						 ▼
┌────────────────────────────────────────────────────────────┐
│ Layer 5: Rate Limiting                                     │
│ • Per-tenant rate limits                                   │
│ • Per-tool rate limits                                     │
│ • Sliding window algorithm                                 │
└────────────────────────────────────────────────────────────┘
						 │
						 ▼
┌────────────────────────────────────────────────────────────┐
│ Layer 6: Audit Logging                                     │
│ • All operations logged with TenantId                       │
│ • Conversation history retention                            │
│ • Usage statistics tracking                                 │
└────────────────────────────────────────────────────────────┘
```
