using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for CopilotModelConfiguration entity.
/// </summary>
public class CopilotModelConfigurationFacetMap
{
    public string? ConfigId { get; set; }
    public string? ModelName { get; set; }
    public string? SystemPrompt { get; set; }
    public Dictionary<string, object?> CustomParameters { get; set; } = new();

    public static CopilotModelConfigurationFacetMap FromEntity(CopilotModelConfiguration entity)
    {
        return new CopilotModelConfigurationFacetMap
        {
            ConfigId = entity.ConfigId,
            ModelName = entity.ModelName,
            SystemPrompt = entity.SystemPrompt,
            //CustomParameters = entity.CustomParameters
        };
    }

    public CopilotModelConfiguration ToEntity()
    {
        return new CopilotModelConfiguration
        {
            ConfigId = ConfigId,
            ModelName = ModelName,
            SystemPrompt = SystemPrompt,
            //CustomParameters = CustomParameters
        };
    }
}
