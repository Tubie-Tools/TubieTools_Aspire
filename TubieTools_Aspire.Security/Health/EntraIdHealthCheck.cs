using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TubieTools_Aspire.Security.Configuration;

namespace TubieTools_Aspire.Security.Health
{
    /// <summary>
    /// Health check for Entra ID (Azure AD) token endpoint connectivity and configuration
    /// </summary>
    public class EntraIdHealthCheck : IHealthCheck
    {
        private readonly EntraIdOptions _options;
        private readonly ILogger<EntraIdHealthCheck> _logger;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public EntraIdHealthCheck(
            IOptions<EntraIdOptions> options,
            ILogger<EntraIdHealthCheck> logger)
        {
            _options = options.Value;
            _logger = logger;

            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{_options.Authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = _options.ValidateCertificate });
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate configuration
                if (string.IsNullOrEmpty(_options.TenantId) || string.IsNullOrEmpty(_options.ClientId))
                {
                    _logger.LogWarning("Entra ID configuration is incomplete");
                    return HealthCheckResult.Unhealthy("Entra ID configuration is missing or incomplete");
                }

                // Try to retrieve OpenID Connect metadata
                var config = await _configurationManager.GetConfigurationAsync(cancellationToken);

                if (config?.SigningKeys == null || !config.SigningKeys.Any())
                {
                    _logger.LogWarning("Entra ID signing keys are not available");
                    return HealthCheckResult.Unhealthy("Unable to retrieve signing keys from Entra ID");
                }

                _logger.LogInformation("Entra ID health check passed. Signing keys available: {Count}", config.SigningKeys.Count);

                var data = new Dictionary<string, object>
                {
                    { "TenantId", _options.TenantId },
                    { "Authority", _options.Authority },
                    { "SigningKeysCount", config.SigningKeys.Count }
                };

                return HealthCheckResult.Healthy("Entra ID is accessible and configured correctly", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Entra ID health check failed: {Message}", ex.Message);
                return HealthCheckResult.Unhealthy($"Entra ID health check failed: {ex.Message}");
            }
        }
    }
}
