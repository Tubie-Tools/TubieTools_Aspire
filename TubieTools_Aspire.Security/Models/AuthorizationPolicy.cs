namespace TubieTools_Aspire.Security.Models
{
    /// <summary>
    /// Represents an authorization policy for controlling access to operations
    /// </summary>
    public class AuthorizationPolicy
    {
        /// <summary>
        /// Unique policy identifier (e.g., "ServiceNow.Create", "Admin.FullAccess")
        /// </summary>
        public string PolicyId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable policy name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this policy allows
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Roles required to satisfy this policy (user must have ANY of these)
        /// </summary>
        public string[] RequiredRoles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Claims required (claim type => required values). If value is null/empty, any value accepted
        /// </summary>
        public Dictionary<string, string[]> RequiredClaims { get; set; } = new();

        /// <summary>
        /// Scopes required for API access
        /// </summary>
        public string[] RequiredScopes { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Pre-defined authorization policies for the application
    /// </summary>
    public static class AuthorizationPolicies
    {
        /* Admin Policies */
        public const string AdminFullAccess = "AdminFullAccess";

        /* ServiceNow Operation Policies */
        public const string ServiceNowCreate = "ServiceNow.Create";
        public const string ServiceNowRead = "ServiceNow.Read";
        public const string ServiceNowUpdate = "ServiceNow.Update";
        public const string ServiceNowDelete = "ServiceNow.Delete";
        public const string ServiceNowAdmin = "ServiceNow.Admin";

        /* Tenant Management Policies */
        public const string TenantAdmin = "Tenant.Admin";
        public const string TenantRead = "Tenant.Read";
        public const string TenantWrite = "Tenant.Write";

        /// <summary>
        /// Default authorization policies
        /// Note: RoleGroupMapping in EntraIdOptions maps group IDs to these role names
        /// </summary>
        public static readonly Dictionary<string, AuthorizationPolicy> DefaultPolicies = new()
        {
            {
                AdminFullAccess,
                new AuthorizationPolicy
                {
                    PolicyId = AdminFullAccess,
                    Name = "Admin - Full Access",
                    Description = "Full administrative access to all operations",
                    RequiredRoles = new[] { "Admin" },
                    RequiredScopes = new[] { "api://servicenow/.default" }
                }
            },
            {
                ServiceNowCreate,
                new AuthorizationPolicy
                {
                    PolicyId = ServiceNowCreate,
                    Name = "ServiceNow - Create Incidents",
                    Description = "Allows creating incidents in ServiceNow",
                    RequiredRoles = new[] { "Admin", "ServiceNow.Creator", "ServiceNow.Admin" },
                    RequiredScopes = new[] { "api://servicenow/.default" }
                }
            },
            {
                ServiceNowRead,
                new AuthorizationPolicy
                {
                    PolicyId = ServiceNowRead,
                    Name = "ServiceNow - Read Incidents",
                    Description = "Allows reading incidents from ServiceNow",
                    RequiredRoles = new[] { "Admin", "ServiceNow.Creator", "ServiceNow.Reader", "ServiceNow.Admin" },
                    RequiredScopes = new[] { "api://servicenow/.default" }
                }
            },
            {
                ServiceNowUpdate,
                new AuthorizationPolicy
                {
                    PolicyId = ServiceNowUpdate,
                    Name = "ServiceNow - Update Incidents",
                    Description = "Allows updating incidents in ServiceNow",
                    RequiredRoles = new[] { "Admin", "ServiceNow.Admin" },
                    RequiredScopes = new[] { "api://servicenow/.default" }
                }
            },
            {
                ServiceNowDelete,
                new AuthorizationPolicy
                {
                    PolicyId = ServiceNowDelete,
                    Name = "ServiceNow - Delete Incidents",
                    Description = "Allows deleting incidents from ServiceNow",
                    RequiredRoles = new[] { "Admin", "ServiceNow.Admin" },
                    RequiredScopes = new[] { "api://servicenow/.default" }
                }
            },
            {
                ServiceNowAdmin,
                new AuthorizationPolicy
                {
                    PolicyId = ServiceNowAdmin,
                    Name = "ServiceNow - Admin",
                    Description = "Full administrative access to ServiceNow operations and configuration",
                    RequiredRoles = new[] { "Admin", "ServiceNow.Admin" },
                    RequiredScopes = new[] { "api://servicenow/.default" }
                }
            }
        };
    }
}
