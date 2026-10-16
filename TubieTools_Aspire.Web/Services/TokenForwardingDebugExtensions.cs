using System.Net.Http.Headers;
using TubieTools_Aspire.Security.Configuration;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.Web.Services
{
    /// <summary>
    /// Debugging extension for TokenForwardingDelegatingHandler
    /// Enables detailed logging of token forwarding operations
    /// </summary>
    public static class TokenForwardingDebugExtensions
    {
        /// <summary>
        /// Log detailed information about token forwarding for debugging
        /// </summary>
        public static void LogTokenForwardingDebug(
            this ILogger logger,
            string httpMethod,
            string requestUri,
            bool tokenFound,
            string tokenSummary,
            string context)
        {
            if (tokenFound)
            {
                logger.LogInformation(
                    "✓ Token forwarded: {Method} {Uri} | Token: {TokenSummary} | Context: {Context}",
                    httpMethod, requestUri, tokenSummary, context);
            }
            else
            {
                logger.LogWarning(
                    "✗ No token forwarded: {Method} {Uri} | Context: {Context}",
                    httpMethod, requestUri, context);
            }
        }

        /// <summary>
        /// Extract token summary for logging (first 20 chars + "...")
        /// </summary>
        public static string GetTokenSummary(string token)
        {
            if (string.IsNullOrEmpty(token)) return "null";
            if (token.Length <= 20) return token;
            return token.Substring(0, 20) + "...";
        }
    }
}
