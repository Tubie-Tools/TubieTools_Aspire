using TubieTools_PublicAPI.Services;

namespace TubieTools_PublicAPI.Middleware
{
    /// <summary>
    /// Middleware for validating Okta bearer tokens on incoming requests
    /// </summary>
    public class OktaAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OktaAuthenticationMiddleware> _logger;
        private readonly List<string> _excludedPaths;

        public OktaAuthenticationMiddleware(RequestDelegate next, ILogger<OktaAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            // Paths that don't require authentication
            _excludedPaths = new List<string>
            {
                "/health",
                "/swagger",
                "/api/v1/auth/token",
                "/metrics"
            };
        }

        public async Task InvokeAsync(HttpContext context, IOktaTokenIntrospectionService tokenService)
        {
            // Check if path requires authentication
            if (IsPathExcluded(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Extract bearer token from Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            var token = tokenService.ExtractBearerToken(authHeader);

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Missing or invalid Authorization header on path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid Authorization header" });
                return;
            }

            // Validate token
            var isValid = await tokenService.ValidateTokenAsync(token);

            if (!isValid)
            {
                _logger.LogWarning("Token validation failed for path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                return;
            }

            _logger.LogInformation("Token validation successful for path: {Path}", context.Request.Path);
            await _next(context);
        }

        private bool IsPathExcluded(PathString path)
        {
            return _excludedPaths.Any(excludedPath => 
                path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
