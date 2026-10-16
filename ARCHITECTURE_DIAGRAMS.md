# Authentication Architecture - Visual Diagrams

## 1. Before Fix (Broken Flow)

```
┌─────────────────────────────────────────────────────────────────┐
│                          BROKEN FLOW                            │
└─────────────────────────────────────────────────────────────────┘

USER
  │
  ├─ Has Entra ID credentials (username/password)
  │
  ▼
┌──────────────────────────────────────────────────────┐
│  TubieTools_Aspire.Web                               │
│  ┌────────────────────────────────────────────────┐  │
│  │ User authenticates (OIDC)                      │  │
│  │ Web app gets EntraID access token              │  │
│  └────────────────────────────────────────────────┘  │
└──────────┬───────────────────────────┬──────────────┘
		   │                           │
		   │ Token stored in context   │
		   │ (from OIDC middleware)    │
		   │                           │
		   ▼                           │
	┌──────────────┐                   │
	│ Token Cache  │                   │
	│ (In Memory)  │                   │
	└──────────────┘                   │
									   │
									   │ PublicApiClient.GetCareProvidersAsync()
									   │
									   ▼ ❌ NO AUTHORIZATION HEADER
									┌──────────────────────────────┐
									│ HTTP GET /api/careproviders  │
									│ (headers empty)               │
									└──────────────────────────────┘
									   │
									   ▼
┌──────────────────────────────────────────────────────┐
│  TubieTools_PublicAPI                                │
│  ┌────────────────────────────────────────────────┐  │
│  │ EntraIdAuthenticationMiddleware                │  │
│  │                                                 │  │
│  │  bool skipcheck = true;  ❌ BYPASS ENABLED     │  │
│  │  if(skipcheck) {                                │  │
│  │      await _next(context);  // SKIP VALIDATION  │  │
│  │      return;                                    │  │
│  │  }                                              │  │
│  │  // Validation code never runs                 │  │
│  └────────────────────────────────────────────────┘  │
│                                                       │
│  ┌────────────────────────────────────────────────┐  │
│  │ CareProvidersController                        │  │
│  │ [AllowAnonymous]                               │  │
│  │ Returns: OK ✓ (but no user context)           │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘

RESULT:
  ✗ API works but with NO SECURITY
  ✗ No authentication validation
  ✗ Anyone can call API (even without token)
  ✗ User context = Anonymous
```

---

## 2. After Fix (Working Flow)

