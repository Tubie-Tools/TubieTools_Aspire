namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

public class NetworkRule
{
    public string Direction { get; set; } // Inbound, Outbound
    public string Protocol { get; set; } // TCP, UDP, ICMP
    public string SourceAddress { get; set; }
    public int? SourcePort { get; set; }
    public string DestinationAddress { get; set; }
    public int? DestinationPort { get; set; }
    public string Action { get; set; } // Allow, Deny
    public int Priority { get; set; }
}
