namespace ModelLayer.Models;

/// <summary>
/// Data residency requirements by landing zone.
/// </summary>
public class DataResidencyRequirements
{
    /// <summary>Allowed geographic regions</summary>
    public List<string> AllowedRegions { get; set; } = new();

    /// <summary>Data must remain in country/region</summary>
    public bool DataLocalizationRequired { get; set; }

    /// <summary>Approved data centers</summary>
    public List<string> ApprovedDataCenters { get; set; } = new();

    /// <summary>Backup region requirements</summary>
    public string BackupRegionRequirement { get; set; }

    /// <summary>Latency SLA (milliseconds)</summary>
    public int? LatencySLAMs { get; set; }

    /// <summary>Disaster recovery region restrictions</summary>
    public string DRRegionRestriction { get; set; }
}
