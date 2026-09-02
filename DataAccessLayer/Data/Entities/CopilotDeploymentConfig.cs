using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Copilot Deployment Configuration - environment and infrastructure settings.
/// </summary>
[Table("CopilotDeploymentConfigs")]
public class CopilotDeploymentConfig
{
    [Key]
    [StringLength(36)]
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    [StringLength(36)]
    public string CopilotId { get; set; }

    [Required]
    [StringLength(50)]
    public string Environment { get; set; }

    [StringLength(500)]
    public string DeploymentEndpoint { get; set; }

    [StringLength(100)]
    public string DeploymentRegion { get; set; }

    [StringLength(100)]
    public string ContainerRegistry { get; set; }

    [StringLength(100)]
    public string ImageTag { get; set; }

    // Scaling configuration (JSON)
    public string ScalingConfig { get; set; }

    // Resource allocation (JSON)
    public string ResourceAllocation { get; set; }

    // Health check configuration (JSON)
    public string HealthCheck { get; set; }

    // Load balancing configuration (JSON)
    public string LoadBalancing { get; set; }

    // SSL/TLS configuration (JSON)
    public string SecurityConfig { get; set; }

    // Environment variables (encrypted JSON)
    public string EnvironmentVariables { get; set; }

    // Deployment status (Pending, InProgress, Active, Failed, Rolled Back, etc.)
    [StringLength(50)]
    public string DeploymentStatus { get; set; }

    public DateTime DeployedDate { get; set; }

    public DateTime LastHealthCheckDate { get; set; }

    // Rollback information (JSON)
    public string RollbackInfo { get; set; }

    // Feature flags (JSON)
    public string FeatureFlags { get; set; }

    public bool IsProductionReady { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }
}
