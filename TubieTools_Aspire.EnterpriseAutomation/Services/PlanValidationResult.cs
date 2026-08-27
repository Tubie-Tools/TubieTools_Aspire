namespace TubieTools_Aspire.EnterpriseAutomation.Services;
#region Supporting Models for Service Operations

/// <summary>
/// Results of plan validation
/// </summary>
public class PlanValidationResult
{
    public string AgentId { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationPassed { get; set; } = new();
    public List<string> ValidationFailed { get; set; } = new();
    public List<string> RecommendedImprovements { get; set; } = new();
    public int CompletionPercentage { get; set; }
}

#endregion
