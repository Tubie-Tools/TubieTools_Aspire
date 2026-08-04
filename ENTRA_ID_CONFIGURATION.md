# Entra ID OAuth & Role-Based Authorization Configuration Guide

## Overview

This guide explains how to configure Microsoft Entra ID (Azure AD) OAuth2 and role-based authorization in the TubieTools Aspire solution.

## Prerequisites

1. **Azure Subscription** - with Entra ID tenant
2. **Application Registration** - create app registrations in Entra ID
3. **Group Setup** - create Entra ID security groups for role mapping
4. **ServiceNow Integration** - existing ServiceNow connector

## Step 1: Create Azure App Registrations

### 1.1 Register Web API Application (for EnterpriseAutomation & PublicAPI)

1. Go to Azure Portal → Entra ID → App registrations
2. Click "New registration"
3. Name: `TubieTools-API` (or similar)
4. Supported account types: "Multi-tenant" (adjust to your needs)
5. Redirect URI: Leave blank for now (APIs use Bearer tokens)
6. Click "Register"

**Note the following values:**
- Application (client) ID → `YOUR_CLIENT_ID`
- Directory (tenant) ID → `YOUR_TENANT_ID`

### 1.2 Configure API Permissions

1. In your registered app, go to "API permissions"
2. Click "Add a permission" → "Microsoft Graph" → "Application permissions"
3. Search for and add:
   - `Directory.Read.All` (to read group membership)
   - `User.Read.All` (optional, to read user info)
4. Click "Grant admin consent for [your tenant]"

### 1.3 Create Client Secret (for backend services)

1. Go to "Certificates & secrets"
2. Click "New client secret"
3. Description: `TubieTools-API-Secret`
4. Expiration: 24 months (adjust as needed)
5. Copy the **Value** → Store in Key Vault/Secrets Manager

**SECURITY WARNING:** Never commit secrets to source control!

### 1.4 Create Azure App Registration for Blazor Web App

Repeat steps 1.1-1.3, but:
- Name: `TubieTools-Web`
- Redirect URI: `https://localhost:7283/signin-oidc` (development)
- Additional redirect URIs for production: `https://your-domain.com/signin-oidc`

## Step 2: Create Entra ID Security Groups

1. Go to Azure Portal → Entra ID → Groups
2. Create new groups and note their Object IDs:

```
Group Name: TubieTools-Admins
Object ID: 12345678-1234-1234-1234-123456789abc

Group Name: TubieTools-ServiceNow-Creators
Object ID: 87654321-4321-4321-4321-abcdef123456

Group Name: TubieTools-ServiceNow-Readers
Object ID: 11111111-2222-3333-4444-555555555555

Group Name: TubieTools-Users
Object ID: 99999999-8888-7777-6666-555555555444
```

Assign users to these groups as needed.

## Step 3: Configure Application Settings

### 3.1 Environment Variables (Production - Recommended)

Set these environment variables in your deployment environment:

```powershell
# Entra ID Configuration
$env:Authentication__EntraId__TenantId = "YOUR_TENANT_ID"
$env:Authentication__EntraId__ClientId = "YOUR_CLIENT_ID"
$env:Authentication__EntraId__ClientSecret = "YOUR_CLIENT_SECRET"
$env:Authentication__EntraId__Authority = "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0"
$env:Authentication__EntraId__Scope = "api://YOUR_CLIENT_ID/.default"

# Role Group Mapping (Groups to Roles)
$env:Authentication__EntraId__RoleGroupMapping__12345678-1234-1234-1234-123456789abc__0 = "Admin"
$env:Authentication__EntraId__RoleGroupMapping__12345678-1234-1234-1234-123456789abc__1 = "ServiceNow.Admin"
$env:Authentication__EntraId__RoleGroupMapping__87654321-4321-4321-4321-abcdef123456__0 = "ServiceNow.Creator"
$env:Authentication__EntraId__RoleGroupMapping__87654321-4321-4321-4321-abcdef123456__1 = "ServiceNow.Read"
```

