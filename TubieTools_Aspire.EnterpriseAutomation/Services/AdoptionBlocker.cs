namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Blockers preventing adoption progress
/// </summary>
public class AdoptionBlocker
{
    public string Phase { get; set; }
    public string BlockerDescription { get; set; }
    public string Impact { get; set; }
    public string RecommendedResolution { get; set; }
    public string AssignedTo { get; set; }
    public DateTime IdentifiedDate { get; set; }
}

#endregion
