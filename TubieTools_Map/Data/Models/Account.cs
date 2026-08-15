using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TubieTools_Map.Data.Models;

[Table("Accounts")]
public class Account
{
    [Key]
    [StringLength(255)]
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(255)]
    public string FullName { get; set; } = "";

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(255)]
    public string? Organization { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(255)]
    public string? EntraObjectId { get; set; }
}