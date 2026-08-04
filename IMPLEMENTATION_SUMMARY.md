# Implementation Summary: Entra ID OAuth, Role-Based Authorization, and ServiceNow Connector

## Overview

This document summarizes the implementation of Microsoft Entra ID (Azure AD) OAuth authentication, role-based authorization (RBAC), and enhanced ServiceNow connector integration into the TubieTools Aspire solution.

## Architecture

### Security Components

```
┌─────────────────────────────────────────────────────────┐
│                  Client Applications                    │
│  (Blazor Web, External APIs, ServiceNow)               │
└────────────────────┬────────────────────────────────────┘
					 │ Bearer Token (JWT)
					 ▼
┌─────────────────────────────────────────────────────────┐
│        Entra ID Authentication Middleware               │
│  • Extract bearer token from Authorization header       │
│  • Validate token against Entra ID signing keys         │
│  • Handle token expiration and revocation              │
└────────────────────┬────────────────────────────────────┘
					 │ ClaimsPrincipal with groups
					 ▼
┌─────────────────────────────────────────────────────────┐
│        Entra ID Claims Transformer                      │
│  • Extract group IDs from token                         │
│  • Map groups to application roles                      │
│  • Add role claims to principal                         │
└────────────────────┬────────────────────────────────────┘
					 │ ClaimsPrincipal with roles
					 ▼
┌─────────────────────────────────────────────────────────┐
│     Authorization Service & Policy Evaluation           │
│  • Evaluate role-based policies                         │
│  • Check claim-based requirements                       │
│  • Enforce operation-level authorization               │
└────────────────────┬────────────────────────────────────┘
					 │ Authorized/Denied
					 ▼
┌─────────────────────────────────────────────────────────┐
│   Business Logic (ServiceNow, Azure, etc.)              │
│  All operations are role-gated and audited             │
└─────────────────────────────────────────────────────────┘
```

## Components Created

### 1. TubieTools_Aspire.Security Project

New library project containing all security infrastructure:

#### Configuration (`Configuration/`)
- **EntraIdOptions.cs** - Configuration model for Entra ID integration
  - TenantId, ClientId, Authority
  - Token validation parameters
  - Role-to-group mapping

#### Claims Processing (`Claims/`)
- **EntraIdClaimsTransformer.cs** - Transforms Entra ID groups to app roles
  - Extracts group IDs from JWT groups claim
  - Maps groups to application roles using configuration
  - Adds role claims to ClaimsPrincipal

#### Authorization (`Authorization/`)
- **IAuthorizationService.cs** - Authorization policy service interface
- **AuthorizationService.cs** - Full implementation
  - Policy evaluation against roles/claims
  - Utility methods: HasRole, HasAnyRole, HasAllRoles
  - User info extraction (email, UPN, object ID)
- **AuthorizationAttributes.cs** - Controller authorization attributes
  - `[AuthorizePolicy]` - Policy-based authorization
  - `[AuthorizeRole]` - Role-based authorization
- **AuthorizationPolicy.cs** - Policy models and pre-defined policies
  - Admin, ServiceNow (CRUD), Tenant management

#### Middleware (`Middleware/`)
- **EntraIdAuthenticationMiddleware.cs** - Validates bearer tokens
  - Extracts tokens from Authorization header
  - Validates against Entra ID signing keys
  - Handles token expiration, signature, issuer validation
  - Excludes public paths (/health, /swagger, /auth/login)

#### Extensions (`Extensions/`)
- **SecurityMiddlewareExtensions.cs** - `UseEntraIdAuthentication()` extension

#### Health Checks (`Health/`)
- **EntraIdHealthCheck.cs** - Validates Entra ID connectivity
  - Verifies configuration completeness
  - Retrieves and validates signing keys
  - Reports health with key count

### 2. Updated ServiceDefaults Project

#### Extensions.cs additions
- **AddEntraIdAuthentication()** - Registers Entra ID auth and services
  - Configures JWT Bearer authentication
  - Registers IEntraIdClaimsTransformer
  - Registers IAuthorizationService
  - Sets up authorization policies
- **AddEntraIdHealthCheck()** - Adds Entra ID health check

#### Project File Updates
- Added authentication NuGet packages:
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - Microsoft.AspNetCore.Authentication.OpenIdConnect
- Added TubieTools_Aspire.Security project reference

### 3. Updated Web Applications

#### TubieTools_Aspire.EnterpriseAutomation
- **Program.cs changes:**
  - Added `builder.AddEntraIdAuthentication()`
  - Added middleware: `UseRouting()`, `UseAuthentication()`, `UseAuthorization()`, `UseEntraIdAuthentication()`
  - Registered Entra ID health check
  - Removed all Okta references
