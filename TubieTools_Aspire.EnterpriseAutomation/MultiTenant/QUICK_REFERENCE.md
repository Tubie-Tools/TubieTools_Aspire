# Multi-Tenant AI Agent Platform - Quick Reference Guide

## 🚀 Quick Start (5 Minutes)

### 1. Register a Tenant
```bash
curl -X POST http://localhost:5000/api/v1/tenants/register \
  -H "Content-Type: application/json" \
  -d '{"tenantName": "MyCompany", "description": "Sales team automation"}'
```
**Response:** `{"tenantId": "uuid", "apiKey": "sk_..."}`

### 2. Make Your First Agent Request
```bash
curl -X POST http://localhost:5000/api/v1/tenants/{tenantId}/agent/ask \
  -H "X-Tenant-ID: {tenantId}" \
  -H "Content-Type: application/json" \
  -d '{"message": "Search for open incidents"}'
```

### 3. Check Your Quota
```bash
curl -X GET http://localhost:5000/api/v1/tenants/{tenantId}/usage \
  -H "X-Tenant-ID: {tenantId}"
```

---

## 📊 Subscription Tiers at a Glance

| Tier | Price | API Calls | Tools | Features |
|------|-------|-----------|-------|----------|
| **Free** | $0 | 100/mo | 1 | Basic |
| **Starter** | $29 | 5K/mo | 3 | +Analytics |
| **Professional** | $99 | 50K/mo | 3 | +Agents, +Hooks |
| **Enterprise** | Custom | ∞ | 3 | +SSO, +DR |

---

## 🔑 Authentication Methods

### Method 1: X-Tenant-ID Header
```bash
-H "X-Tenant-ID: your-tenant-id"
```

### Method 2: API Key Bearer Token
```bash
-H "Authorization: Bearer sk_your_api_key"
```

### Method 3: JWT Claim
Include `tenant_id` in JWT token claims

---

## 🛠️ Common Operations

### Upgrade Subscription
```bash
POST /api/v1/tenants/{tenantId}/upgrade
Body: {"newTier": 2}  # 0=Free, 1=Starter, 2=Pro, 3=Enterprise
```

### Create Custom Agent
```bash
POST /api/v1/tenants/{tenantId}/agents
Body: {
  "agentName": "Support Agent",
  "systemPrompt": "You help customers...",
  "assignedTools": ["search_incident", "create_incident"],
  "preferredModel": "gpt-4"
}
```

### Add Team Member
```bash
POST /api/v1/tenants/{tenantId}/team
Body: {
  "email": "user@company.com",
  "role": "admin"  # admin, user, or viewer
}
```

### Get Available Tiers
```bash
GET /api/v1/tenants/tiers
```

---

## 📈 Usage Tracking

### View Usage Stats
```bash
GET /api/v1/tenants/{tenantId}/usage?daysBack=30
```

Returns:
```json
{
  "quota": {
	"monthlyApiCallLimit": 5000,
	"monthlyApiCallsUsed": 2345,
	"quotaExceeded": false
  },
  "usage": [
	{
	  "date": "2024-01-20",
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

---

## 🎯 Feature Access by Tier

| Feature | Free | Starter | Pro | Enterprise |
|---------|------|---------|-----|-----------|
| Search Tool | ✅ | ✅ | ✅ | ✅ |
| Create Tool | ❌ | ✅ | ✅ | ✅ |
| Close Tool | ❌ | ✅ | ✅ | ✅ |
| Custom Prompts | ❌ | ✅ | ✅ | ✅ |
| Multiple Agents | ❌ | ❌ | ✅ | ✅ |
| Webhook Support | ❌ | ❌ | ✅ | ✅ |
| SSO | ❌ | ❌ | ❌ | ✅ |

---

## ⚠️ Quota Enforcement

### What Happens When Quota is Exceeded?

1. **Daily Limit Exceeded** → Blocked for 24 hours
2. **Monthly Limit Exceeded** → Blocked until month resets
3. **Concurrent Requests Exceeded** → Request queued (if tier supports)

### Reset Schedule
- **Daily:** 00:00 UTC
- **Monthly:** 1st of month, 00:00 UTC

### Upgrading to Remove Limits
```
Free → Starter:  100 → 5,000 calls/month
Starter → Pro:   5K → 50,000 calls/month
Pro → Enterprise: 50K → Unlimited calls/month
```

---

## 🤖 AI Agent Best Practices

### Good Request
```json
{
  "message": "Create a high-priority incident about the production database being down with description 'Database connection failed at 3:45 PM EDT'"
}
```

### Better Request with Prompts
Upgrade to **Starter tier** to use custom prompts:
```bash
POST /api/v1/tenants/{tenantId}/agents
{
  "agentName": "DBOpsAgent",
  "systemPrompt": "You are a database operations expert. Always use priority=1 for database issues...",
  "assignedTools": ["create_incident", "search_incident"],
  "preferredModel": "gpt-4"
}
```

---

## 🔐 Security Checklist

- [x] Always use HTTPS in production
- [x] Never commit API keys to version control
- [x] Rotate API keys monthly
- [x] Use X-Tenant-ID or Bearer token consistently
- [x] Enable audit logging for compliance
- [x] Monitor quota usage to detect anomalies
- [x] Use strong TLS 1.2+

---

## 💰 Pricing & Billing

### Monthly Invoice Includes
1. Base tier subscription fee
2. Active add-ons
3. Overage charges (Pro only)
4. Usage-based fees (if applicable)

### Supported Billing Intervals
- Monthly: Charge on same day each month
- Annual: Discount applies (save ~10%)

### Payment Methods
- Credit Card
- Bank Transfer / ACH
- Invoice (Enterprise only)

---

## 🐛 Troubleshooting

### "Tenant not found or inactive"
```
→ Check X-Tenant-ID header matches your tenantId
→ Ensure subscription is active
→ Verify tenant was created successfully
```

### "API quota exceeded"
```
→ Upgrade to higher tier for more calls
→ Check usage stats with /usage endpoint
→ Wait for daily/monthly quota reset
```

### "No tools available for your subscription tier"
```
→ Free tier only has search_incident
→ Upgrade to Starter for all tools
→ Or use Starter up to create/close
```

### "Custom prompts not available"
```
→ Upgrade to Starter tier minimum
→ Create custom agent with system prompt
```

### "Cannot create multiple agents"
```
→ Only Professional/Enterprise tiers support this
→ Upgrade your subscription
```

---

## 📚 Documentation Links

| Document | Purpose |
|----------|---------|
| `README.md` | Full API documentation & architecture |
| `ARCHITECTURE.md` | Detailed system design with diagrams |
| `IMPLEMENTATION_SUMMARY.md` | Project completion overview |
| `subscription-tiers.json` | Tier configurations |
| `multi-tenant-api-schema.json` | API specifications |
| `sample-tenants.json` | Example data & usage |

---

## 🔄 API Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation completed",
  "result": { ... },
  "executedTools": ["tool_name"],
  "conversationHistory": [
	{
	  "role": "user",
	  "content": "Your message",
	  "timestamp": "2024-01-20T10:30:00Z"
	}
  ]
}
```

