using ModelLayer.Models;

namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Landing zone configuration and guardrails.
/// </summary>
public class LandingZoneConfiguration
{
    public string LandingZoneId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Landing zone type</summary>
    public string LandingZoneType { get; set; }

    /// <summary>Landing zone name</summary>
    public string Name { get; set; }

    /// <summary>Landing zone description</summary>
    public string Description { get; set; }

    /// <summary>Business classification</summary>
    public string BusinessClassification { get; set; }

    /// <summary>Data classification level</summary>
    public string DataClassificationLevel { get; set; } // Public, Internal, Confidential, Restricted

    /// <summary>Regulatory requirements applicable</summary>
    public List<string> ApplicableRegulations { get; set; } = new();

    /// <summary>Governance policy</summary>
    public CopilotGovernancePolicy GovernancePolicy { get; set; }

    /// <summary>Network configuration</summary>
    public NetworkConfiguration NetworkConfig { get; set; }

    /// <summary>Identity and access management</summary>
    public IAMConfiguration IAMConfig { get; set; }

    /// <summary>Storage configuration</summary>
    public StorageConfiguration StorageConfig { get; set; }

    /// <summary>Monitoring and logging</summary>
    public MonitoringConfiguration MonitoringConfig { get; set; }

    /// <summary>Disaster recovery and backup</summary>
    public DRConfiguration DRConfig { get; set; }

    /// <summary>Capacity and scaling</summary>
    public CapacityConfiguration CapacityConfig { get; set; }

    /// <summary>Approved services/tools list</summary>
    public List<ApprovedService> ApprovedServices { get; set; } = new();

    /// <summary>Blocked services/tools list</summary>
    public List<string> BlockedServices { get; set; } = new();

    /// <summary>Cost budget for zone</summary>
    public decimal? MonthlyBudget { get; set; }

    /// <summary>Environment type</summary>
    public string EnvironmentType { get; set; } // Development, Testing, Staging, Production

    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public object? Id { get; set; }
    public object ZoneType { get; set; }
    public string CreatedBy { get; set; }
}
