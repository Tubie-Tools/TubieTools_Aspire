namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Identity and Access Management configuration.
/// </summary>
public class IAMConfiguration
{
    /// <summary>Identity provider (Azure AD, Okta, etc.)</summary>
    public string IdentityProvider { get; set; }

    /// <summary>RBAC enabled</summary>
    public bool RBACEnabled { get; set; }

    /// <summary>Predefined roles</summary>
    public List<RoleDefinition> Roles { get; set; } = new();

    /// <summary>Service principal required</summary>
    public bool ServicePrincipalRequired { get; set; }

    /// <summary>Managed identity preferred</summary>
    public bool ManagedIdentityPreferred { get; set; }

    /// <summary>MFA enforcement</summary>
    public bool MFAEnforced { get; set; }

    /// <summary>Conditional access policies</summary>
    public List<string> ConditionalAccessPolicies { get; set; } = new();

    /// <summary>Privileged access management</summary>
    public bool PAMEnabled { get; set; }

    /// <summary>Session recording for sensitive roles</summary>
    public bool SessionRecordingEnabled { get; set; }
}