```
┌─────────────────────────────────────────────────────────────────┐
│                      WORKING FLOW (FIXED)                       │
└─────────────────────────────────────────────────────────────────┘

USER
  │
  ├─ Has Entra ID credentials (username/password)
  │
  ▼
┌──────────────────────────────────────────────────────┐
│  TubieTools_Aspire.Web (Program.cs)                  │
│  ┌────────────────────────────────────────────────┐  │
│  │ AddAuthentication("MultiScheme")                │  │
│  │  .AddJwtBearer("Bearer", options => {           │  │
│  │    Authority = "https://login.microsoftonline" │  │
│  │    SaveToken = true  ✓                          │  │
│  │  })                                              │  │
│  │  .AddCookie("Interactive")                      │  │
│  └────────────────────────────────────────────────┘  │
└──────────┬───────────────────────────┬──────────────┘
		   │                           │
	┌──────▼───────────┐               │
	│ User Login       │               │
	│ OIDC Flow        │               │
	│ ✓ Authenticated  │               │
	└──────┬───────────┘               │
		   │                           │
		   │ Entra ID issues           │
		   │ access_token              │
		   │                           │
		   ▼                           │
	┌──────────────┐                   │
	│ Token Cache  │                   │
	│ (In Memory)  │                   │  ← SaveToken=true saves here
	│ access_token │◄──────────────────┘
	│ expires_in   │
	└──────────────┘
		   │
		   │
		   │ PublicApiClient.GetCareProvidersAsync()
		   │
		   ▼
	┌───────────────────────────────────────────┐
	│ TokenForwardingDelegatingHandler           │
	│ ┌───────────────────────────────────────┐  │
	│ │ protected override SendAsync() {      │  │
	│ │   var accessToken =                   │  │
	│ │     GetTokenAsync("access_token")  ✓  │  │
	│ │   request.Headers.Authorization =     │  │
	│ │     Bearer <accessToken>  ✓           │  │
	│ │ }                                     │  │
	│ └───────────────────────────────────────┘  │
	└──────────┬────────────────────────────────┘
			   │
			   ▼ ✓ WITH AUTHORIZATION HEADER
			┌───────────────────────────────────────┐
			│ HTTP GET /api/careproviders           │
			│ Authorization: Bearer eyJhbGciOiJ...  │
			│ Content-Type: application/json        │
			└─────────────┬───────────────────────┘
						  │
						  ▼
┌──────────────────────────────────────────────────────┐
│  TubieTools_PublicAPI                                │
│  ┌────────────────────────────────────────────────┐  │
│  │ EntraIdAuthenticationMiddleware                │  │
│  │                                                 │  │
│  │  ✓ skipcheck = REMOVED (Validation ENABLED)   │  │
│  │                                                 │  │
│  │  var token = ExtractBearerToken(authHeader) ✓ │  │
│  │  var principal = ValidateTokenAsync(token) ✓  │  │
│  │    ├─ Fetch signing keys from Entra ID        │  │
│  │    ├─ Validate signature                      │  │
│  │    ├─ Validate issuer                         │  │
│  │    ├─ Validate expiration                     │  │
│  │    └─ Extract claims (email, groups, etc)     │  │
│  │                                                 │  │
│  │  context.User = principal  ✓                   │  │
│  │  (User context now populated)                  │  │
│  └────────────────────────────────────────────────┘  │
│                                                       │
│  ┌────────────────────────────────────────────────┐  │
│  │ CareProvidersController                        │  │
│  │                                                 │  │
│  │ User = authenticated with all claims:          │  │
│  │   - sub: "user-id-guid"                       │  │
│  │   - email: "user@company.com"                 │  │
│  │   - groups: ["users-group-id", ...]          │  │
│  │   - roles: ["User", "Admin"] (from mapping)   │  │
│  │                                                 │  │
│  │ Returns: OK ✓ + user context available         │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘

RESULT:
  ✓ API requests validated with proper security
  ✓ Token signature verified against Entra ID keys
  ✓ Token expiration checked
  ✓ User context populated (claims accessible)
  ✓ Audit trail of authenticated requests
```

---

## 3. Token Flow Sequence Diagram

```
┌────────┐          ┌──────────┐          ┌─────────────┐         ┌──────────┐
│  User  │          │Web App   │          │ Token       │         │PublicAPI │
│Browser │          │Blazor    │          │Forwarding   │         │          │
└───┬────┘          └────┬─────┘          └──────┬──────┘         └────┬─────┘
	│                    │                       │                      │
	│ 1. Login Request   │                       │                      │
	├────────────────────►                       │                      │
	│                    │                       │                      │
	│                    │ 2. Redirect to Entra ID (OAuth)              │
	│◄────────────────────────────────────────────────────────────────┘
	│                    │                       │                      │
	│ 3. Authenticate with credentials           │                      │
	► (username/password)                        │                      │
	│                    │                       │                      │
	│                    │ 4. Entra ID validates & issues token          │
	│◄────────────────────────────────────────────────────────────────┘
	│                    │                       │                      │
	│ 5. Redirect back to Web with code         │                      │
	│◄────────────────────                       │                      │
	│                    │                       │                      │
	│                    │ 6a. Exchange code for access_token            │
	│                    │────────────────┐                              │
	│                    │ (backend call) │                              │
	│                    │◄───────────────┘                              │
	│                    │                       │                      │
	│                    │ 7. Save token to memory cache                 │
	│                    │    + Set session cookie                       │
	│                    │ (SaveToken=true)                             │
	│                    │                       │                      │
	│                    │ 8. Redirect to protected page                │
	│◄────────────────────                       │                      │
	│                    │                       │                      │
	│ 9. Request protected resource              │                      │
	├────────────────────►                       │                      │
	│                    │ (cookie included)    │                      │
	│                    │                       │                      │
	│                    │ 10. Extract token from cache                 │
	│                    │────────────────────────►                     │
	│                    │                       │                      │
	│                    │ 11. HTTP Request      │                      │
	│                    │ with Authorization:   │                      │
	│                    │ Bearer <token>        │                      │
	│                    │────────────────────────────────────────────►│
	│                    │                       │                      │
	│                    │                       │ 12. Validate token  │
	│                    │                       │     • Check signature
	│                    │                       │     • Check expiration
	│                    │                       │     • Check issuer
	│                    │                       │     • Extract claims
	│                    │                       │                      │
	│                    │                       │ 13. Token valid ✓   │
	│                    │                       │────────────────────►
	│                    │                       │                      │
	│                    │                       │ 14. Return 200 OK   │
	│                    │                       │ + user context      │
	│                    │◄────────────────────────────────────────────│
	│                    │                       │                      │
	│ 15. Render response                        │                      │
	│◄────────────────────                       │                      │
	│                    │                       │                      │

FLOW SUMMARY:
Step 1-8:   Initial authentication (one-time)
Step 9-15:  Subsequent API calls (reuses token)
```

