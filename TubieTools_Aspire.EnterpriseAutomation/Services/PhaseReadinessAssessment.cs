namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Assessment of agent readiness for next phase
/// </summary>
public class PhaseReadinessAssessment
{
    public string AgentId { get; set; }
    public string CurrentPhase { get; set; }
    public string TargetPhase { get; set; }
    public bool IsReady { get; set; }
    public List<string> CompletedRequirements { get; set; } = new();
    public List<string> MissingRequirements { get; set; } = new();
    public int ReadinessPercentage { get; set; }
    public DateTime AssessmentDate { get; set; }
}

#endregion
