using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

[Table("RouteRedirections")]
public class RouteRedirection
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public int OriginalRouteId { get; set; }

    [Required]
    [StringLength(100)]
    public string Reason { get; set; } = "";

    [Required]
    public string Description { get; set; } = "";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [StringLength(255)]
    public int? AlternativeRouteId { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey("OriginalRouteId")]
    public virtual MapRoute? OriginalRoute { get; set; }

    [ForeignKey("AlternativeRouteId")]
    public virtual MapRoute? AlternativeRoute { get; set; }
    public DateTime RedirectionDate { get; set; }
    public string CreatedBy { get; set; }
}
