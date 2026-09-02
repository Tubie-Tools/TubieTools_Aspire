using DataAccessLayer.Data.Entities;

namespace DTOLayer.FacetMaps.MapApp;

/// <summary>
/// Facet mapping for MapRoute entity.
/// </summary>
public class MapRouteFacetMap
{
    public int RouteId { get; set; }
    public string? RouteName { get; set; }
    public string? Description { get; set; }
    public decimal Distance { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsActive { get; set; }

    public static MapRouteFacetMap FromEntity(MapRoute entity)
    {
        return new MapRouteFacetMap
        {
            RouteId = entity.Id,
            RouteName = entity.RouteName,
            Description = entity.Description,
            Distance = entity.Distance,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedDate = entity.CreatedDate,
            LastModifiedDate = entity.LastModifiedDate,
            IsActive = entity.IsActive
        };
    }

    public MapRoute ToEntity()
    {
        return new MapRoute
        {
            Id = RouteId,
            RouteName = RouteName,
            Description = Description,
            Distance = Distance,
            Status = Status,
            CreatedBy = CreatedBy,
            CreatedDate = CreatedDate,
            LastModifiedDate = LastModifiedDate,
            IsActive = IsActive
        };
    }
}