### 3.2 Configuration File (Development - Not Production!)

Edit `appsettings.json` or `appsettings.Development.json`:

```json
{
  "Authentication": {
	"EntraId": {
	  "TenantId": "12345678-1234-1234-1234-123456789abc",
	  "ClientId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
	  "ClientSecret": "${ENTRA_CLIENT_SECRET}",
	  "Authority": "https://login.microsoftonline.com/12345678-1234-1234-1234-123456789abc/v2.0",
	  "Scope": "api://aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee/.default",
	  "ValidateCertificate": true,
	  "TokenValidation": {
		"ValidateLifetime": true,
		"ValidateSignature": true,
		"ValidateIssuer": true,
		"ValidateAudience": false,
		"Issuer": "https://login.microsoftonline.com/12345678-1234-1234-1234-123456789abc/v2.0",
		"Audience": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
		"ClockSkewSeconds": 30
	  },
	  "RoleGroupMapping": {
		"12345678-1234-1234-1234-123456789abc": [ "Admin", "ServiceNow.Admin" ],
		"87654321-4321-4321-4321-abcdef123456": [ "ServiceNow.Creator", "ServiceNow.Read" ],
		"11111111-2222-3333-4444-555555555555": [ "ServiceNow.Reader" ],
		"99999999-8888-7777-6666-555555555444": [ "User" ]
	  }
	}
  }
}
```

### 3.3 Using Azure Key Vault for Secrets

In production, store secrets in Azure Key Vault:

```csharp
// In Program.cs
var keyVaultUrl = new Uri($"https://{keyVaultName}.vault.azure.net/");
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
```

Key Vault Structure:
```
entra-id-tenant-id: 12345678-1234-1234-1234-123456789abc
entra-id-client-id: aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee
entra-id-client-secret: [actual secret]
```

## Step 4: Authorization Policies & Roles

### Available Authorization Policies

| Policy ID | Description | Required Roles |
|-----------|-------------|-----------------|
| `AdminFullAccess` | Full administrative access | `Admin` |
| `ServiceNow.Create` | Create incidents in ServiceNow | `Admin`, `ServiceNow.Creator`, `ServiceNow.Admin` |
| `ServiceNow.Read` | Read incidents from ServiceNow | `Admin`, `ServiceNow.Creator`, `ServiceNow.Reader`, `ServiceNow.Admin` |
| `ServiceNow.Update` | Update incidents in ServiceNow | `Admin`, `ServiceNow.Admin` |
| `ServiceNow.Delete` | Delete incidents from ServiceNow | `Admin`, `ServiceNow.Admin` |
| `ServiceNow.Admin` | Full admin access to ServiceNow | `Admin`, `ServiceNow.Admin` |

### Using Authorization Attributes in Controllers

```csharp
using TubieTools_Aspire.Security.Authorization;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
	[HttpPost("create")]
	[AuthorizePolicy(AuthorizationPolicies.ServiceNowCreate)]
	public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentRequest request)
	{
		// Only users with ServiceNow.Creator or Admin roles can access this
		return Ok(await _servicenowService.CreateIncidentAsync(request));
	}

	[HttpGet]
	[AuthorizeRole("ServiceNow.Reader", "ServiceNow.Creator", "Admin")]
	public async Task<IActionResult> GetIncidents()
	{
		// Any one of these roles is sufficient
		return Ok(await _servicenowService.GetIncidentsAsync());
	}
}
```

## Step 5: Testing the Integration

### 5.1 Get an Access Token

```bash
# PowerShell
$clientId = "YOUR_CLIENT_ID"
$clientSecret = "YOUR_CLIENT_SECRET"
$tenantId = "YOUR_TENANT_ID"

$tokenEndpoint = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"

$body = @{
	grant_type    = "client_credentials"
	client_id     = $clientId
	client_secret = $clientSecret
	scope         = "api://$clientId/.default"
}

$response = Invoke-RestMethod -Method Post -Uri $tokenEndpoint -Body $body
$accessToken = $response.access_token

Write-Output "Access Token: $accessToken"
```

