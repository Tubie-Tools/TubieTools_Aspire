using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TubieTools_Aspire.Security.Configuration;
using TubieTools_Aspire.Security.Models;

namespace TubieTools_Aspire.Security.Authorization
{
    /// <summary>
    /// Default implementation of authorization service
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly EntraIdOptions _options;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly Dictionary<string, AuthorizationPolicy> _customPolicies;

        // Standard Entra ID claim types
        private const string ObjectIdClaimType = "oid";
        private const string UserPrincipalNameClaimType = "upn";
        private const string EmailClaimType = "email";
        private const string GivenNameClaimType = "given_name";
        private const string FamilyNameClaimType = "family_name";

        public AuthorizationService(
            IOptions<EntraIdOptions> options,
            ILogger<AuthorizationService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _customPolicies = new Dictionary<string, AuthorizationPolicy>();

            // Register default policies
            foreach (var policy in AuthorizationPolicies.DefaultPolicies.Values)
            {
                _customPolicies[policy.PolicyId] = policy;
            }
        }

        public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string policyId)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("User is not authenticated for policy {PolicyId}", policyId);
                return false;
            }

            if (!_customPolicies.TryGetValue(policyId, out var policy))
            {
                _logger.LogError("Policy {PolicyId} not found", policyId);
                return false;
            }

            // Check required roles
            if (policy.RequiredRoles.Any())
            {
                if (!HasAnyRole(user, policy.RequiredRoles))
                {
                    var userRoles = string.Join(", ", GetUserRoles(user));
                    _logger.LogWarning("User {User} denied access to policy {PolicyId}. Has roles: {UserRoles}, required: {RequiredRoles}",
                        GetUserPrincipalName(user) ?? "unknown",
                        policyId,
                        userRoles,
                        string.Join(", ", policy.RequiredRoles));
                    return false;
                }
            }

            // Check required claims
            if (policy.RequiredClaims.Any())
            {
                foreach (var (claimType, requiredValues) in policy.RequiredClaims)
                {
                    var userClaim = user.FindFirst(claimType)?.Value;
                    if (requiredValues.Length > 0 && !requiredValues.Contains(userClaim))
                    {
                        _logger.LogWarning("User {User} missing required claim {ClaimType} with values {RequiredValues}",
                            GetUserPrincipalName(user) ?? "unknown",
                            claimType,
                            string.Join(", ", requiredValues));
                        return false;
                    }
                }
            }

            _logger.LogInformation("User {User} authorized for policy {PolicyId}",
                GetUserPrincipalName(user) ?? "unknown",
                policyId);
            return await Task.FromResult(true);
        }

        public bool HasRole(ClaimsPrincipal user, string role)
        {
            return user?.FindFirst(ClaimTypes.Role)?.Value == role;
        }

        public bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
        {
            if (roles.Length == 0)
                return true;

            var userRoles = user?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
            return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
        }

        public bool HasAllRoles(ClaimsPrincipal user, params string[] roles)
        {
            if (roles.Length == 0)
                return true;

            var userRoles = user?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return roles.All(role => userRoles.Contains(role));
        }

        public IEnumerable<string> GetUserRoles(ClaimsPrincipal user)
        {
            return user?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        public string? GetUserObjectId(ClaimsPrincipal user)
        {
            return user?.FindFirst(ObjectIdClaimType)?.Value;
        }

        public string? GetUserPrincipalName(ClaimsPrincipal user)
        {
            return user?.FindFirst(UserPrincipalNameClaimType)?.Value;
        }

        public string? GetUserEmail(ClaimsPrincipal user)
        {
            return user?.FindFirst(EmailClaimType)?.Value;
        }

        public void RegisterPolicy(AuthorizationPolicy policy)
        {
            _customPolicies[policy.PolicyId] = policy;
            _logger.LogInformation("Registered authorization policy: {PolicyId}", policy.PolicyId);
        }

        public AuthorizationPolicy? GetPolicy(string policyId)
        {
            return _customPolicies.TryGetValue(policyId, out var policy) ? policy : null;
        }
    }
}
