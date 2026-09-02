using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for CopilotVersion entity.
/// </summary>
public class CopilotVersionFacetMap
{
    public string? VersionId { get; set; }
    public string? CopilotId { get; set; }
    public string? VersionNumber { get; set; }
    public string? Description { get; set; }
    public DateTime ReleaseDate { get; set; }
    public List<string> Changes { get; set; } = new();
    public List<string> BreakingChanges { get; set; } = new();
    public List<string> Deprecations { get; set; } = new();

    public static CopilotVersionFacetMap FromEntity(CopilotVersion entity)
    {
        return new CopilotVersionFacetMap
        {
            VersionId = entity.VersionId,
            CopilotId = entity.CopilotId,
            VersionNumber = entity.VersionNumber,
            Description = entity.Description,
            ReleaseDate = entity.ReleaseDate,
            Changes = entity.Changes,
            BreakingChanges = entity.BreakingChanges,
            Deprecations = entity.Deprecations
        };
    }

    public CopilotVersion ToEntity()
    {
        return new CopilotVersion
        {
            VersionId = VersionId,
            CopilotId = CopilotId,
            VersionNumber = VersionNumber,
            Description = Description,
            ReleaseDate = ReleaseDate,
            Changes = Changes,
            BreakingChanges = BreakingChanges,
            Deprecations = Deprecations
        };
    }
}
