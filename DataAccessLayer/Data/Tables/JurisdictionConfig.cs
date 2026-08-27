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