---

## 4. Component Interaction Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                        .NET Aspire App                         │
│                                                                │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ TubieTools_Aspire.Web                           ┌────┐ │  │
│  │ ┌──────────────────────────────────────────────┐│User││ │  │
│  │ │ Razor Component Page (Shop.razor)            ││    ││ │  │
│  │ │                                               ││ 📦 ││ │  │
│  │ │  GetCareProvidersAsync()                      └────┘ │  │
│  │ │          │                                          │  │
│  │ │          ▼                                          │  │
│  │ │ ┌──────────────────────────────────────────────┐   │  │
│  │ │ │ PublicApiClient (DI Injected)               │   │  │
│  │ │ │ _httpClient.GetAsync("/api/careproviders")  │   │  │
│  │ └─┤ Uses: HttpClientFactory                     │   │  │
│  │   └──────────────────┬───────────────────────────┘   │  │
│  │                      │                               │  │
│  │                      ▼                               │  │
│  │   ┌────────────────────────────────────────────┐    │  │
│  │   │     HttpClientFactory (Program.cs)          │    │  │
│  │   │  ┌─────────────────────────────────────┐   │    │  │
│  │   │  │ WithTokenForwarding()                │   │    │  │
│  │   │  │  .AddHttpMessageHandler<            │   │    │  │
│  │   │  │   TokenForwardingDelegatingHandler>()    │    │  │
│  │   └──┤                                      │   │    │  │
│  │      └────────────┬──────────────────────────┘   │  │
│  │                   │                               │  │
│  │                   ▼                               │  │
│  │   ┌────────────────────────────────────────────┐    │  │
│  │   │ TokenForwardingDelegatingHandler            │    │  │
│  │   │  ┌──────────────────────────────────────┐  │    │  │
│  │   │  │ GetCurrentUserToken()                │  │    │  │
│  │   │  │  • Access IHttpContextAccessor        │  │    │  │
│  │   │  │  • GetTokenAsync("access_token")      │  │    │  │
│  │   │  │  • Return: Bearer token               │  │    │  │
│  │   │  └──────────────────────────────────────┘  │    │  │
│  │   │  ┌──────────────────────────────────────┐  │    │  │
│  │   │  │ ForwardToken(httpRequest)            │  │    │  │
│  │   │  │  • Add to request.Headers.Authorization   │    │  │
│  │   │  └──────────────────────────────────────┘  │    │  │
│  │   └────────────────────┬─────────────────────────┘   │  │
│  │                        │                            │  │
│  │                        │ HTTP Pipeline              │  │
│  └────────────────────────┼────────────────────────────┘  │
│                           │                                │
│                           ▼ (Network)                      │
│                                                            │
│  ┌────────────────────────────────────────────────────┐  │
│  │ TubieTools_PublicAPI                              │  │
│  │ ┌──────────────────────────────────────────────┐  │  │
│  │ │ HTTP Request Processing Pipeline             │  │  │
│  │ │                                               │  │  │
│  │ │  1. app.UseRouting()                          │  │  │
│  │ │  2. app.UseAuthentication()                   │  │  │
│  │ │  3. app.UseAuthorization()                    │  │  │
│  │ │  4. app.UseEntraIdAuthentication()  ◄─────┐  │  │  │
│  │ │                                            │  │  │  │
│  │ │  5. Middleware: EntraIdAuthenticationMiddleware
│  │ │     ┌──────────────────────────────────────┐  │  │  │
│  │ │     │ Validation Sequence:                 │  │  │  │
│  │ │     │  ✓ Extract Authorization header      │  │  │  │
│  │ │     │  ✓ Extract Bearer token              │  │  │  │
│  │ │     │  ✓ Validate signature (Entra ID keys)│  │  │  │
│  │ │     │  ✓ Validate expiration               │  │  │  │
│  │ │     │  ✓ Validate issuer                   │  │  │  │
│  │ │     │  ✓ Extract claims                    │  │  │  │
│  │ │     │  ✓ Set context.User                  │  │  │  │
│  │ │     └──────────────────────────────────────┘  │  │  │
│  │ │                                               │  │  │
│  │ │  6. app.MapControllers()                      │  │  │
│  │ │                                               │  │  │
│  │ │  CareProvidersController                      │  │  │
│  │ │  [HttpGet("api/careproviders")]               │  │  │
│  │ │  public async Task<> GetAllProviders() {      │  │  │
│  │ │    // User = context.User (now populated)     │  │  │
│  │ │    // Can access claims & roles               │  │  │
│  │ │    return Ok(providers);                      │  │  │
│  │ │  }                                            │  │  │
│  │ └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                                                            │
└────────────────────────────────────────────────────────────┘

