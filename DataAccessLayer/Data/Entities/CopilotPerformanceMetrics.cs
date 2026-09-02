using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Data.Entities;

/// <summary>
/// Entity model for Copilot Performance Metrics - monitoring and observability.
/// </summary>
[Table("CopilotPerformanceMetrics")]
public class CopilotPerformanceMetrics
{
    [Key]
    [StringLength(36)]
    public string MetricsId { get; set; } = Guid.NewGuid().ToString();

    [StringLength(36)]
    public string CopilotId { get; set; }

    // Average response time (milliseconds)
    public decimal AvgResponseTimeMs { get; set; }

    // P95 response time (milliseconds)
    public decimal P95ResponseTimeMs { get; set; }

    // P99 response time (milliseconds)
    public decimal P99ResponseTimeMs { get; set; }

    // Total invocations
    public long TotalInvocations { get; set; }

    // Successful invocations
    public long SuccessfulInvocations { get; set; }

    // Failed invocations
    public long FailedInvocations { get; set; }

    // Average tokens per response
    public decimal AvgTokensUsed { get; set; }

    // Total cost (in applicable currency)
    public decimal TotalCost { get; set; }

    // Average cost per invocation
    public decimal AvgCostPerInvocation { get; set; }

    // User satisfaction rating (0-100)
    public decimal UserSatisfactionRating { get; set; }

    // Error rate (0-100)
    public decimal ErrorRate { get; set; }

    // Uptime percentage (0-100)
    public decimal UptimePercentage { get; set; }

    // Last updated timestamp
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Detailed metrics as JSON
    public string DetailedMetrics { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    public double AverageResponseTime { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public double SuccessRate { get; set; }
}
