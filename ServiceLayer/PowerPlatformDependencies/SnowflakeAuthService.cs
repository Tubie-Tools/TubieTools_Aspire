using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.PowerPlatformDependencies
{

    public class SnowflakeAuthService : ISnowflakeAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SnowflakeAuthService> _logger;
        private readonly DefaultAzureCredential _credential;
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public SnowflakeAuthService(IConfiguration configuration, ILogger<SnowflakeAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _credential = new DefaultAzureCredential();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            try
            {
                var oAuthAudience = _configuration["Snowflake:OAuthAudience"]
                    ?? throw new InvalidOperationException("Snowflake:OAuthAudience not configured");

                var token = await _credential.GetTokenAsync(
                    new Azure.Core.TokenRequestContext(new[] { $"{oAuthAudience}/.default" }));

                _cachedToken = token.Token;
                _tokenExpiry = token.ExpiresOn.UtcDateTime.AddSeconds(-60);

                _logger.LogInformation("Snowflake OAuth token acquired successfully");
                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire Snowflake OAuth token");
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                var token = await GetAccessTokenAsync();
                return !string.IsNullOrEmpty(token) && DateTime.UtcNow < _tokenExpiry;
            }
            catch
            {
                return false;
            }
        }
    }

}
