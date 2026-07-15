using TubieTools_PublicAPI.Models;

namespace TubieTools_PublicAPI.Services
{
    /// <summary>
    /// Interface for Okta token introspection service
    /// </summary>
    public interface IOktaTokenIntrospectionService
    {
        /// <summary>
        /// Introspects an Okta access token to validate it and retrieve claims
        /// </summary>
        /// <param name="accessToken">The access token to introspect</param>
        /// <returns>Token introspection response with validity and claims</returns>
        Task<TokenIntrospectionResponse> IntrospectTokenAsync(string accessToken);

        /// <summary>
        /// Validates that a token is active and has required scopes
        /// </summary>
        /// <param name="accessToken">The access token to validate</param>
        /// <param name="requiredScopes">Optional scopes that must be present</param>
        /// <returns>True if token is valid and has required scopes, false otherwise</returns>
        Task<bool> ValidateTokenAsync(string accessToken, params string[] requiredScopes);

        /// <summary>
        /// Extracts the bearer token from an authorization header
        /// </summary>
        /// <param name="authorizationHeader">The Authorization header value</param>
        /// <returns>The bearer token, or null if not found</returns>
        string? ExtractBearerToken(string? authorizationHeader);
    }
}
