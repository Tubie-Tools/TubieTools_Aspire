using DataAccessLayer.Data.Entities;

namespace DTOLayer.FacetMaps.MapApp;

/// <summary>
/// Facet mapping for RouteRedirection entity.
/// </summary>
public class RouteRedirectionFacetMap
{
    public int RedirectionId { get; set; }
    public int OriginalRouteId { get; set; }
    public int? AlternativeRouteId { get; set; }
    public string? Reason { get; set; }
    public DateTime RedirectionDate { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }

    public static RouteRedirectionFacetMap FromEntity(RouteRedirection entity)
    {
        return new RouteRedirectionFacetMap
        {
            RedirectionId = entity.Id,
            OriginalRouteId = entity.OriginalRouteId,
            AlternativeRouteId = entity.AlternativeRouteId,
            Reason = entity.Reason,
            RedirectionDate = entity.RedirectionDate,
            IsActive = entity.IsActive,
            CreatedBy = entity.CreatedBy
        };
    }

    public RouteRedirection ToEntity()
    {
        return new RouteRedirection
        {
            Id = RedirectionId,
            OriginalRouteId = OriginalRouteId,
            AlternativeRouteId = AlternativeRouteId,
            Reason = Reason,
            RedirectionDate = RedirectionDate,
            IsActive = IsActive,
            CreatedBy = CreatedBy
        };
    }
}