SECURITY CHECKPOINTS:
  ① Web App: User authenticated via OIDC
  ② Web App: Token extracted from memory cache
  ③ Request: Token added as Bearer header
  ④ PublicAPI: Token signature validated against Entra ID keys
  ⑤ PublicAPI: Token not expired + issuer verified
  ⑥ Controller: User context available for audit/authorization
```

---

## 5. Error Scenarios

### Scenario A: Token Not Forwarded

```
User → Web App (authenticated) → API Call (❌ No token)
									  │
									  ▼
						PublicAPI EntraIdAuthenticationMiddleware
									  │
						┌─────────────▼──────────────┐
						│ var token = ExtractBearer()│
						│ if (token == null) {       │
						│   return 401 Unauthorized  │ ❌ ERROR
						│ }                          │
						└────────────────────────────┘

Cause: TokenForwardingDelegatingHandler not registered
Fix: Add .AddHttpMessageHandler<TokenForwardingDelegatingHandler>()
```

### Scenario B: Invalid Token

```
User → Web App (authenticated, but token corrupted)
			  → API Call (Invalid Bearer token)
						  │
						  ▼
			PublicAPI EntraIdAuthenticationMiddleware
						  │
			┌─────────────▼──────────────────────────┐
			│ var principal = ValidateTokenAsync()   │
			│                                        │
			│ // Token signature doesn't match       │
			│ // Bails out:                          │
			│  return 401 Unauthorized  ❌ ERROR   │
			└────────────────────────────────────────┘

Cause: Token tampered with or signed by different key
Fix: Issue fresh token or check Entra ID keys are up-to-date
```

### Scenario C: Token Expired

```
User → Web App (old cached token, expired)
			  → API Call (Bearer: expired_token)
						  │
						  ▼
			PublicAPI EntraIdAuthenticationMiddleware
						  │
			┌─────────────▼──────────────────────────┐
			│ tokenValidationParameters.ValidateLifetime │
			│                                        │
			│ // Token lifetime exceeded             │
			│ // DateTime.UtcNow > token.ExpiresAt   │
			│  return 401 Unauthorized  ❌ ERROR   │
			└────────────────────────────────────────┘

