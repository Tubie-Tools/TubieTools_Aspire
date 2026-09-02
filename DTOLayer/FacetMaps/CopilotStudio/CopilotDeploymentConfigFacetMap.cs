using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for CopilotDeploymentConfig entity.
/// </summary>
public class CopilotDeploymentConfigFacetMap
{
    public string? ConfigId { get; set; }
    public string? CopilotId { get; set; }
    public string? Environment { get; set; }
    public string? DeploymentStatus { get; set; }
    public DateTime DeployedDate { get; set; }
    public DateTime? LastUpdated { get; set; }

    public static CopilotDeploymentConfigFacetMap FromEntity(CopilotDeploymentConfig entity)
    {
        return new CopilotDeploymentConfigFacetMap
        {
            ConfigId = entity.ConfigId,
            CopilotId = entity.CopilotId,
            Environment = entity.Environment,
            DeploymentStatus = entity.DeploymentStatus,
            DeployedDate = entity.DeployedDate,
            LastUpdated = entity.LastUpdated
        };
    }

    public CopilotDeploymentConfig ToEntity()
    {
        return new CopilotDeploymentConfig
        {
            ConfigId = ConfigId,
            CopilotId = CopilotId,
            Environment = Environment,
            DeploymentStatus = DeploymentStatus,
            DeployedDate = DeployedDate,
            //LastUpdated = LastUpdated
        };
    }
}
