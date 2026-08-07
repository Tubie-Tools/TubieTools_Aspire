namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a state/jurisdiction configuration
/// </summary>
[Table("Jurisdictions")]
public class JurisdictionConfig
{
    [Key]
    public string JurisdictionId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string JurisdictionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string StateCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "nvarchar(max)")]
    public string RegulationsJson { get; set; } = "{}";

    [Column(TypeName = "nvarchar(max)")]
    public string FeaturesJson { get; set; } = "{}";

    public string DatabaseSchema { get; set; } = string.Empty;

    public string ConnectionStringName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual ICollection<TenantJurisdictionMapping> TenantMappings { get; } = [];
}

/// <summary>
/// Maps tenants to jurisdictions
/// </summary>
[Table("TenantJurisdictionMappings")]
public class TenantJurisdictionMapping
{
    [Key]
    public string MappingId { get; set; } = string.Empty;

    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public string JurisdictionId { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(JurisdictionId))]
    public virtual JurisdictionConfig? Jurisdiction { get; set; }
}

/// <summary>
/// State-specific regulations
/// </summary>
public class StateRegulations
{
    public string StateCode { get; set; } = string.Empty;
    public Dictionary<string, object> DataRetentionPolicies { get; set; } = [];
    public Dictionary<string, object> ComplianceRequirements { get; set; } = [];
    public Dictionary<string, object> SecurityStandards { get; set; } = [];
    public Dictionary<string, bool> RestrictedFeatures { get; set; } = [];
}

/// <summary>
/// State-specific features
/// </summary>
public class StateFeatures
{
    public string StateCode { get; set; } = string.Empty;
    public List<string> EnabledFeatures { get; set; } = [];
    public Dictionary<string, object> FeatureConfiguration { get; set; } = [];
    public int MaxTeamMembers { get; set; } = 50;
    public int MaxApiKeys { get; set; } = 10;
    public bool RequiresMFA { get; set; } = false;
    public bool RequiresDataEncryption { get; set; } = true;
}

/// <summary>
/// Current jurisdiction context
/// </summary>
public class JurisdictionContext
{
    public string JurisdictionId { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string JurisdictionName { get; set; } = string.Empty;
    public StateRegulations? Regulations { get; set; }
    public StateFeatures? Features { get; set; }
    public string DatabaseSchema { get; set; } = string.Empty;
    public string ConnectionStringName { get; set; } = string.Empty;
}