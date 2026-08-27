namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// SLA compliance report
/// </summary>
public class SLAComplianceReport
{
    public string AgentId { get; set; }
    public int ReportingPeriodDays { get; set; }
    public decimal UptimePercentage { get; set; }
    public decimal UptimeSLATarget { get; set; }
    public bool MetUptimeSLA { get; set; }
    public decimal ErrorRatePercentage { get; set; }
    public decimal ErrorRateSLATarget { get; set; }
    public bool MetErrorRateSLA { get; set; }
    public decimal AverageLatencyMs { get; set; }
    public decimal LatencySLATargetMs { get; set; }
    public bool MetLatencySLA { get; set; }
    public List<string> SLABreaches { get; set; } = new();
}

#endregion
