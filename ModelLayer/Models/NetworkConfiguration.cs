namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

/// <summary>
/// Network configuration for landing zone.
/// </summary>
public class NetworkConfiguration
{
    /// <summary>Virtual network/VPC ID</summary>
    public string VirtualNetworkId { get; set; }

    /// <summary>Subnets</summary>
    public List<string> Subnets { get; set; } = new();

    /// <summary>Network security groups/firewall rules</summary>
    public List<NetworkRule> SecurityRules { get; set; } = new();

    /// <summary>DDoS protection enabled</summary>
    public bool DDoSProtectionEnabled { get; set; }

    /// <summary>WAF (Web Application Firewall) enabled</summary>
    public bool WAFEnabled { get; set; }

    /// <summary>VPN/ExpressRoute required</summary>
    public bool VPNRequired { get; set; }

    /// <summary>Egress filtering/proxy required</summary>
    public bool EgressFilteringRequired { get; set; }
}
