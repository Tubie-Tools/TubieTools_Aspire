using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TubieTools_Aspire.Security.Configuration;

namespace TubieTools_Aspire.Security.Claims
{
    /// <summary>
    /// Transforms Entra ID claims to application roles based on group membership
    /// </summary>
    public interface IEntraIdClaimsTransformer
    {
        /// <summary>
        /// Transform Entra ID groups to application roles
        /// </summary>
        Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal);
    }

    /// <summary>
    /// Default implementation of Entra ID claims transformer
    /// Maps Entra ID group IDs (from groups claim) to application roles
    /// </summary>
    public class EntraIdClaimsTransformer : IEntraIdClaimsTransformer
    {
        private readonly EntraIdOptions _options;
        private readonly ILogger<EntraIdClaimsTransformer> _logger;

        public EntraIdClaimsTransformer(
            IOptions<EntraIdOptions> options,
            ILogger<EntraIdClaimsTransformer> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Transform the principal by extracting groups and mapping to roles
        /// </summary>
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity is not ClaimsIdentity identity)
            {
                _logger.LogWarning("Principal or identity is null, returning original principal");
                return Task.FromResult(principal ?? new ClaimsPrincipal());
            }

            // Extract group IDs from the 'groups' claim (array of group object IDs)
            var groupClaims = identity.FindAll("groups");
            if (!groupClaims.Any())
            {
                _logger.LogDebug("No group claims found for user");
                return Task.FromResult(principal);
            }

            var groupIds = groupClaims.Select(c => c.Value).ToList();
            _logger.LogInformation("User is member of {GroupCount} groups: {Groups}", 
                groupIds.Count, string.Join(", ", groupIds));

            // Map groups to application roles
            var applicableRoles = new HashSet<string>();
            foreach (var groupId in groupIds)
            {
                if (_options.RoleGroupMapping.TryGetValue(groupId, out var roles))
                {
                    foreach (var role in roles)
                    {
                        applicableRoles.Add(role);
                        _logger.LogDebug("Mapped group {GroupId} to role {Role}", groupId, role);
                    }
                }
            }

            // Add role claims to identity
            foreach (var role in applicableRoles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            if (applicableRoles.Count > 0)
            {
                _logger.LogInformation("User assigned {RoleCount} application roles: {Roles}",
                    applicableRoles.Count, string.Join(", ", applicableRoles));
            }
            else
            {
                _logger.LogWarning("No application roles mapped for groups: {Groups}",
                    string.Join(", ", groupIds));
            }

            return Task.FromResult(principal);
        }
    }
}
