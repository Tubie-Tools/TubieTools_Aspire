using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for CopilotGovernancePolicy entity.
/// </summary>
public class CopilotGovernancePolicyFacetMap
{
    public string? PolicyId { get; set; }
    public string? PolicyName { get; set; }
    public string? LandingZone { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }

    public static CopilotGovernancePolicyFacetMap FromEntity(CopilotGovernancePolicy entity)
    {
        return new CopilotGovernancePolicyFacetMap
        {
            PolicyId = entity.PolicyId,
            PolicyName = entity.PolicyName,
            LandingZone = entity.LandingZone,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            LastModifiedDate = entity.LastModifiedDate
        };
    }

    public CopilotGovernancePolicy ToEntity()
    {
        return new CopilotGovernancePolicy
        {
            PolicyId = PolicyId,
            PolicyName = PolicyName,
            LandingZone = LandingZone,
            Description = Description,
            IsActive = IsActive,
            CreatedDate = CreatedDate,
            LastModifiedDate = LastModifiedDate
        };
    }
}
