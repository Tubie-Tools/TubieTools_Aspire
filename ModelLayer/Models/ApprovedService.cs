namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Approved service in landing zone.
/// </summary>
public class ApprovedService
{
    public string ServiceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Service name</summary>
    public string ServiceName { get; set; }

    /// <summary>Service provider</summary>
    public string Provider { get; set; }

    /// <summary>Version approved</summary>
    public string ApprovedVersion { get; set; }

    /// <summary>Approval date</summary>
    public DateTime ApprovalDate { get; set; }

    /// <summary>Expiration date</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Service level agreement</summary>
    public string SLAAgreement { get; set; }

    /// <summary>Cost per unit/month</summary>
    public decimal? CostPerUnit { get; set; }
}
