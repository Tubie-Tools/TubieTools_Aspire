namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

public class Milestone
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime TargetDate { get; set; }
    public int Progress { get; set; }
    public string Owner { get; set; }
}

#endregion
