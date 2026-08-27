namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// AI Adoption roadmap with milestones
/// </summary>
public class AdoptionRoadmap
{
    public Dictionary<string, List<Milestone>> PhasesMilestones { get; set; } = new();
    public DateTime ProjectedCompletionDate { get; set; }
    public List<string> KeySuccesFactors { get; set; } = new();
    public List<string> CriticalDependencies { get; set; } = new();
}

#endregion
