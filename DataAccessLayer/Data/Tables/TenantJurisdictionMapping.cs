namespace TubieTools_Aspire.EnterpriseAutomation.MultiTenant.Jurisdiction;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