- **appsettings.EntraId.json** - Configuration template
- **ServiceNowService.cs enhancements:**
  - All CRUD operations now require authorization
  - GetIncidents, GetIncidentAsync - Requires `ServiceNow.Read` policy
  - CreateIncident, CreateChangeRequest - Requires `ServiceNow.Create` policy
  - UpdateIncident - Requires `ServiceNow.Update` policy
  - ApproveChange - Requires `ServiceNow.Admin` policy
  - Detailed audit logging with user identity

#### TubieTools_PublicAPI
- **Program.cs changes:**
  - Replaced Okta middleware with Entra ID
  - Added `builder.AddServiceDefaults()` and `builder.AddEntraIdAuthentication()`
  - Updated middleware pipeline for proper authentication order
  - Removed all Okta service registration
- **appsettings.EntraId.json** - Configuration template
- **Project file:** Added security and service defaults references

#### TubieTools_Aspire.Web (Blazor)
- **Program.cs changes:**
  - Added Entra ID authentication for interactive OIDC flow
  - Added `UseAuthentication()` and `UseAuthorization()` middleware
  - Integrated claims transformer
- **appsettings.EntraId.json** - Configuration template
- **Project file:** Added security and service defaults references

## Authorization Policies

Pre-defined policies available for use:

| Policy | Description | Required Roles | Use Case |
|--------|-------------|-----------------|----------|
| `AdminFullAccess` | Full admin access | `Admin` | Administrative operations |
| `ServiceNow.Create` | Create incidents | `Admin`, `ServiceNow.Creator`, `ServiceNow.Admin` | Create incident/change |
| `ServiceNow.Read` | Read incidents | `Admin`, `ServiceNow.Creator`, `ServiceNow.Reader`, `ServiceNow.Admin` | Query incidents |
| `ServiceNow.Update` | Update incidents | `Admin`, `ServiceNow.Admin` | Modify existing incidents |
| `ServiceNow.Delete` | Delete incidents | `Admin`, `ServiceNow.Admin` | Remove incidents |
| `ServiceNow.Admin` | ServiceNow admin | `Admin`, `ServiceNow.Admin` | Admin operations |
| `Tenant.Admin` | Tenant admin | `Admin`, `Tenant.Admin` | Tenant management |
| `Tenant.Read` | Read tenant data | Multiple roles | Query tenant |
| `Tenant.Write` | Modify tenant | Admin-level roles | Update tenant |

## Configuration

### Example Configuration Structure

```json
{
  "Authentication": {
	"EntraId": {
	  "TenantId": "your-tenant-id",
	  "ClientId": "your-app-id",
	  "ClientSecret": "${ENV_VARIABLE}",
	  "Authority": "https://login.microsoftonline.com/your-tenant-id/v2.0",
	  "RoleGroupMapping": {
		"entra-id-group-uuid": ["ApplicationRole1", "ApplicationRole2"]
	  }
	}
  }
}
```

### Environment Variables (Production)

```
Authentication__EntraId__TenantId = xxx
Authentication__EntraId__ClientId = xxx
Authentication__EntraId__ClientSecret = xxx
Authentication__EntraId__RoleGroupMapping__[group-id]__0 = RoleName
```

See `ENTRA_ID_CONFIGURATION.md` for complete setup guide.

## Integration Points

### 1. ServiceNow Connector
- All operations now validate user authorization
- Audit logging includes user identity and roles
- Returns 403 Forbidden if user lacks required role

### 2. Multi-Tenancy
- Tenant context preserved throughout auth pipeline
- Authorization middleware respects tenant isolation
- ServiceNow operations scoped to tenant data

### 3. Health Checks
- `/health` endpoint includes Entra ID status
- Validates signing key availability
- Reports connectivity to token endpoint

### 4. OpenTelemetry & Monitoring
- Authorization events logged at appropriate levels
- Failed authorization attempts logged as warnings
- User identity and roles included in audit trail

## Usage Examples

### Protecting API Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
	[HttpPost]
	[AuthorizePolicy(AuthorizationPolicies.ServiceNowCreate)]
	public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
	{
		return Ok(await _servicenowService.CreateIncidentAsync(request));
	}

	[HttpGet]
	[AuthorizeRole("ServiceNow.Reader", "Admin")]
	public async Task<IActionResult> GetIncidents()
	{
		return Ok(await _servicenowService.GetIncidentsAsync());
	}
}
```

### Programmatic Authorization

```csharp
public class MyService
{
	private readonly IAuthorizationService _authService;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public async Task<bool> CanUserDeleteIncidents()
	{
		var user = _httpContextAccessor.HttpContext?.User;
		return await _authService.AuthorizeAsync(user, AuthorizationPolicies.ServiceNowDelete);
	}

