using DataAccessLayer.Data.Entities;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace DTOLayer.FacetMaps.CopilotStudio;

/// <summary>
/// Facet mapping for CopilotPerformanceMetrics entity.
/// </summary>
public class CopilotPerformanceMetricsFacetMap
{
    public string? MetricsId { get; set; }
    public string? CopilotId { get; set; }
    public double AverageResponseTime { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastUpdated { get; set; }

    public static CopilotPerformanceMetricsFacetMap FromEntity(CopilotPerformanceMetrics entity)
    {
        return new CopilotPerformanceMetricsFacetMap
        {
            MetricsId = entity.MetricsId,
            CopilotId = entity.CopilotId,
            AverageResponseTime = entity.AverageResponseTime,
            TotalRequests = entity.TotalRequests,
            SuccessfulRequests = entity.SuccessfulRequests,
            SuccessRate = entity.SuccessRate,
            LastUpdated = entity.LastUpdated
        };
    }

    public CopilotPerformanceMetrics ToEntity()
    {
        return new CopilotPerformanceMetrics
        {
            MetricsId = MetricsId,
            CopilotId = CopilotId,
            AverageResponseTime = AverageResponseTime,
            TotalRequests = TotalRequests,
            SuccessfulRequests = SuccessfulRequests,
            SuccessRate = SuccessRate,
            LastUpdated = LastUpdated
        };
    }
}