Cause: User has been logged in too long
Fix: User needs to re-authenticate or implement token refresh
```

---

## 6. Configuration Mapping

```
┌─────────────────────────────────────────────────────────────┐
│ appsettings.EntraId.json Structure                         │
└─────────────────────────────────────────────────────────────┘

BOTH Web & PublicAPI:

{
  "Authentication": {
	"EntraId": {
	  ┌─────────────────────────────────────────────────────┐
	  │ TenantId: Shared                                    │
	  │ "12345678-1234-1234-1234-123456789012"             │
	  │ (Azure AD tenant GUID - must be same for both)      │
	  └─────────────────────────────────────────────────────┘

	  ┌─────────────────────────────────────────────────────┐
	  │ ClientId: DIFFERENT for each app!                   │
	  │                                                     │
	  │ Web App ClientId:                                   │
	  │  "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"            │
	  │  (Web app registration in Entra ID)                 │
	  │                                                     │
	  │ PublicAPI ClientId:                                 │
	  │  "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"            │
	  │  (API app registration in Entra ID)                 │
	  └─────────────────────────────────────────────────────┘

	  ┌─────────────────────────────────────────────────────┐
	  │ Authority: Shared (same for both)                   │
	  │ "https://login.microsoftonline.com/                 │
	  │  12345678-1234-1234-1234-123456789012/v2.0"        │
	  │                                                     │
	  │ This is where metadata & signing keys are fetched   │
	  └─────────────────────────────────────────────────────┘

	  ┌─────────────────────────────────────────────────────┐
	  │ Scope: DIFFERENT for each app                       │
	  │                                                     │
	  │ Web App Scope (request multiple scopes):            │
	  │  "openid profile email                              │
	  │   api://bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/…"    │
	  │                                                     │
	  │  openid    → Required for OIDC flow                 │
	  │  profile   → Get user profile claims                │
	  │  email     → Get user email                         │
	  │  api://... → Consent to call PublicAPI             │
	  │                                                     │
	  │ PublicAPI Scope (API expects):                      │
	  │  "api://bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb        │
	  │   /.default"                                        │
	  │                                                     │
	  │  /.default → Use all permissions for this app       │
	  └─────────────────────────────────────────────────────┘

	  TokenValidation:
		ValidateIssuer: true
		ValidateAudience: false (often disabled)
		ValidateLifetime: true
		ValidateSignature: true
		ClockSkewSeconds: 30
	}
  }
}

┌─────────────────────────────────────────────────────────────┐
│ Token Grant Flow                                            │
└─────────────────────────────────────────────────────────────┘

Web App (Interactive Client):
  Scope: "openid profile email api://publicapi/.default"
  ▼ User consents in login flow
  ▼ Entra ID issues:
	 • ID Token (who the user is)
	 • Access Token (permission to call PublicAPI)
	 • Refresh Token (get new tokens without re-login)

PublicAPI:
  Validates: Access Token audience matches its ClientId
  Extracts: User claims (email, groups, etc.)
  Allows: Request processing
```

---

## 7. Fix Summary Comparison

```
ASPECT                  BEFORE FIX          AFTER FIX
─────────────────────────────────────────────────────────────
Token Validation        ❌ BYPASSED         ✅ FULLY ENABLED
						(skipcheck=true)    (skipcheck removed)

Token Forwarding        ❌ NOT IMPLEMENTED  ✅ IMPLEMENTED
											(Handler added)

Authorization Headers   ❌ MISSING          ✅ PRESENT
						(No Bearer token)   (Bearer token added)

User Context            ❌ NULL/Anonymous   ✅ POPULATED
											(Claims available)

Security Level          🔓 OPEN (No auth)   🔒 SECURE (Full)

Inter-Service Auth      ❌ IMPOSSIBLE       ✅ AUTOMATIC

Okta Integration        ⚠️ LEGACY UNUSED    ❌ REMOVED

Error Handling          ❌ NONE              ✅ PROPER 401s
```
