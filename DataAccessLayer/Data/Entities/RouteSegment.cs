using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

[Table("RouteSegments")]
public class RouteSegment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RouteId { get; set; }

    public int SegmentIndex { get; set; }

    public decimal DistanceKm { get; set; }

    public long DurationTicks { get; set; }

    [NotMapped]
    public TimeSpan Duration
    {
        get => new(DurationTicks);
        set => DurationTicks = value.Ticks;
    }

    public string? Coordinates { get; set; }

    [ForeignKey("RouteId")]
    public virtual MapRoute? Route { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public string RoadType { get; set; }
}
