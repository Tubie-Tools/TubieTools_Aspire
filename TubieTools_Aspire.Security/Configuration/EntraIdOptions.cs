namespace TubieTools_Aspire.Security.Configuration
{
    /// <summary>
    /// Configuration options for Microsoft Entra ID (Azure AD) OAuth integration
    /// </summary>
    public class EntraIdOptions
    {
        public const string SectionName = "Authentication:EntraId";

        /// <summary>
        /// Azure AD tenant ID (directory ID)
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Client ID (application ID) registered in Azure AD
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret - should be stored in Key Vault or Secrets Manager in production
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// The URL authority for the token endpoint
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// Scopes required for API access (space-separated)
        /// </summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// Token validation parameters
        /// </summary>
        public TokenValidationOptions TokenValidation { get; set; } = new();

        /// <summary>
        /// Whether to validate SSL certificates (false only for development)
        /// </summary>
        public bool ValidateCertificate { get; set; } = true;

        /// <summary>
        /// Role group mapping: maps Entra ID group IDs to application roles
        /// </summary>
        public Dictionary<string, string[]> RoleGroupMapping { get; set; } = new();
    }

    /// <summary>
    /// Token validation configuration
    /// </summary>
    public class TokenValidationOptions
    {
        /// <summary>
        /// Validate token expiration
        /// </summary>
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Validate token signature
        /// </summary>
        public bool ValidateSignature { get; set; } = true;

        /// <summary>
        /// Validate issuer
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Validate audience
        /// </summary>
        public bool ValidateAudience { get; set; } = false; // Often disabled for multi-app scenarios

        /// <summary>
        /// Expected issuer
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Expected audience
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Clock skew tolerance in seconds
        /// </summary>
        public int ClockSkewSeconds { get; set; } = 30;
    }
}
