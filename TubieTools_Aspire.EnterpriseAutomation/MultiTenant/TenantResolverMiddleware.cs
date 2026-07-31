using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant
{
    /// <summary>
    /// Middleware for resolving tenant context from requests
    /// </summary>
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolverMiddleware> _logger;

        public TenantResolverMiddleware(RequestDelegate next, ILogger<TenantResolverMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, 
            ITenantContextAccessor tenantContextAccessor, 
            ITenantService tenantService,
            ISubscriptionManager subscriptionManager)
        {
            try
            {
                // Extract tenant info from headers or claims
                var tenantId = ExtractTenantId(context);

                if (string.IsNullOrEmpty(tenantId))
                {
                    // when in development, we have no tenantId until we sign in, forego the error for now, seems not to be using the json file currently

                    //context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    //await context.Response.WriteAsJsonAsync(new { error = "Tenant ID not provided" });
                    //return;
                    tenantId = "acme-corp-001"; // For development purposes
                }

                _logger.LogInformation("Resolving tenant context for: {TenantId}", tenantId);

                // Get tenant configuration
                var tenantConfig = await tenantService.GetTenantAsync(tenantId);
                if (tenantConfig == null || !tenantConfig.IsActive)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { error = "Tenant not found or inactive" });
                    return;
                }

                // Get subscription info
                var subscription = await tenantService.GetSubscriptionAsync(tenantId);
                var quota = await tenantService.GetQuotaAsync(tenantId);

                // Get tier configuration
                var tierConfig = await subscriptionManager.GetTierConfigAsync(tenantConfig.CurrentTier);

                // Create tenant context
                var tenantContext = new TenantContext
                {
                    TenantId = tenantId,
                    TenantName = tenantConfig.TenantName,
                    CurrentTier = tenantConfig.CurrentTier,
                    Subscription = subscription,
                    Quota = quota,
                    AvailableTools = tierConfig?.AvailableTools ?? new List<string>(),
                    AvailableModels = tierConfig?.AvailableModels ?? new List<string>(),
                    IsActive = tenantConfig.IsActive,
                    Features = BuildFeatureFlags(tierConfig)
                };

                // Set the context
                tenantContextAccessor.TenantContext = tenantContext;

                _logger.LogInformation("Tenant context resolved: {TenantId}, Tier: {Tier}", tenantId, tenantConfig.CurrentTier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant context");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
                return;
            }

            await _next(context);
        }

        private string ExtractTenantId(HttpContext context)
        {
            // Try to get from header first
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
                return tenantHeader.ToString();

            // Try to get from claims
            var tenantClaim = context.User?.FindFirst("tenant_id");
            if (tenantClaim != null)
                return tenantClaim.Value;

            // Try to get from API key
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var bearerToken = authHeader.ToString().Replace("Bearer ", "");
                // In real implementation, would validate API key and extract tenant
                return ExtractTenantFromApiKey(bearerToken);
            }

            return null;
        }

        private string ExtractTenantFromApiKey(string apiKey)
        {
            // This would normally verify the API key and extract tenant ID from database
            // For now, just return null
            return null;
        }

        private Dictionary<string, bool> BuildFeatureFlags(SubscriptionTierConfig tierConfig)
        {
            if (tierConfig == null)
                return new Dictionary<string, bool>();

            return new Dictionary<string, bool>
            {
                { "custom_prompts", tierConfig.AllowCustomPrompts },
                { "multiple_agents", tierConfig.AllowMultipleAgents },
                { "workflow_orchestration", tierConfig.AllowWorkflowOrchestration },
                { "analytics", tierConfig.AllowAnalytics },
                { "api_access", tierConfig.AllowAPIAccess },
                { "webhooks", tierConfig.AllowWebhooks },
                { "priority_support", tierConfig.PrioritySupport },
                { "data_export", tierConfig.AllowDataExport },
                { "public_api", tierConfig.PublicAPI }
            };
        }
    }

    /// <summary>
    /// Extension methods for adding the tenant resolver middleware
    /// </summary>
    public static class TenantResolverMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolver(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolverMiddleware>();
        }
    }
}
