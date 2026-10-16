using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.Web.Services
{
    /// <summary>
    /// HttpClientHandler that automatically forwards the current user's access token
    /// to downstream API calls. This enables service-to-service authentication using
    /// the user's EntraID token context.
    /// </summary>
    public class TokenForwardingDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TokenForwardingDelegatingHandler> _logger;

        public TokenForwardingDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<TokenForwardingDelegatingHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Get the current HTTP context
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                bool tokenForwarded = false;

                // Try to get the access token from the authentication scheme (OIDC flow)
                var accessToken = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Add the token as a bearer token to the outgoing request
                    request.Headers.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    tokenForwarded = true;
                    _logger.LogDebug(
                        "✓ Forwarded access_token from OIDC to {Method} {RequestUri}",
                        request.Method, request.RequestUri);
                }
                else
                {
                    // Try to get it from the JWT bearer token in the Authorization header
                    var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && 
                        authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeader.Substring("Bearer ".Length).Trim();
                        request.Headers.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        tokenForwarded = true;
                        _logger.LogDebug(
                            "✓ Forwarded Bearer token from header to {Method} {RequestUri}",
                            request.Method, request.RequestUri);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "✗ No token found in HttpContext for {Method} {RequestUri}. " +
                            "User: {User}, HasAuthHeader: {HasAuth}",
                            request.Method, request.RequestUri,
                            httpContext.User?.Identity?.Name ?? "Anonymous",
                            !string.IsNullOrEmpty(authHeader));
                    }
                }

                if (!tokenForwarded)
                {
                    _logger.LogWarning(
                        "⚠ No token forwarded to {Method} {RequestUri}. " +
                        "This may cause 401 Unauthorized if the endpoint requires authentication.",
                        request.Method, request.RequestUri);
                }
            }
            else
            {
                _logger.LogWarning(
                    "⚠ HttpContext is null for {Method} {RequestUri}. " +
                    "This may happen in background tasks or non-HTTP requests.",
                    request.Method, request.RequestUri);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
