using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for KnowledgeTool entity.
/// </summary>
public class KnowledgeToolFacetMap
{
    public string? ToolId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public bool IsActive { get; set; }

    public static KnowledgeToolFacetMap FromEntity(KnowledgeTool entity)
    {
        return new KnowledgeToolFacetMap
        {
            ToolId = entity.ToolId,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.Type,
            IsActive = entity.IsActive
        };
    }

    public KnowledgeTool ToEntity()
    {
        return new KnowledgeTool
        {
            ToolId = ToolId,
            Name = Name,
            Description = Description,
            Type = Type,
            IsActive = IsActive
        };
    }
}
