namespace ModelLayer.Models;

public class SeverityLevel
{
    public string Level { get; set; } // Critical, High, Medium, Low
    public string Description { get; set; }
    public int ResponseTimeMinutes { get; set; }
}
