using Microsoft.AspNetCore.Builder;
using TubieTools_Aspire.Security.Middleware;

namespace TubieTools_Aspire.Security.Extensions
{
    /// <summary>
    /// Extension methods for registering security middleware and services
    /// </summary>
    public static class SecurityMiddlewareExtensions
    {
        /// <summary>
        /// Add Entra ID authentication middleware to the request pipeline
        /// Must be called after app.UseRouting() and before endpoint mapping
        /// </summary>
        public static WebApplication UseEntraIdAuthentication(this WebApplication app)
        {
            app.UseMiddleware<EntraIdAuthenticationMiddleware>();
            return app;
        }
    }
}
