using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TubieTools_Map.Data.Models;

[Table("Routes")]
public class MapRoute
{
    [Key]
    public string RouteId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string RouteName { get; set; } = "";

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = "";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active";

    public double DistanceKm { get; set; }

    public long EstimatedDurationTicks { get; set; }

    [NotMapped]
    public TimeSpan EstimatedDuration
    {
        get => new(EstimatedDurationTicks);
        set => EstimatedDurationTicks = value.Ticks;
    }

    [StringLength(50)]
    public string? VehicleType { get; set; } = "car";

    [Required]
    public string Waypoints { get; set; } = "[]";

    public ICollection<RouteSegment> Segments { get; set; } = new List<RouteSegment>();
    public ICollection<RouteRedirection> Redirections { get; set; } = new List<RouteRedirection>();
}

[Table("RouteSegments")]
public class RouteSegment
{
    [Key]
    public string SegmentId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string RouteId { get; set; } = "";

    public int SegmentIndex { get; set; }

    public double DistanceKm { get; set; }

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
}

[Table("RouteRedirections")]
public class RouteRedirection
{
    [Key]
    public string RedirectionId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    public string OriginalRouteId { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string Reason { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [StringLength(255)]
    public string? AlternativeRouteId { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey("OriginalRouteId")]
    public virtual MapRoute? OriginalRoute { get; set; }

    [ForeignKey("AlternativeRouteId")]
    public virtual MapRoute? AlternativeRoute { get; set; }
}