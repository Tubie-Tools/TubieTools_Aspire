namespace ModelLayer.Models;

/// <summary>
/// Security requirements for the copilot.
/// </summary>
public class SecurityRequirements
{
    /// <summary>Encryption in transit (TLS version minimum)</summary>
    public string EncryptionInTransit { get; set; } = "TLS 1.2";

    /// <summary>Encryption at rest required</summary>
    public bool EncryptionAtRestRequired { get; set; }

    /// <summary>Encryption key management requirements</summary>
    public string KeyManagementService { get; set; } // "Managed", "BYOK", "BYOZK"

    /// <summary>Authentication requirements for users</summary>
    public string UserAuthenticationMethod { get; set; } // MFA, SAML, AD, OAuth

    /// <summary>Multi-factor authentication required</summary>
    public bool MFARequired { get; set; }

    /// <summary>Session timeout minutes</summary>
    public int? SessionTimeoutMinutes { get; set; }

    /// <summary>IP allowlist/blocklist required</summary>
    public bool IPRestrictionRequired { get; set; }

    /// <summary>Allowed IP ranges</summary>
    public List<string> AllowedIPRanges { get; set; } = new();

    /// <summary>VPC/Network requirements</summary>
    public string NetworkIsolationRequirement { get; set; } // "Public", "Private", "VPCOnly"

    /// <summary>Secret management solution required</summary>
    public string SecretManagement { get; set; } // "KeyVault", "SecretsManager", "Custom"

    /// <summary>Vulnerability scanning required</summary>
    public bool VulnerabilityScanningRequired { get; set; }

    /// <summary>Scanning frequency</summary>
    public string ScanningFrequency { get; set; } // "Continuous", "Daily", "Weekly"

    /// <summary>Penetration testing required</summary>
    public bool PenetrationTestingRequired { get; set; }

    /// <summary>Third-party security assessment</summary>
    public bool ThirdPartyAssessmentRequired { get; set; }
}
