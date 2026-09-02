using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for CopilotApplication entity.
/// Maps between database entity and DTO representation.
/// </summary>
public class CopilotApplicationFacetMap
{
    public string? CopilotId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LandingZone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }

    /// <summary>
    /// Maps from entity to facet.
    /// </summary>
    public static CopilotApplicationFacetMap FromEntity(CopilotApplication entity)
    {
        return new CopilotApplicationFacetMap
        {
            CopilotId = entity.CopilotId,
            Name = entity.Name,
            Description = entity.Description,
            LandingZone = entity.LandingZone,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            LastModifiedDate = entity.LastModifiedDate
        };
    }

    /// <summary>
    /// Maps from facet to entity.
    /// </summary>
    public CopilotApplication ToEntity()
    {
        return new CopilotApplication
        {
            CopilotId = CopilotId,
            Name = Name,
            Description = Description,
            LandingZone = LandingZone,
            IsActive = IsActive,
            CreatedDate = CreatedDate,
            LastModifiedDate = LastModifiedDate
        };
    }
}
