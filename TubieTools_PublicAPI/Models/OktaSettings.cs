namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Configuration settings for Okta integration
    /// </summary>
    public class OktaSettings
    {
        /// <summary>
        /// Okta organization domain (e.g., https://dev-123456.okta.com)
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Okta API token for server-to-server communication
        /// </summary>
        public string ApiToken { get; set; } = string.Empty;

        /// <summary>
        /// OAuth 2.0 Authorization Server ID (typically "default" or a custom ID)
        /// </summary>
        public string AuthorizationServerId { get; set; } = "default";

        /// <summary>
        /// Application (client) ID for token introspection
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Application client secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Token introspection endpoint path (relative to domain)
        /// </summary>
        public string IntrospectionEndpoint { get; set; } = "/oauth2/{authorizationServerId}/v1/introspect";

        /// <summary>
        /// Whether to enable token caching (default: true)
        /// </summary>
        public bool EnableTokenCaching { get; set; } = true;

        /// <summary>
        /// Token cache duration in minutes (default: 10)
        /// </summary>
        public int TokenCacheDurationMinutes { get; set; } = 10;

        /// <summary>
        /// List of scopes required for API access
        /// </summary>
        public List<string> RequiredScopes { get; set; } = new();

        /// <summary>
        /// Gets the full introspection endpoint URL
        /// </summary>
        public string GetIntrospectionUrl()
        {
            if (string.IsNullOrEmpty(Domain) || string.IsNullOrEmpty(AuthorizationServerId))
            {
                return string.Empty;
            }

            var endpoint = IntrospectionEndpoint.Replace("{authorizationServerId}", AuthorizationServerId);
            return $"{Domain}{endpoint}";
        }
    }
}
