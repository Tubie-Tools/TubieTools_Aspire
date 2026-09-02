namespace ModelLayer.Models;

/// <summary>
/// Compliance requirement for specific regulation.
/// </summary>
public class ComplianceRequirementPolicy
{
    public string RequirementId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Regulation name</summary>
    public string RegulationName { get; set; } // GDPR, HIPAA, SOC2, NIST, etc.

    /// <summary>Jurisdiction</summary>
    public string Jurisdiction { get; set; }

    /// <summary>Specific articles/requirements</summary>
    public List<string> SpecificRequirements { get; set; } = new();

    /// <summary>Required controls</summary>
    public List<string> RequiredControls { get; set; } = new();

    /// <summary>Compliance certification needed</summary>
    public string CertificationNeeded { get; set; }

    /// <summary>Audit frequency</summary>
    public string AuditFrequency { get; set; } // Annual, Semi-Annual, Quarterly

    /// <summary>Last certification date</summary>
    public DateTime? LastCertificationDate { get; set; }

    /// <summary>Certification expiry date</summary>
    public DateTime? CertificationExpiryDate { get; set; }
}
