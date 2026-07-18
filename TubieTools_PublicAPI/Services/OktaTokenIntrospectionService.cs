using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using TubieTools_PublicAPI.Models;

namespace TubieTools_PublicAPI.Services
{
    /// <summary>
    /// Service for introspecting and validating Okta access tokens
    /// </summary>
    public class OktaTokenIntrospectionService : IOktaTokenIntrospectionService
    {
        private readonly OktaSettings _oktaSettings;
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly ILogger<OktaTokenIntrospectionService> _logger;

        public OktaTokenIntrospectionService(
            IOptions<OktaSettings> oktaSettings,
           IDistributedCache cache,
            ILogger<OktaTokenIntrospectionService> logger,
            HttpClient httpClient = null)
        {
            _oktaSettings = oktaSettings.Value ?? throw new ArgumentNullException(nameof(oktaSettings));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? new HttpClient();

            ValidateSettings();
        }

        /// <summary>
        /// Introspects an Okta access token to validate it and retrieve claims
        /// </summary>
        public async Task<TokenIntrospectionResponse> IntrospectTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("Attempted to introspect null or empty token");
                return new TokenIntrospectionResponse { Active = false };
            }

            // Check cache first if enabled
            if (_oktaSettings.EnableTokenCaching)
            {
                var cachedResponse = await GetCachedTokenAsync(accessToken);
                if (cachedResponse != null)
                {
                    _logger.LogDebug("Token found in cache");
                    return cachedResponse;
                }
            }

            try
            {
                var introspectionUrl = _oktaSettings.GetIntrospectionUrl();
                var request = new HttpRequestMessage(HttpMethod.Post, introspectionUrl);

                // Add Basic Auth header with client credentials
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_oktaSettings.ClientId}:{_oktaSettings.ClientSecret}"));
                request.Headers.Add("Authorization", $"Basic {credentials}");

                // Add token and client_id as form data
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "token", accessToken },
                    { "client_id", _oktaSettings.ClientId }
                });
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var introspectionResponse = JsonSerializer.Deserialize<TokenIntrospectionResponse>(jsonContent);

                if (introspectionResponse == null)
                {
                    _logger.LogError("Failed to deserialize token introspection response");
                    return new TokenIntrospectionResponse { Active = false };
                }

                // Cache the response if enabled and token is active
                if (_oktaSettings.EnableTokenCaching && introspectionResponse.Active)
                {
                    await CacheTokenResponseAsync(accessToken, introspectionResponse);
                }

                _logger.LogInformation("Token introspection successful. Token active: {Active}", introspectionResponse.Active);
                return introspectionResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during token introspection");
                return new TokenIntrospectionResponse { Active = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token introspection");
                return new TokenIntrospectionResponse { Active = false };
            }
        }

        /// <summary>
        /// Validates that a token is active and has required scopes
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string accessToken, params string[] requiredScopes)
        {
            var introspectionResponse = await IntrospectTokenAsync(accessToken);

            if (!introspectionResponse.IsValid())
            {
                _logger.LogWarning("Token is not valid");
                return false;
            }

            if (requiredScopes.Length > 0 && !introspectionResponse.HasRequiredScopes(requiredScopes))
            {
                _logger.LogWarning("Token does not have required scopes. Required: {RequiredScopes}, Actual: {ActualScopes}",
                    string.Join(", ", requiredScopes), introspectionResponse.Scope);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts the bearer token from an authorization header
        /// </summary>
        public string? ExtractBearerToken(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return null;
            }

            const string bearerScheme = "Bearer ";
            if (authorizationHeader.StartsWith(bearerScheme, StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring(bearerScheme.Length).Trim();
            }

            return null;
        }

        private async Task<TokenIntrospectionResponse?> GetCachedTokenAsync(string accessToken)
        {
            try
            {
                var cacheKey = $"okta_token_{GetTokenHash(accessToken)}";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (cachedData != null)
                {
                    return JsonSerializer.Deserialize<TokenIntrospectionResponse>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving token from cache");
            }

            return null;
        }

        private async Task CacheTokenResponseAsync(string accessToken, TokenIntrospectionResponse response)
        {
            try
            {
                var cacheKey = $"okta_token_{GetTokenHash(accessToken)}";
                var serialized = JsonSerializer.Serialize(response);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_oktaSettings.TokenCacheDurationMinutes)
                };

                await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching token response");
            }
        }

        private static string GetTokenHash(string token)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashedBytes).Replace("/", "_").Replace("+", "-");
            }
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_oktaSettings.Domain))
                throw new InvalidOperationException("Okta Domain is not configured");

            if (string.IsNullOrWhiteSpace(_oktaSettings.ClientId))
                throw new InvalidOperationException("Okta ClientId is not configured");

            if (string.IsNullOrWhiteSpace(_oktaSettings.ClientSecret))
                throw new InvalidOperationException("Okta ClientSecret is not configured");

            if (string.IsNullOrWhiteSpace(_oktaSettings.ApiToken))
                throw new InvalidOperationException("Okta ApiToken is not configured");
        }
    }
}
