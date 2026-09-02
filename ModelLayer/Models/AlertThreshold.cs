namespace TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

public class AlertThreshold
{
    public string MetricName { get; set; }
    public string Operator { get; set; } // GreaterThan, LessThan, Equals
    public decimal Threshold { get; set; }
    public string Severity { get; set; }
}
