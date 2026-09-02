namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Capacity and scaling configuration.
/// </summary>
public class CapacityConfiguration
{
    /// <summary>Auto-scaling enabled</summary>
    public bool AutoScalingEnabled { get; set; }

    /// <summary>Min capacity</summary>
    public int MinCapacity { get; set; }

    /// <summary>Max capacity</summary>
    public int MaxCapacity { get; set; }

    /// <summary>Scale-up threshold (%)</summary>
    public decimal ScaleUpThreshold { get; set; }

    /// <summary>Scale-down threshold (%)</summary>
    public decimal ScaleDownThreshold { get; set; }

    /// <summary>Scale cooldown period (minutes)</summary>
    public int ScaleCooldownMinutes { get; set; }
}
