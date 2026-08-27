namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Tool access control configuration.
/// </summary>
public class ToolAccessControl
{
    /// <summary>Access control enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Roles allowed to use this tool</summary>
    public List<string> AllowedRoles { get; set; } = new();

    /// <summary>Users allowed to use this tool</summary>
    public List<string> AllowedUsers { get; set; } = new();

    /// <summary>User groups allowed</summary>
    public List<string> AllowedGroups { get; set; } = new();

    /// <summary>Row-level security filtering enabled</summary>
    public bool EnableRowLevelSecurity { get; set; }

    /// <summary>Data classification levels allowed to be queried</summary>
    public List<string> AllowedDataClassifications { get; set; } = new();

    /// <summary>Audit all tool usage</summary>
    public bool AuditAllUsage { get; set; } = true;

    /// <summary>Rate limit (queries per minute)</summary>
    public int RateLimitPerMinute { get; set; } = 100;
}
