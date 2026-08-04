using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using TubieTools_Aspire.Security.Authorization;

namespace TubieTools_Aspire.Security.Authorization
{
    /// <summary>
    /// Authorization filter for policy-based access control
    /// Usage: [AuthorizePolicy("ServiceNow.Create")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizePolicyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _policyId;

        public AuthorizePolicyAttribute(string policyId)
        {
            _policyId = policyId;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Skip if user is not authenticated
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get the authorization service from DI
            var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthorizationService)) as IAuthorizationService;
            if (authService == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            // Authorize against the policy
            var isAuthorized = await authService.AuthorizeAsync(context.HttpContext.User, _policyId);
            if (!isAuthorized)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    /// <summary>
    /// Authorization filter for role-based access control
    /// Usage: [AuthorizeRole("Admin", "ServiceNow.Admin")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Skip if user is not authenticated
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get the authorization service from DI
            var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthorizationService)) as IAuthorizationService;
            if (authService == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            // Check if user has any of the required roles
            var hasRole = authService.HasAnyRole(context.HttpContext.User, _allowedRoles);
            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }

            await Task.CompletedTask;
        }
    }
}