	public string GetCurrentUserEmail()
	{
		var user = _httpContextAccessor.HttpContext?.User;
		return _authService.GetUserEmail(user);
	}
}
```

## Security Considerations

### Token Security
- Tokens validated against Entra ID signing keys
- Token expiration enforced
- Signature verification required
- Clock skew tolerance: 30 seconds (configurable)

### Access Control
- Principle of least privilege via role groups
- Operation-level authorization via policies
- Audit logging for all operations
- Fails secure (denies on auth failure)

### Secret Management
- Never commit secrets to source control
- Use Azure Key Vault or similar secure storage
- Rotate secrets regularly
- Use managed identities where possible

## Deployment Considerations

### Development
1. Create test Entra ID groups
2. Set environment variables or use `appsettings.Development.json`
3. Update `RoleGroupMapping` with test group IDs
4. Test with bearer tokens from Entra ID

### Production
1. Store secrets in Azure Key Vault
2. Use managed identity for Azure services
3. Enable SSL certificate validation
4. Implement token caching for performance
5. Monitor health checks and authorization events
6. Set up alerts for authentication failures

### Backward Compatibility
- Old Okta middleware completely replaced
- Update API clients to use new Entra ID tokens
- Version API if needed for deprecation

## Performance Implications

- **Token Validation** - Cached signing keys reduce calls to Entra ID
- **Claims Transformation** - Runs per-request (overhead: ~5-10ms)
- **Authorization Checks** - In-memory policy evaluation (overhead: <1ms)
- **Health Checks** - Async, can be called independently

## Monitoring & Troubleshooting

### Key Metrics to Monitor
- Authorization policy evaluation time
- Percentage of denied requests
- Token validation failures
- Entra ID API call latency

### Health Endpoint
```bash
GET /health

Response includes:
- Overall status (Healthy/Unhealthy)
- Entra ID status and signing key count
- ServiceNow connectivity
- Azure connectivity
- Kubernetes connectivity
```

### Logging
Look for:
- `EntraIdAuthenticationMiddleware` - Token validation events
- `EntraIdClaimsTransformer` - Group-to-role mapping
- `AuthorizationService` - Policy evaluation
- `ServiceNowService` - Operation-level authorization

## Next Steps

1. **Configure Entra ID** - Follow `ENTRA_ID_CONFIGURATION.md`
2. **Test Integration** - Use provided PowerShell scripts
3. **Deploy to staging** - Verify in non-production first
4. **Monitor health** - Watch logs and health endpoints
5. **Audit access** - Review role assignments and permissions

## File Structure Reference

```
TubieTools_Aspire.Security/
├── Configuration/
│   └── EntraIdOptions.cs
├── Claims/
│   └── EntraIdClaimsTransformer.cs
├── Authorization/
│   ├── IAuthorizationService.cs
│   ├── AuthorizationService.cs
│   ├── AuthorizationAttributes.cs
│   └── AuthorizationPolicy.cs
├── Middleware/
│   └── EntraIdAuthenticationMiddleware.cs
├── Extensions/
│   └── SecurityMiddlewareExtensions.cs
├── Health/
│   └── EntraIdHealthCheck.cs
└── TubieTools_Aspire.Security.csproj

TubieTools_Aspire.ServiceDefaults/
├── Extensions.cs (updated with auth methods)
└── TubieTools_Aspire.ServiceDefaults.csproj

[Updated Projects]
├── TubieTools_Aspire.EnterpriseAutomation/
│   ├── Program.cs (updated)
│   ├── ServiceNow/ServiceNowService.cs (enhanced)
│   └── appsettings.EntraId.json
├── TubieTools_PublicAPI/
│   ├── Program.cs (updated)
│   └── appsettings.EntraId.json
└── TubieTools_Aspire.Web/
	├── Program.cs (updated)
	└── appsettings.EntraId.json

Documentation/
├── ENTRA_ID_CONFIGURATION.md (setup guide)
└── IMPLEMENTATION_SUMMARY.md (this file)
```

## Rollback Plan

If needed to revert to Okta:

1. Restore Okta middleware from source control
2. Remove Entra ID authentication calls from Program.cs
3. Remove security project references
4. Restore Okta configuration to appsettings.json
5. Redeploy

Note: Data/database changes are not affected by authentication switching.

## Support & Questions

Refer to:
- `ENTRA_ID_CONFIGURATION.md` - Setup and configuration
- Code comments in security components
- Microsoft Entra documentation
- Azure Key Vault documentation