### Error Response
```json
{
  "success": false,
  "message": "Descriptive error message",
  "error": "error_code"
}
```

---

## 🚨 Rate Limits

| Tier | Requests/Min | Requests/Day | Requests/Month |
|------|--------------|--------------|----------------|
| Free | 5 | 20 | 100 |
| Starter | 30 | 200 | 5,000 |
| Professional | 100 | 2,000 | 50,000 |
| Enterprise | 1,000 | Unlimited | Unlimited |

---

## 📞 Support

### Getting Help
- **Documentation:** See ARCHITECTURE.md & README.md
- **Issues:** GitHub Issues or support@example.com
- **Feature Requests:** GitHub Discussions
- **Enterprise Support:** contact-sales@example.com

### Response Times
- **Free Tier:** 24-48 hours
- **Starter:** 12-24 hours
- **Professional:** 4-8 hours
- **Enterprise:** 1 hour SLA

---

## 🎓 Learning Path

### Beginner (5 min)
1. Register tenant
2. Make first API request
3. Check quota usage

### Intermediate (20 min)
1. Create custom agent
2. Upgrade tier
3. Add team members
4. Monitor usage stats

### Advanced (1 hour)
1. Build workflow with orchestration (Pro+)
2. Setup webhooks (Pro+)
3. Implement custom integrations (Enterprise)
4. Configure SSO (Enterprise)

---

## ✨ Pro Tips

1. **Test with Free Tier First**
   - No credit card required
   - 100 free API calls/month
   - All features available to try

2. **Upgrade Strategically**
   - Calculate your usage
   - Choose tier 30% above current use
   - Can downgrade any time (prorated refund)

3. **Use Custom Agents for Specialization**
   - Different agents for different tasks
   - Professional tier included
   - Each agent can have custom system prompt

4. **Monitor Usage Daily**
   - Avoid surprise quota limits
   - Plan for growth
   - Upgrade before hitting limits

5. **Archive Old Conversations**
   - Free: 7 days retention
   - Starter: 30 days
   - Export before retention expires (Pro+)

---

## 🔗 Common Recipes

### Recipe 1: Basic Incident Search
```bash
curl -X POST http://localhost:5000/api/v1/tenants/{ID}/agent/ask \
  -H "X-Tenant-ID: {ID}" \
  -d '{"message": "Find all high priority incidents created today"}'
```

### Recipe 2: Automated Incident Creation
```bash
curl -X POST http://localhost:5000/api/v1/tenants/{ID}/agent/ask \
  -H "X-Tenant-ID: {ID}" \
  -d '{"message": "Create urgent incident: API response time exceeded 5 seconds"}'
```

### Recipe 3: Batch Close Incidents
```bash
curl -X POST http://localhost:5000/api/v1/tenants/{ID}/agent/ask \
  -H "X-Tenant-ID: {ID}" \
  -d '{"message": "Close all resolved incidents from yesterday"}'
```

### Recipe 4: Workflow Automation (Pro+ only)
Create custom agent → assign all tools → orchestrate multi-step workflows

---

## Version & Updates

- **Current Version:** 1.0.0
- **Last Updated:** 2024-01-20
- **Status:** Production Ready
- **Next Features:** Stripe integration, advanced analytics dashboard

---

**Happy automating! 🎉**
