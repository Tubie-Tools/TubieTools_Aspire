namespace ModelLayer.Models;

/// <summary>
/// PII handling requirements.
/// </summary>
public class PIIHandlingRequirements
{
    /// <summary>Can process PII</summary>
    public bool CanProcessPII { get; set; }

    /// <summary>Allowed PII types</summary>
    public List<string> AllowedPIITypes { get; set; } = new();

    /// <summary>Masking strategy</summary>
    public string MaskingStrategy { get; set; } // Tokenization, Pseudonymization, Encryption, Redaction

    /// <summary>Encryption for PII</summary>
    public bool EncryptPII { get; set; }

    /// <summary>PII access logging</summary>
    public bool LogPIIAccess { get; set; }

    /// <summary>Consent management required</summary>
    public bool ConsentRequired { get; set; }

    /// <summary>Consent tracking and storage</summary>
    public string ConsentStorage { get; set; }
}
