using System.Security.Claims;
using TubieTools_Aspire.Security.Models;

namespace TubieTools_Aspire.Security.Authorization
{
    /// <summary>
    /// Service for evaluating authorization policies
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Check if a user satisfies a specific authorization policy
        /// </summary>
        Task<bool> AuthorizeAsync(ClaimsPrincipal user, string policyId);

        /// <summary>
        /// Check if a user has a specific role
        /// </summary>
        bool HasRole(ClaimsPrincipal user, string role);

        /// <summary>
        /// Check if a user has any of the specified roles
        /// </summary>
        bool HasAnyRole(ClaimsPrincipal user, params string[] roles);

        /// <summary>
        /// Check if a user has all of the specified roles
        /// </summary>
        bool HasAllRoles(ClaimsPrincipal user, params string[] roles);

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        IEnumerable<string> GetUserRoles(ClaimsPrincipal user);

        /// <summary>
        /// Get the user's object ID (from 'oid' claim)
        /// </summary>
        string? GetUserObjectId(ClaimsPrincipal user);

        /// <summary>
        /// Get the user's UPN (user principal name)
        /// </summary>
        string? GetUserPrincipalName(ClaimsPrincipal user);

        /// <summary>
        /// Get the user's email
        /// </summary>
        string? GetUserEmail(ClaimsPrincipal user);

        /// <summary>
        /// Register a custom authorization policy
        /// </summary>
        void RegisterPolicy(AuthorizationPolicy policy);

        /// <summary>
        /// Get a registered policy by ID
        /// </summary>
        AuthorizationPolicy? GetPolicy(string policyId);
    }
}
