using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

[Table("Routes")]
public class MapRoute
{
    [Key]
    public int Id { get; set; }

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
    public string Description { get; set; }
    public decimal Distance { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsActive { get; set; }
}
