using DataAccessLayer.Data.Entities;  

namespace DTOLayer.FacetMaps.MapApp;

/// <summary>
/// Facet mapping for RouteSegment entity.
/// </summary>
public class RouteSegmentFacetMap
{
    public int SegmentId { get; set; }
    public int RouteId { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public decimal SegmentDistance { get; set; }
    public int SequenceNumber { get; set; }
    public string? RoadType { get; set; }
    public int EstimatedDurationMinutes { get; set; }

    public static RouteSegmentFacetMap FromEntity(RouteSegment entity)
    {
        return new RouteSegmentFacetMap
        {
            SegmentId = entity.Id,
            RouteId = entity.RouteId,
            StartLatitude = entity.StartLatitude,
            StartLongitude = entity.StartLongitude,
            EndLatitude = entity.EndLatitude,
            EndLongitude = entity.EndLongitude,
            SegmentDistance = entity.DistanceKm,
            SequenceNumber = entity.SegmentIndex,
            RoadType = entity.RoadType,
            EstimatedDurationMinutes = (int)TimeSpan.FromTicks(entity.DurationTicks).TotalMinutes
        };
    }

    public RouteSegment ToEntity()
    {
        return new RouteSegment
        {
            Id = SegmentId,
            RouteId = RouteId,
            StartLatitude = StartLatitude,
            StartLongitude = StartLongitude,
            EndLatitude = EndLatitude,
            EndLongitude = EndLongitude,
            DistanceKm = SegmentDistance,
            SegmentIndex = SequenceNumber,
            RoadType = RoadType,
            DurationTicks = TimeSpan.FromMinutes(EstimatedDurationMinutes).Ticks
        };
    }
}