### 5.2 Test Authorization

```bash
# Test with Authorization header
$headers = @{
	"Authorization" = "Bearer $accessToken"
	"Content-Type"  = "application/json"
}

# Test ServiceNow Create (should succeed if user has ServiceNow.Creator role)
Invoke-RestMethod -Method Post `
	-Uri "https://localhost:7000/api/incidents/create" `
	-Headers $headers `
	-Body (@{ title = "Test Incident"; description = "Test" } | ConvertTo-Json)

# Test without token (should return 401)
Invoke-RestMethod -Method Get `
	-Uri "https://localhost:7000/api/incidents"
```

### 5.3 Check Health

```bash
curl https://localhost:7000/health

# Response includes entra-id health status
```

## Step 6: ServiceNow Integration

ServiceNow operations are now controlled by role-based authorization:

- **Create Incidents** - Requires `ServiceNow.Creator` or `Admin`
- **Read Incidents** - Requires `ServiceNow.Reader`, `ServiceNow.Creator`, or `Admin`
- **Update Incidents** - Requires `ServiceNow.Admin` or `Admin`
- **Delete Incidents** - Requires `ServiceNow.Admin` or `Admin`

### Configure ServiceNow Connection

Update `appsettings.json`:

```json
{
  "ServiceNow": {
	"Instance": "dev12345",
	"Token": "${SERVICENOW_API_TOKEN}",
	"User": "api_user",
	"Password": "${SERVICENOW_PASSWORD}"
  }
}
```

## Security Best Practices

1. **Secrets Management**
   - Use Azure Key Vault or similar secure storage
   - Never commit secrets to version control
   - Rotate secrets regularly

2. **Token Validation**
   - Always validate token expiration
   - Always validate token signature
   - Validate issuer and audience

3. **Logging and Monitoring**
   - Log authorization failures
   - Monitor failed authentication attempts
   - Use Application Insights for production monitoring

4. **RBAC Principles**
   - Use principle of least privilege
   - Assign users to groups rather than individual roles
   - Regularly audit group membership

5. **Multi-Tenant Support**
   - Tenant context is preserved in authorization middleware
   - Each tenant's users only see their own data
   - ServiceNow operations are tenant-aware

## Troubleshooting

### Issue: "Unauthorized" when calling API

**Solution:**
1. Verify access token is included in `Authorization: Bearer <token>` header
2. Check token expiration with `jwt.io`
3. Verify user is member of required group in Entra ID
4. Check `RoleGroupMapping` configuration is correct

### Issue: "Forbidden" when calling ServiceNow operations

**Solution:**
1. Verify user is member of required role group
2. Check `Authorization__EntraId__RoleGroupMapping__` environment variables
3. Verify policy is correctly applied to controller action

### Issue: Entra ID health check fails

**Solution:**
1. Verify `TenantId` and `ClientId` are correct
2. Check network connectivity to `login.microsoftonline.com`
3. Verify firewall/proxy allows HTTPS to token endpoint

### Issue: Token validation errors

**Solution:**
1. Verify token is valid JWT with `jwt.io`
2. Check token not yet expired
3. Verify `ValidateAudience` setting. If false, audience validation is skipped
4. Check `ClockSkewSeconds` if time sync issues exist

## Additional Resources

- [Microsoft Entra ID Documentation](https://learn.microsoft.com/en-us/entra/identity-platform/)
- [OAuth 2.0 Client Credentials Flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-client-creds-grant-flow)
- [JWT Tokens](https://jwt.io)
- [Azure Key Vault Integration](https://learn.microsoft.com/en-us/azure/key-vault/)

## Support

For issues or questions, contact your development team or refer to the codebase documentation.
