namespace TubieTools_Aspire.EnterpriseAutomation.Services;

using TubieTools_Aspire.EnterpriseAutomation.Models;
#region Supporting Models for Service Operations

/// <summary>
/// Report for phase transition
/// </summary>
public class PhaseTransitionReport
{
    public string AgentId { get; set; }
    public string FromPhase { get; set; }
    public string ToPhase { get; set; }
    public DateTime TransitionDate { get; set; }
    public string TransitionStatus { get; set; }
    public List<string> ObservationsAndRecommendations { get; set; } = new();
    public List<RiskAssessment> IdentifiedRisks { get; set; } = new();
}

#endregion
