using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TubieTools_Aspire.Security.Claims;
using TubieTools_Aspire.Security.Configuration;

namespace TubieTools_Aspire.Security.Middleware
{
    /// <summary>
    /// Middleware for validating Entra ID (Azure AD) bearer tokens on incoming requests
    /// Replaces the previous Okta authentication middleware
    /// </summary>
    public class EntraIdAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EntraIdAuthenticationMiddleware> _logger;
        private readonly EntraIdOptions _options;
        private readonly List<string> _excludedPaths;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public EntraIdAuthenticationMiddleware(
            RequestDelegate next,
            ILogger<EntraIdAuthenticationMiddleware> logger,
            IOptions<EntraIdOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _tokenHandler = new JwtSecurityTokenHandler();

            // OpenID Connect configuration manager for retrieving signing keys
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{_options.Authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = _options.ValidateCertificate });

            // Paths that don't require authentication
            _excludedPaths = new List<string>
            {
                "/health",
                "/alive",
                "/swagger",
                "/api/v1/auth/login",
                "/api/v1/auth/callback",
                "/metrics",
                "/.well-known",
                "/connect/authorize",
                "/connect/token"
            };
        }

        public async Task InvokeAsync(
            HttpContext context,
            IEntraIdClaimsTransformer claimsTransformer)
        {
            // Check if path requires authentication
            if (IsPathExcluded(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Extract bearer token from Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            var token = ExtractBearerToken(authHeader);

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Missing or invalid Authorization header on path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid Authorization header" });
                return;
            }

            // Validate token and extract claims
            var principal = await ValidateTokenAsync(token);

            if (principal == null)
            {
                _logger.LogWarning("Token validation failed for path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                return;
            }

            // Transform claims (e.g., groups to roles)
            principal = await claimsTransformer.TransformAsync(principal);

            // Set the authenticated principal on the HttpContext
            context.User = principal;

            _logger.LogInformation("Token validation successful for user {User} on path {Path}",
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown",
                context.Request.Path);

            await _next(context);
        }

        /// <summary>
        /// Extract bearer token from the Authorization header
        /// </summary>
        private static string? ExtractBearerToken(string? authHeader)
        {
            if (string.IsNullOrWhiteSpace(authHeader))
                return null;

            const string bearerScheme = "Bearer ";
            if (!authHeader.StartsWith(bearerScheme, StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader.Substring(bearerScheme.Length).Trim();
        }

        /// <summary>
        /// Validate the Entra ID token
        /// </summary>
        private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                // Get the current signing keys from Entra ID metadata
                var openIdConfig = await _configurationManager.GetConfigurationAsync(default);
                var signingKeys = openIdConfig.SigningKeys;

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _options.TokenValidation.ValidateIssuer,
                    ValidIssuer = _options.TokenValidation.Issuer ?? _options.Authority,
                    ValidateAudience = _options.TokenValidation.ValidateAudience,
                    ValidAudience = _options.TokenValidation.Audience ?? _options.ClientId,
                    ValidateIssuerSigningKey = _options.TokenValidation.ValidateSignature,
                    IssuerSigningKeys = signingKeys,
                    ValidateLifetime = _options.TokenValidation.ValidateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(_options.TokenValidation.ClockSkewSeconds)
                };

                var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                _logger.LogDebug("Token validated successfully for audience: {Audience}",
                    principal.FindFirst("aud")?.Value ?? "unknown");

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Check if a path requires authentication
        /// </summary>
        private bool IsPathExcluded(PathString path)
        {
            return _excludedPaths.Any(excludedPath =>
                path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
