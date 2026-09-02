namespace ModelLayer.Models;

/// <summary>
/// Data handling and privacy policies.
/// </summary>
public class DataHandlingPolicy
{
    /// <summary>PII/Personal data handling</summary>
    public PIIHandlingRequirements PIIHandling { get; set; }

    /// <summary>Data classification levels allowed</summary>
    public List<string> AllowedDataClassifications { get; set; } = new();

    /// <summary>Sensitive data masking required</summary>
    public bool SensitiveDataMaskingRequired { get; set; }

    /// <summary>Data anonymization requirement</summary>
    public string AnonymizationRequirement { get; set; } // Required, Recommended, Optional

    /// <summary>Retention policies</summary>
    public DataRetentionPolicy RetentionPolicy { get; set; }

    /// <summary>Right to be forgotten implementation</summary>
    public bool SupportsRightToBeForgotten { get; set; }

    /// <summary>Data lineage tracking required</summary>
    public bool DataLineageTrackingRequired { get; set; }

    /// <summary>Third-party data sharing restrictions</summary>
    public List<string> ThirdPartyRestrictions { get; set; } = new();
}
